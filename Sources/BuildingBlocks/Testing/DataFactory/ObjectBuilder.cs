using MongoDB.Bson;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace Pulsar.BuildingBlocks.DataFactory
{
    public class ObjectBuilder<T> : ObjectBuilderBase where T : class
    {
        private Random _rng;
        private Generator<T> _generator;
        private BuilderRoot _parent;
        private Func<object?, object?, object?, T>? _builder;
        private Type[] _argTypes = new Type[0];
        private bool _autocomplete = false;
        private List<object?>? _autocompleteArgs = null;
        private bool _autofill = true;
        private static readonly HashSet<PropertyInfo> _emptyProperties = new HashSet<PropertyInfo>();
        private static readonly HashSet<Type> _canBeGenerated = new HashSet<Type>
        {
            typeof(Guid), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(char), typeof(string), typeof(bool), 
            typeof(decimal), typeof(DateTime), typeof(DateOnly), typeof(TimeOnly), typeof(TimeSpan), typeof(char), typeof(ObjectId),
            typeof(Guid?), typeof(byte?), typeof(short?), typeof(int?), typeof(long?), typeof(char?), typeof(bool?), typeof(decimal?), 
            typeof(DateTime?), typeof(DateOnly?), typeof(TimeOnly?), typeof(TimeSpan?), typeof(char?), typeof(ObjectId?)
        };


        internal ObjectBuilder(Random random, BuilderRoot parent)
        {
            this._rng = random;
            this._generator = new Generator<T>(_rng);
            this._parent = parent;
        }
        public ObjectBuilder<T> AutoComplete()
        {
            _autocomplete = true;
            _autocompleteArgs = new List<object?>();
            _parent.RegisterForAutoComplete(typeof(T), this);
            return this;
        }

        public ObjectBuilder<T> AutoComplete(object? arg1)
        {
            _autocomplete = true;
            _autocompleteArgs = new List<object?> { arg1 };
            _parent.RegisterForAutoComplete(typeof(T), this);
            return this;
        }

        public ObjectBuilder<T> AutoComplete(object? arg1, object? arg2)
        {
            _autocomplete = true;
            _autocompleteArgs = new List<object?> { arg1, arg2 };
            _parent.RegisterForAutoComplete(typeof(T), this);
            return this;
        }

        public ObjectBuilder<T> AutoComplete(object? arg1, object? arg2, object? arg3)
        {
            _autocomplete = true;
            _autocompleteArgs = new List<object?> { arg1, arg2, arg3 };
            _parent.RegisterForAutoComplete(typeof(T), this);
            return this;
        }

        public BuildFunction<T> AutoRecipe(Expression<Func<T, object?>>? include = null, Expression<Func<T, object?>>? exclude = null)
        {
            if (_autocomplete && _autocompleteArgs!.Count != 0)
                throw new InvalidOperationException("cannot autocomplete with 0 arguments as options");
            if (_builder != null)
                throw new InvalidOperationException("cannot set recipe twice");

            var result = (BuildFunction<T>)(() =>
            {
                return _parent.UpOneLevel<T>(() =>
                {
                    var obj = (T)(Activator.CreateInstance(typeof(T)) ?? throw new InvalidOperationException());
                    SetUpProperties(obj, GetPropertiesFromExpression(include), GetPropertiesFromExpression(exclude));
                    return obj;
                });
            });
            _builder = (a1, a2, a3) => result();
            SetGenerator(result, _generator);
            return result;
        }

        private HashSet<PropertyInfo> GetPropertiesFromExpression(Expression<Func<T, object?>>? properties)
        {
            if (properties == null)
                return _emptyProperties;

            var body = properties.Body;
            if (body is UnaryExpression ue && ue.NodeType == ExpressionType.Convert)
                body = ue.Operand;
            var target = properties.Parameters[0];
            var result = new HashSet<PropertyInfo>();
            if (body is MemberExpression me && me.Member is PropertyInfo pi && me.Expression == target)
            {
                result.Add(pi);
            }
            else if (body is NewExpression ne)
            {
                foreach (var arg in ne.Arguments)
                {
                    if (arg is MemberExpression me2 && me2.Member is PropertyInfo pi2 && me2.Expression == target)
                    {
                        result.Add(pi2);
                    }
                    else
                        throw new InvalidOperationException($"unknown expression {properties}");
                }
            }
            else
                throw new InvalidOperationException($"unknown expression {properties}");

            return result;
        }

        public BuildFunction<T> Recipe(Expression<Func<Generator<T>, T>> generator, Expression<Func<T, object?>>? include = null, Expression<Func<T, object?>>? exclude = null)
        {
            if (_autocomplete && _autocompleteArgs!.Count != 0)
                throw new InvalidOperationException("cannot autocomplete with 0 arguments as options");
            if (_builder != null)
                throw new InvalidOperationException("cannot set recipe twice");

            var compiled = generator.Compile();
            var result = (BuildFunction<T>)(() =>
            {
                return _parent.UpOneLevel<T>(() =>
                {
                    var obj = compiled(_generator);
                    var ignoredProperties = GetIgnoredProperties(generator);
                    ignoredProperties.UnionWith(GetPropertiesFromExpression(exclude));
                    SetUpProperties(obj, GetPropertiesFromExpression(include), ignoredProperties);
                    return obj;
                });
            });
            _builder = (a1, a2, a3) => result();
            SetGenerator(result, _generator);
            return result;
        }

        public BuildFunction<T, TArg1, TArg2, TArg3> Recipe<TArg1, TArg2, TArg3>(Expression<Func<Generator<T>, Func<TArg1, TArg2, TArg3, T>>> generator, 
            Expression<Func<T, object?>>? include = null, Expression<Func<T, object?>>? exclude = null)
        {
            if (_autocomplete && _autocompleteArgs!.Count != 3)
                throw new InvalidOperationException("cannot autocomplete with 3 arguments as options");
            if (_builder != null)
                throw new InvalidOperationException("cannot set recipe twice");

            var compiled = generator.Compile();
            var result = (BuildFunction<T, TArg1, TArg2, TArg3>)((a1, a2, a3) =>
            {
                return _parent.UpOneLevel<T>(() =>
                {
                    var obj = compiled(_generator)(a1, a2, a3);
                    var ignoredProperties = GetIgnoredProperties(generator);
                    ignoredProperties.UnionWith(GetPropertiesFromExpression(exclude));
                    SetUpProperties(obj, GetPropertiesFromExpression(include), ignoredProperties);
                    return obj;
                });
            });
            _builder = (a1, a2, a3) => result((TArg1)a1!, (TArg2)a2!, (TArg3)a3!);
            _argTypes = new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) };
            SetGenerator(result, _generator);
            return result;
        }

        public BuildFunction<T, TArg1, TArg2> Recipe<TArg1, TArg2>(Expression<Func<Generator<T>, Func<TArg1, TArg2, T>>> generator, 
            Expression<Func<T, object?>>? include = null, Expression<Func<T, object?>>? exclude = null)
        {
            if (_autocomplete && _autocompleteArgs!.Count != 2)
                throw new InvalidOperationException("cannot autocomplete with 2 arguments as options");
            if (_builder != null)
                throw new InvalidOperationException("cannot set recipe twice");

            var compiled = generator.Compile();
            var result = (BuildFunction<T, TArg1, TArg2>)((a1, a2) =>
            {
                return _parent.UpOneLevel<T>(() =>
                {
                    var obj = compiled(_generator)(a1, a2);
                    var ignoredProperties = GetIgnoredProperties(generator);
                    ignoredProperties.UnionWith(GetPropertiesFromExpression(exclude));
                    SetUpProperties(obj, GetPropertiesFromExpression(include), ignoredProperties);
                    return obj;
                });
            });
            _builder = (a1, a2, a3) => result((TArg1)a1!, (TArg2)a2!);
            _argTypes = new Type[] { typeof(TArg1), typeof(TArg2) };
            SetGenerator(result, _generator);
            return result;
        }
        public BuildFunction<T, TArg1> Recipe<TArg1>(Expression<Func<Generator<T>, Func<TArg1, T>>> generator, Expression<Func<T, object?>>? include = null, Expression<Func<T, object?>>? exclude = null)
        {
            if (_autocomplete && _autocompleteArgs!.Count != 1)
                throw new InvalidOperationException("cannot autocomplete with 1 argument as an option");
            if (_builder != null)
                throw new InvalidOperationException("cannot set recipe twice");

            var compiled = generator.Compile();
            var result = (BuildFunction<T, TArg1>)((a1) =>
            {
                return _parent.UpOneLevel<T>(() =>
                {
                    var obj = compiled(_generator)(a1);
                    var ignoredProperties = GetIgnoredProperties(generator);
                    ignoredProperties.UnionWith(GetPropertiesFromExpression(exclude));
                    SetUpProperties(obj, GetPropertiesFromExpression(include), ignoredProperties);
                    return obj;
                });
            });
            _builder = (a1, a2, a3) => result((TArg1)a1!);
            _argTypes = new Type[] { typeof(TArg1) };
            SetGenerator(result, _generator);
            return result;
        }

        private void SetUpProperties(T obj, HashSet<PropertyInfo> included, HashSet<PropertyInfo> excluded)
        {
            if (!_autofill)
                return;
            var objType = obj.GetType();
            if(included == null || included.Count == 0)
                included = new HashSet<PropertyInfo>(objType.GetProperties());
            foreach (var prop in included)
            {
                if (excluded.Contains(prop))
                    continue;
                if (IsSupportedCollection(prop.PropertyType, out var itemType, out var addFunction, out var createFunction) && CanBeGenerated(itemType!))
                {
                    var collection = prop.GetValue(obj);
                    if (collection != null)
                    {
                        var size = _rng.Next(0, 100);
                        for (int i = 0; i < size; i++)
                        {
                            var generated = Generate(null, itemType!);
                            addFunction!(collection, generated);
                        }
                    }
                    else
                    {
                        if (!prop.CanWrite)
                            continue;
                        if (prop.SetMethod == null || !prop.SetMethod!.IsPublic)
                            continue;
                        if (createFunction == null)
                            continue;

                        collection = createFunction();
                        var size = _rng.Next(0, 100);
                        for (int i = 0; i < size; i++)
                        {
                            var generated = Generate(null, itemType!);
                            addFunction!(collection, generated);
                        }

                        prop.SetValue(obj, collection);
                    }
                }
                else
                {
                    if (!prop.CanWrite)
                        continue;
                    if (prop.SetMethod == null || !prop.SetMethod!.IsPublic)
                        continue;

                    var propType = prop.PropertyType;

                    if (!CanBeGenerated(propType))
                        continue;

                    prop.SetValue(obj, Generate(prop, propType));
                }
            }
        }

        private object? Generate(PropertyInfo? propInfo, Type propType)
        {
            var canBeAutoCompleted = _parent.CanBeAutoCompleted(propType);
            if (propInfo != null && canBeAutoCompleted)
            {
                var nullabilityInfoContext = new NullabilityInfoContext();
                var nullabilityInfo = nullabilityInfoContext.Create(propInfo);
                return _parent.AutoComplete(propType, nullabilityInfo.WriteState == NullabilityState.Nullable);
            }

            if (canBeAutoCompleted)
            {
                return _parent.AutoComplete(propType, false);
            }

            if (propType == typeof(Guid))
                return _generator.NextGuid();

            if (propType == typeof(byte))
                return _generator.NextByte();

            if (propType == typeof(short))
                return _generator.NextShort();

            if (propType == typeof(int))
                return _generator.NextInt();

            if (propType == typeof(long))
                return _generator.NextLong();

            if (propType == typeof(char))
                return _generator.NextChar();

            if (propType == typeof(string))
                return _generator.NextString();

            if (propType == typeof(bool))
                return _generator.NextBool();

            if (propType == typeof(decimal))
                return _generator.NextDecimal();

            if (propType == typeof(DateTime))
                return _generator.NextDateTime();

            if (propType == typeof(DateOnly))
                return _generator.NextDateOnly();

            if (propType == typeof(TimeOnly))
                return _generator.NextTimeOnly();

            if (propType == typeof(TimeSpan))
                return _generator.NextTimeSpan();

            if (propType == typeof(char))
                return _generator.NextChar();

            if (propType == typeof(ObjectId))
                return _generator.NextObjectId();

            if (propType.IsEnum)
            {
                var values = Enum.GetValues(propType);
                return values.GetValue(_rng.Next(0, values.Length));
            }

            if (propType.IsConstructedGenericType && propType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var keyType = propType.GetGenericArguments()[0];
                var valueType = propType.GetGenericArguments()[1];
                return Activator.CreateInstance(propType, Generate(null, keyType), Generate(null, valueType));
            }


            if (propType.IsConstructedGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var nType = propType.GetGenericArguments()[0];
                if (_rng.NextDouble() < Constants.NULL_PROBABILITY)
                    return null;
                else
                    return Generate(null, nType);
            }

            throw new InvalidOperationException();
        }

        private bool IsSupportedCollection(Type propType, out Type? itemType, out Action<object, object?>? addFunction, out Func<object>? createFunction)
        {
            itemType = null;
            addFunction = null;
            createFunction = null;

            var ifaces = propType.GetInterfaces();
            if (ifaces.Any(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)) && ifaces.Any(i => i == typeof(IDictionary)))
            {
                var iface = ifaces.First(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                var args = iface.GetGenericArguments();
                itemType = typeof(KeyValuePair<,>).MakeGenericType(args[0], args[1]);
                var getKey = itemType.GetProperty("Key")!;
                var getValue = itemType.GetProperty("Value")!;
                addFunction = (collection, obj) =>
                {
                    if (obj == null)
                        return;
                    var dict = (IDictionary)collection;
                    dict[getKey.GetValue(obj) ?? throw new InvalidOperationException("null key detected")] = getValue.GetValue(obj);
                };
                var dictType = typeof(Dictionary<,>).MakeGenericType(args[0], args[1]);
                if (propType.IsInterface && propType.IsAssignableFrom(dictType))
                    createFunction = () => Activator.CreateInstance(dictType) ?? throw new InvalidOperationException("could not instantiate object");
                else if (propType.GetConstructor(Type.EmptyTypes) != null)
                    createFunction = () => Activator.CreateInstance(propType) ?? throw new InvalidOperationException("could not instantiate object");
                else
                    return false;

                return true;
            }
            else if (ifaces.Any(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)) && ifaces.Any(i => i == typeof(IList)))
            {
                var iface = ifaces.First(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
                var args = iface.GetGenericArguments();
                itemType = args[0];
                addFunction = (collection, obj) =>
                {
                    if (obj == null)
                        return;
                    var list = (IList)collection;
                    list.Add(obj);
                };
                var listType = typeof(List<>).MakeGenericType(args[0]);
                if (propType.IsInterface && propType.IsAssignableFrom(listType))
                    createFunction = () => Activator.CreateInstance(listType) ?? throw new InvalidOperationException("could not instantiate object");
                else if (propType.GetConstructor(Type.EmptyTypes) != null)
                    createFunction = () => Activator.CreateInstance(propType) ?? throw new InvalidOperationException("could not instantiate object");
                else
                    return false;

                return true;
            }
            else if (ifaces.Any(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>)))
            {
                var iface = ifaces.First(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>));
                var args = iface.GetGenericArguments();
                itemType = args[0];
                var addToSetFunction = propType.GetMethods().FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 1);
                if (addToSetFunction == null)
                    return false;
                addFunction = (collection, obj) =>
                {
                    if (obj == null)
                        return;
                    addToSetFunction.Invoke(collection, new object[] { obj });
                };
                var setType = typeof(HashSet<>).MakeGenericType(args[0]);
                if (propType.IsInterface && propType.IsAssignableFrom(setType))
                    createFunction = () => Activator.CreateInstance(setType) ?? throw new InvalidOperationException("could not instantiate object");
                else if (propType.GetConstructor(Type.EmptyTypes) != null)
                    createFunction = () => Activator.CreateInstance(propType) ?? throw new InvalidOperationException("could not instantiate object");
                else
                    return false;

                return true;
            }

            return false;
        }

        private bool CanBeGenerated(Type propType)
        {
            return 
                _canBeGenerated.Contains(propType) || 
                _parent.CanBeAutoCompleted(propType) ||
                propType.IsEnum || 
                (propType.IsConstructedGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>) && propType.GenericTypeArguments[0].IsEnum) ||
                (propType.IsConstructedGenericType && propType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>) && 
                    CanBeGenerated(propType.GenericTypeArguments[0]) && CanBeGenerated(propType.GenericTypeArguments[1]));
        }

        private HashSet<PropertyInfo> GetIgnoredProperties(Expression generator)
        {
            if (generator.NodeType == ExpressionType.Lambda)
            {
                var lambda = (LambdaExpression)generator;
                if (lambda.Body is MethodCallExpression call && call.Object == lambda.Parameters[0] && call.Method.Name == "With" && call.Arguments.Count == 1 && call.Arguments[0] is LambdaExpression lambda2)
                {
                    if (lambda2.Body.NodeType == ExpressionType.MemberInit)
                        return GetIgnoredProperties((MemberInitExpression)lambda2.Body);
                    else
                        return _emptyProperties;
                }
                else if (lambda.Body.NodeType == ExpressionType.MemberInit)
                {
                    return GetIgnoredProperties((MemberInitExpression)lambda.Body);
                }
                else
                    return _emptyProperties;
            }
            else
                return _emptyProperties;
        }

        private HashSet<PropertyInfo> GetIgnoredProperties(MemberInitExpression expression)
        {
            HashSet<PropertyInfo> properties = new HashSet<PropertyInfo>();
            foreach (var b in expression.Bindings)
            {
                if (b.Member is PropertyInfo)
                    properties.Add((PropertyInfo)b.Member);
            }
            return properties;
        }

        internal override object CreateAutoCompletedObject()
        {
            if (_builder == null)
                throw new InvalidOperationException();
            if (_argTypes.Length != (_autocompleteArgs?.Count ?? 0))
                throw new InvalidOperationException($"failed to autocomplete object because {_argTypes.Length} arguments were expected but {(_autocompleteArgs?.Count ?? 0)} arguments were provided");

            for (int i = 0; i < _argTypes.Length; i++)
            {
                var ty = _argTypes[i];
                var val = _autocompleteArgs != null && _autocompleteArgs.Count >= i + 1 ? _autocompleteArgs[i] : null;

                if (val == null && ty.IsValueType && !(ty.IsConstructedGenericType && ty.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    throw new InvalidOperationException($"failed to autocomplete object because arg {i} of type {ty.FullName} cannot be null");
                if (val != null && !ty.IsAssignableFrom(val.GetType()))
                    throw new InvalidOperationException($"failed to autocomplete object because arg {i} is {val.GetType().FullName} and is not assignable to {ty.FullName}");
            }
            var a1 = _autocompleteArgs != null && _autocompleteArgs.Count >= 1 ? _autocompleteArgs[0] : null;
            var a2 = _autocompleteArgs != null && _autocompleteArgs.Count >= 2 ? _autocompleteArgs[1] : null;
            var a3 = _autocompleteArgs != null && _autocompleteArgs.Count >= 3 ? _autocompleteArgs[2] : null;
            return _builder(a1, a2, a3);
        }

        public ObjectBuilder<T> Recursive(out BuildFunction<T> bf)
        {
            bf = () =>
            {
                if (_builder == null)
                    throw new InvalidOperationException("no recipe was defined");
                if (_argTypes.Length != 0)
                    throw new InvalidOperationException($"expected args ({string.Join(", ", _argTypes.Select(t => t.FullName).ToArray())})");
                return _builder(null, null, null);
            };
            return this;
        }
        public ObjectBuilder<T> Recursive<TArg1, TArg2, TArg3>(out BuildFunction<T, TArg1, TArg2, TArg3> bf)
        {
            bf = (a1, a2, a3) =>
            {
                if (_builder == null)
                    throw new InvalidOperationException("no recipe was defined");
                if (_argTypes.Length != 3)
                    throw new InvalidOperationException($"expected args ({string.Join(", ", _argTypes.Select(t => t.FullName).ToArray())})");
                if (_argTypes[0] != typeof(TArg1))
                    throw new InvalidOperationException($"expected args ({string.Join(", ", _argTypes.Select(t => t.FullName).ToArray())})");
                if (_argTypes[1] != typeof(TArg2))
                    throw new InvalidOperationException($"expected args ({string.Join(", ", _argTypes.Select(t => t.FullName).ToArray())})");
                if (_argTypes[2] != typeof(TArg3))
                    throw new InvalidOperationException($"expected args ({string.Join(", ", _argTypes.Select(t => t.FullName).ToArray())})");
                return _builder(a1, a2, a3);
            };
            return this;
        }
        public ObjectBuilder<T> Recursive<TArg1, TArg2>(out BuildFunction<T, TArg1, TArg2> bf)
        {
            bf = (a1, a2) =>
            {
                if (_builder == null)
                    throw new InvalidOperationException("no recipe was defined");
                if (_argTypes.Length != 2)
                    throw new InvalidOperationException($"expected args ({string.Join(", ", _argTypes.Select(t => t.FullName).ToArray())})");
                if (_argTypes[0] != typeof(TArg1))
                    throw new InvalidOperationException($"expected args ({string.Join(", ", _argTypes.Select(t => t.FullName).ToArray())})");
                if (_argTypes[1] != typeof(TArg2))
                    throw new InvalidOperationException($"expected args ({string.Join(", ", _argTypes.Select(t => t.FullName).ToArray())})");
                return _builder(a1, a2, null);
            };
            return this;
        }
        public ObjectBuilder<T> Recursive<TArg1>(out BuildFunction<T, TArg1> bf)
        {
            bf = (a1) =>
            {
                if (_builder == null)
                    throw new InvalidOperationException("no recipe was defined");
                if (_argTypes.Length != 1)
                    throw new InvalidOperationException($"expected args ({string.Join(", ", _argTypes.Select(t => t.FullName).ToArray())})");
                if (_argTypes[0] != typeof(TArg1))
                    throw new InvalidOperationException($"expected args ({string.Join(", ", _argTypes.Select(t => t.FullName).ToArray())})");
                return _builder(a1, null, null);
            };
            return this;
        }

        public ObjectBuilder<T> ChangeLater(out ObjectBuilder<T> builder)
        {
            builder = this;
            return this;
        }

        public ObjectBuilder<T> AutoFill(bool enable)
        {
            this._autofill = enable;
            return this;
        }
    }
}