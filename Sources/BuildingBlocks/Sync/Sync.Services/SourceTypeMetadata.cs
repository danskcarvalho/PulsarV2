using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD;
using static Pulsar.BuildingBlocks.Utils.GeneralExtensions;
using System.Reflection;
using System.Text.Json;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.Syncing.DDD;
using System.Collections;
using Amazon.Auth.AccessControlPolicy;
using AutoMapper;

namespace Pulsar.BuildingBlocks.Sync.Services;

class SourceTypeMetadata
{
    private static readonly HashSet<Type> PrimitiveTypes = [typeof(int), typeof(long), typeof(byte), typeof(string), typeof(bool), typeof(char), typeof(double), typeof(float), typeof(decimal), typeof(DateTime), typeof(TimeSpan), typeof(DateOnly),
            typeof(ObjectId), typeof(Guid)];
    private readonly Dictionary<Type, Map> _mappers = new Dictionary<Type, Map>();
    private Type _shadowType, _sourceType;
    private string _shadowEntityName;

    public Type ShadowType => _shadowType;
    public Type SourceType => _sourceType;
    public string ShadowEntityName => _shadowEntityName;

    public object ToShadow(object source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (!_sourceType.IsAssignableFrom(source.GetType()))
        {
            throw new ArgumentException($"source has invalid type {_sourceType.FullName}");
        }

        var mapper = GetMapper(source.GetType(), _shadowType);
        var shadow = mapper.Mapper.Map(source, source.GetType(), _shadowType) as Shadow;
        var root = source as AggregateRoot;

        shadow!.CopyIdVersion(root!);
        shadow!.TimeStamp = DateTime.UtcNow;

        InterceptShadow(source, shadow);

        return shadow;
    }

    private Map GetMapper(Type from, Type to)
    {
        lock (_mappers)
        {
            if (_mappers.TryGetValue(from, out var mapper))
            {
                return mapper;
            }
            else
            {
                _mappers[from] = Map.Create(from, to);
                return _mappers[from];
            }
        }
    }

    public SourceTypeMetadata(Type sourceType)
    {
        _sourceType = sourceType;
        _shadowType = sourceType.GetCustomAttribute<TrackChangesAttribute>()?.ShadowType ?? throw new ArgumentException("no TrackChangesAttribute");
        if (!HasParameterlessConstructor(_shadowType))
        {
            throw new InvalidOperationException($"shadow type {_shadowType.FullName} has no parameterless constructor");
        }
        var shadowAttr = _shadowType.GetCustomAttribute<ShadowAttribute>();
        if (shadowAttr == null)
        {
            throw new InvalidOperationException($"shadow type {_shadowType.FullName} has no ShadowAttribute");
        }
        _shadowEntityName = shadowAttr.Name;
    }

    #region [ AutoMapper ]
    abstract class Map
    {
        public static Map Create(Type from, Type to)
        {
            return (Map)(Activator.CreateInstance(typeof(Map<,>).MakeGenericType(from, to)) ?? throw new InvalidOperationException("could not instantiate mapper"));
        }

        public abstract IMapper Mapper { get; }
        protected static void CreateMapping<F, T>(IMapperConfigurationExpression cfg, HashSet<(Type From, Type To)> addedMappings)
        {
            if(typeof(T) == typeof(F))
            {
                return;
            }

            if (addedMappings.Contains((typeof(F), typeof(T))))
            {
                return;
            }
            addedMappings.Add((typeof(F), typeof(T)));

            var start = cfg.CreateMap<F, T>();
            foreach (var fprop in typeof(F).GetProperties())
            {
                var tprop = typeof(T).GetProperty(fprop.Name);
                var attr = fprop.GetCustomAttribute<TrackChangesAttribute>();
                if (tprop == null)
                {
                    continue;
                }
                if (attr == null || !IsValidType(fprop.PropertyType) || !IsValidType(tprop.PropertyType))
                {
                    start = start.ForMember(fprop.Name, opt => opt.Ignore());
                    continue;
                }

                if (IsNullableOrPrimitiveType(fprop.PropertyType) && IsNullableOrPrimitiveType(tprop.PropertyType))
                {
                    if(fprop.PropertyType == tprop.PropertyType)
                    {

                    }
                    else
                    {
                        start = start.ForMember(fprop.Name, opt => opt.Ignore());
                        continue;
                    }
                }
                else if (IsCollectionOfPrimitivesTypes(fprop.PropertyType) && IsCollectionOfPrimitivesTypes(tprop.PropertyType))
                {
                    if (fprop.PropertyType == tprop.PropertyType)
                    {

                    }
                    else
                    {
                        start = start.ForMember(fprop.Name, opt => opt.Ignore());
                        continue;
                    }
                }
                else if (IsCollectionOfComplexType(fprop.PropertyType) && IsCollectionOfComplexType(tprop.PropertyType))
                {
                    CreateMappingNonGeneric(GetComplexTypeFromCollection(fprop.PropertyType), GetComplexTypeFromCollection(tprop.PropertyType), cfg, addedMappings);
                }
                else if (IsComplexType(fprop.PropertyType) && IsComplexType(tprop.PropertyType))
                {
                    CreateMappingNonGeneric(fprop.PropertyType, tprop.PropertyType, cfg, addedMappings);
                }
                else
                {
                    start = start.ForMember(fprop.Name, opt => opt.Ignore());
                }
            }
        }

        private static void CreateMappingNonGeneric(Type from, Type to, IMapperConfigurationExpression cfg, HashSet<(Type From, Type To)> addedMappings)
        {
            typeof(Map).GetMethod("CreateMapping", BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(from, to).Invoke(null, [cfg, addedMappings]);
        }

        private static Type GetComplexTypeFromCollection(Type type)
        {
            if (type.IsConstructedGenericType)
            {
                var td = type.GetGenericTypeDefinition();
                if (td == typeof(List<>))
                {
                    var arg = type.GetGenericArguments()[0];
                    if (arg.IsConstructedGenericType)
                    {
                        var td2 = arg.GetGenericTypeDefinition();
                        if (td2 == typeof(List<>))
                            return GetComplexTypeFromCollection(arg);
                        else if (td2 == typeof(Dictionary<,>))
                            return GetComplexTypeFromCollection(arg);
                        else
                            return arg;
                    }
                    else
                        return arg;
                }
                else if (td == typeof(Dictionary<,>))
                {
                    var arg = type.GetGenericArguments()[1];

                    if (arg.IsConstructedGenericType)
                    {
                        var td2 = arg.GetGenericTypeDefinition();
                        if (td2 == typeof(List<>))
                            return GetComplexTypeFromCollection(arg);
                        else if (td2 == typeof(Dictionary<,>))
                            return GetComplexTypeFromCollection(arg);
                        else
                            return arg;
                    }
                    else
                        return arg;
                }
                else
                    throw new InvalidOperationException();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
    class Map<TFrom, TTo> : Map
    {
        private MapperConfiguration _config;

        public override IMapper Mapper => _config.CreateMapper();

        public Map()
        {
            _config = new MapperConfiguration(cfg =>
            {
                cfg.ShouldMapField = f => false;
                HashSet<(Type From, Type To)> addedMappings = new HashSet<(Type From, Type To)>();

                CreateMapping<TFrom, TTo>(cfg, addedMappings);
            });
        }
    }
    #endregion

    #region [ Tests ]
    private static bool IsValidType(Type type)
    {
        return IsNullableOrPrimitiveType(type) || IsCollectionOfPrimitivesTypes(type) || IsCollectionOfComplexType(type) || IsComplexType(type);
    }
    private static bool HasParameterlessConstructor(Type type)
    {
        var constructor = type.GetConstructor(Type.EmptyTypes);
        return constructor != null && constructor.IsPublic;
    }
    private static bool IsComplexType(Type type)
    {
        foreach (var prop in type.GetProperties())
        {
            var attr = prop.GetCustomAttribute<TrackChangesAttribute>();
            if (attr == null)
            {
                continue;
            }

            if (IsNullableOrPrimitiveType(prop.PropertyType))
            {
                continue;
            }
            if (IsCollectionOfPrimitivesTypes(prop.PropertyType))
            {
                continue;
            }
            if (IsCollectionOfComplexType(prop.PropertyType))
            {
                continue;
            }
            if (IsComplexType(prop.PropertyType))
            {
                continue;
            }

            return false;
        }

        return true;
    }
    private static bool IsNullableType(Type type)
    {
        return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsPrimitiveType(type.GetGenericArguments()[0]);
    }
    private static bool IsNullableOrPrimitiveType(Type type)
    {
        return IsNullableType(type) || IsPrimitiveType(type);
    }
    private static bool IsCollectionOfPrimitivesTypes(Type type)
    {
        if (type.IsConstructedGenericType)
        {
            var td = type.GetGenericTypeDefinition();
            if (td == typeof(List<>))
            {
                return IsNullableOrPrimitiveType(type.GetGenericArguments()[0]) || IsCollectionOfPrimitivesTypes(type.GetGenericArguments()[0]);
            }
            else if (td == typeof(Dictionary<,>))
            {
                return (IsNullableOrPrimitiveType(type.GetGenericArguments()[0]) || IsCollectionOfPrimitivesTypes(type.GetGenericArguments()[0])) &&
                       (IsNullableOrPrimitiveType(type.GetGenericArguments()[1]) || IsCollectionOfPrimitivesTypes(type.GetGenericArguments()[1]));
            }
            else if (td == typeof(HashSet<>))
            {
                return IsNullableOrPrimitiveType(type.GetGenericArguments()[0]) || IsCollectionOfPrimitivesTypes(type.GetGenericArguments()[0]);
            }
            else
                return false;
        }
        else
        {
            return false;
        }
    }
    private static bool IsCollectionOfComplexType(Type type)
    {
        if (type.IsConstructedGenericType)
        {
            var td = type.GetGenericTypeDefinition();
            if (td == typeof(List<>))
            {
                return IsComplexType(type.GetGenericArguments()[0]) || IsCollectionOfComplexType(type.GetGenericArguments()[0]);
            }
            else if (td == typeof(Dictionary<,>))
            {
                return IsNullableOrPrimitiveType(type.GetGenericArguments()[0]) && (IsComplexType(type.GetGenericArguments()[1]) || IsCollectionOfPrimitivesTypes(type.GetGenericArguments()[1]));
            }
            else
                return false;
        }
        else
        {
            return false;
        }
    }
    private static bool IsPrimitiveType(Type type)
    {
        return type.IsEnum || PrimitiveTypes.Contains(type);
    }

    #endregion

    #region [ Interception ]
    private void InterceptShadow(object source, object shadow)
    {
        var intercept = Activator.CreateInstance(typeof(ShadowInterception<,>).MakeGenericType(_sourceType, _shadowType)) as ShadowInterception;
        if (intercept == null)
        {
            throw new InvalidOperationException();
        }
        intercept.Intercept(source, shadow);
    }

    abstract class ShadowInterception
    {
        abstract public void Intercept(object source, object shadow);
    }
    class ShadowInterception<TSource, TShadow> : ShadowInterception where TShadow : class, IShadow where TSource : class
    {
        void Intercept(TSource source, TShadow shadow)
        {
            if (source is IInterceptShadow<TShadow> interceptShadow)
            {
                interceptShadow.Intercept(shadow);
            }
        }

        public override void Intercept(object source, object shadow)
        {
            Intercept((TSource)source, (TShadow)shadow);
        }
    }
    #endregion
}
