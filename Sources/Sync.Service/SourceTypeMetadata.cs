using MongoDB.Bson;
using Pulsar.BuildingBlocks.DDD;
using static Pulsar.BuildingBlocks.Utils.GeneralExtensions;
using System.Reflection;
using System.Text.Json;
using Pulsar.BuildingBlocks.Sync.Contracts;
using Pulsar.BuildingBlocks.Syncing.DDD;
using System.Collections;
using Amazon.Auth.AccessControlPolicy;

namespace Pulsar.BuildingBlocks.Sync.Service;

class SourceTypeMetadata
{
    private static readonly HashSet<Type> PrimitiveTypes = [typeof(int), typeof(long), typeof(byte), typeof(string), typeof(bool), typeof(char), typeof(double), typeof(float), typeof(decimal), typeof(DateTime), typeof(TimeSpan), typeof(DateOnly),
            typeof(ObjectId), typeof(Guid)];
    private readonly Dictionary<(Type SourceType, Type DestType), Func<object?, object?>> _converters = new Dictionary<(Type SourceType, Type DestType), Func<object?, object?>>();
    private readonly List<SourceTypeMetadataProperty> _propertiesMetadata = new List<SourceTypeMetadataProperty>();
    private Type _shadowType, _sourceType;
    private string _shadowEntityName;

    public Type ShadowType => _shadowType;
    public Type SourceType => _sourceType;
    public string ShadowEntityName => _shadowEntityName;

    public object ToShadow(object source)
    {
        if(source == null) throw new ArgumentNullException(nameof(source));
        if (!_sourceType.IsAssignableFrom(source.GetType()))
        {
            throw new ArgumentException($"source has invalid type {_sourceType.FullName}");
        }
        var shadow = Activator.CreateInstance(_shadowType);

        if (shadow == null)
        {
            throw new InvalidOperationException("could not create shadow");
        }

        foreach (var propMetadata in _propertiesMetadata)
        {
            var sourceValue = propMetadata.SourceProperty.GetValue(source);
            var destValue = propMetadata.ConverterFromSourceToShadow(sourceValue);
            propMetadata.ShadowProperty.SetValue(shadow, destValue);
        }

        return shadow;
    }

    public SourceTypeMetadata(Type sourceType)
    {
        _sourceType = sourceType;
        _shadowType = sourceType.GetCustomAttribute<TrackChangesAttribute>()?.ShadowType ?? throw new ArgumentException("no TrackChangesAttribute");
        if(!HasParameterlessConstructor(_shadowType))
        {
            throw new InvalidOperationException($"shadow type {_shadowType.FullName} has no parameterless constructor");
        }
        _propertiesMetadata = GetProperties(sourceType, _shadowType).ToList();
        var shadowAttr = _shadowType.GetCustomAttribute<ShadowAttribute>();
        if (shadowAttr == null)
        {
            throw new InvalidOperationException($"shadow type {_shadowType.FullName} has no ShadowAttribute");
        }
        _shadowEntityName = shadowAttr.Name;
    }

    private IEnumerable<SourceTypeMetadataProperty> GetProperties(Type sourceType, Type shadowType)
    {
        if (!HasParameterlessConstructor(shadowType))
        {
            throw new InvalidOperationException("shadow type must be parameterless constructor");
        }

        foreach (var property in sourceType.GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
        {
            var attribute = property.GetCustomAttribute<TrackChangesAttribute>();
            if (attribute == null)
            {
                continue;
            }

            var shadowProperty = shadowType.GetProperty(property.Name);
            if (shadowProperty == null)
            {
                throw new InvalidOperationException($"{sourceType} and {shadowType} are not compatible");
            }
            if (!AreTypesCompatible(property.PropertyType, shadowProperty.PropertyType))
            {
                throw new InvalidOperationException($"{sourceType} and {shadowType} are not compatible");
            }

            yield return new SourceTypeMetadataProperty(property, shadowProperty, CreateConverter(property.PropertyType, shadowProperty.PropertyType));
        }
    }

    private Func<object?, object?> CreateConverter(Type sourceType, Type destType)
    {
        lock (_converters)
        {
            var key = (sourceType, destType);
            if(_converters.TryGetValue(key, out var converter))
            {
                return converter;
            }
            else
            {
                var result = SlowCreateConverter(sourceType, destType);
                _converters[key] = result;
                return result;
            }
        }
    }

    private Func<object?, object?> SlowCreateConverter(Type sourceType, Type destType)
    {
        if (sourceType == destType)
        {
            return obj => obj;
        }
        else
        {
            return obj =>
            {
                var result = Activator.CreateInstance(destType);
                foreach (var property in sourceType.GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
                {
                    var attribute = property.GetCustomAttribute<TrackChangesAttribute>();
                    if (attribute == null)
                    {
                        continue;
                    }

                    var destProperty = destType.GetProperty(property.Name);
                    if (destProperty == null)
                    {
                        throw new InvalidOperationException();
                    }

                    var sourceValue = property.GetValue(obj, null);
                    destProperty.SetValue(result, CreateConverter(property.PropertyType, destProperty.PropertyType)(sourceValue));
                }
                return result;
            };
        }
    }

    private static bool HasParameterlessConstructor(Type type)
    {
        var constructor = type.GetConstructor(Type.EmptyTypes);
        return constructor != null && constructor.IsPublic;
    }

    // TODO: Deal with possible infinite recursion
    private static bool AreTypesCompatible(Type sourceType, Type shadowType)
    {
        if (shadowType == sourceType)
        {
            return true;
        }

        if (!HasParameterlessConstructor(shadowType))
        {
            return false;
        }

        if (shadowType.IsClass && !shadowType.IsEnum && !shadowType.IsValueType && !PrimitiveTypes.Contains(shadowType) &&
            sourceType.IsClass && !sourceType.IsEnum && !sourceType.IsValueType && !PrimitiveTypes.Contains(sourceType))
        {
            foreach (var property in sourceType.GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
            {
                var attribute = property.GetCustomAttribute<TrackChangesAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                var shadowProperty = shadowType.GetProperty(property.Name);
                if (shadowProperty == null)
                {
                    return false;
                }

                if (!AreTypesCompatible(property.PropertyType, shadowProperty.PropertyType))
                {
                    return false;
                }
            }
        }

        return false;
    }

    private static bool IsValidPropertyType(Type propertyType)
    {
        return IsPrimitiveTypeOrValueObject(propertyType) || IsCollectionOfPrimitivesOrValueObjects(propertyType) || IsNullableOfPrimitiveType(propertyType);
    }

    private static bool IsNullableOfPrimitiveType(Type propertyType)
    {
        return propertyType.IsConstructedGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && IsPrimitiveTypeOrValueObject(propertyType.GetGenericArguments()[0]);
    }

    private static bool IsCollectionOfPrimitivesOrValueObjects(Type propertyType)
    {
        if (propertyType.IsConstructedGenericType)
        {
            var td = propertyType.GetGenericTypeDefinition();
            if (td == typeof(List<>))
            {
                return IsNullableOfPrimitiveType(propertyType.GetGenericArguments()[0]);
            }
            else if (td == typeof(Dictionary<,>))
            {
                return IsNullableOfPrimitiveType(propertyType.GetGenericArguments()[0]) && IsNullableOfPrimitiveType(propertyType.GetGenericArguments()[1]);
            }
            else if (td == typeof(HashSet<>))
            {
                return IsNullableOfPrimitiveType(propertyType.GetGenericArguments()[0]);
            }
            else
                return false;
        }
        else
        {
            return false;
        }
    }

    private static bool IsPrimitiveTypeOrValueObject(Type propertyType)
    {
        return typeof(ValueObject).IsAssignableFrom(propertyType) || propertyType.IsEnum || PrimitiveTypes.Contains(propertyType);
    }

    record SourceTypeMetadataProperty(PropertyInfo SourceProperty, PropertyInfo ShadowProperty, Func<object?, object?> ConverterFromSourceToShadow);

}
