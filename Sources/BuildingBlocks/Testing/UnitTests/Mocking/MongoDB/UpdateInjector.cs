namespace Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;

class UpdateInjector<TModel> : IUpdateInjectField<TModel>
{
    public required BsonValue Target { get; set; }
    public BsonValue? Result { get; set; }

    public void Inject<TField>(string commandName, Expression<Func<TModel, TField>> expression, TField value)
    {
        switch (commandName)
        {
            case UpdateCommandNames.INC:
                {
                    var propValue = Get(Target, expression);
                    if (propValue is not null && !propValue.IsBsonNull && !propValue.IsBsonUndefined)
                    {
                        if (propValue.IsDecimal128)
                            Result = propValue.AsDecimal + Convert.ToDecimal(value);
                        else if (propValue.IsDouble)
                            Result = propValue.AsDouble + Convert.ToDouble(value);
                        else if (propValue.IsInt32)
                            Result = propValue.AsInt32 + Convert.ToInt32(value);
                        else if (propValue.IsInt64)
                            Result = propValue.AsInt64 + Convert.ToInt64(value);
                        else
                            throw new InvalidCastException();

                        if (!IsIdentity(expression))
                            Set(Target, expression, Result);
                        else
                            Result = null;
                    }
                }
                break;
            case UpdateCommandNames.SET:
                {
                    if (!IsIdentity(expression))
                        Set(Target, expression, ToBsonValue(value));
                    else
                        Result = ToBsonValue(value);
                }
                break;
            case UpdateCommandNames.MAX:
                {
                    var propValue = Get(Target, expression);
                    var current = ToBsonValue(value);

                    if (!IsIdentity(expression))
                        Set(Target, expression, propValue >= current ? propValue : current);
                    else
                        Result = propValue >= current ? propValue : current;
                }
                break;
            case UpdateCommandNames.MIN:
                {
                    var propValue = Get(Target, expression);
                    var current = ToBsonValue(value);

                    if (!IsIdentity(expression))
                        Set(Target, expression, propValue <= current ? propValue : current);
                    else
                        Result = propValue <= current ? propValue : current;
                }
                break;
            case UpdateCommandNames.MUL:
                {
                    var propValue = Get(Target, expression);
                    if (propValue is not null && !propValue.IsBsonNull && !propValue.IsBsonUndefined)
                    {
                        // -- this doesn't follow MongoDB promotion of types but it should be okay if we don't mix types
                        if (propValue.IsDecimal128)
                            Result = propValue.AsDecimal * Convert.ToDecimal(value);
                        else if (propValue.IsDouble)
                            Result = propValue.AsDouble * Convert.ToDouble(value);
                        else if (propValue.IsInt32)
                            Result = propValue.AsInt32 * Convert.ToInt32(value);
                        else if (propValue.IsInt64)
                            Result = propValue.AsInt64 * Convert.ToInt64(value);
                        else
                            throw new InvalidCastException();

                        if (!IsIdentity(expression))
                            Set(Target, expression, Result);
                        else
                            Result = null;
                    }
                }
                break;
            default:
                break;
        }
    }

    public void Inject<TField>(string commandName, Expression<Func<TModel, IEnumerable<TField>>> expression, TField value)
    {
        switch (commandName)
        {
            case UpdateCommandNames.ADD_TO_SET:
                {
                    var propValue = Get(Target, expression);
                    if (propValue is not null && !propValue.IsBsonNull && !propValue.IsBsonUndefined)
                    {
                        var array = propValue.AsBsonArray;
                        var set = new SortedSet<BsonValue>(array)
                        {
                            ToBsonValue(value)
                        };
                        array.Clear();
                        foreach (var item in set)
                            array.Add(item);
                    }
                }
                break;
            case UpdateCommandNames.PUSH:
                {
                    var propValue = Get(Target, expression);
                    if (propValue is not null && !propValue.IsBsonNull && !propValue.IsBsonUndefined)
                    {
                        var array = propValue.AsBsonArray;
                        array.Add(ToBsonValue(value));
                    }
                }
                break;
            case UpdateCommandNames.PULL:
                {
                    var propValue = Get(Target, expression);
                    if (propValue is not null && !propValue.IsBsonNull && !propValue.IsBsonUndefined)
                    {
                        var array = propValue.AsBsonArray;
                        var bsonValue = ToBsonValue(value);
                        var array2 = new List<BsonValue>(array);
                        array2.RemoveAll(x => x == bsonValue);
                        array.Clear();
                        foreach (var item in array2)
                            array.Add(item);
                    }
                }
                break;
            default:
                break;
        }
    }

    public void Inject<TField>(string commandName, Expression<Func<TModel, IEnumerable<TField>>> expression, IEnumerable<TField> values)
    {
        switch (commandName)
        {
            case UpdateCommandNames.ADD_TO_SET_EACH:
                {
                    var propValue = Get(Target, expression);
                    if (propValue is not null && !propValue.IsBsonNull && !propValue.IsBsonUndefined)
                    {
                        var array = propValue.AsBsonArray;
                        var set = new SortedSet<BsonValue>(array);
                        foreach (var value in values)
                        {
                            set.Add(ToBsonValue(value));
                        }
                        array.Clear();
                        foreach (var item in set)
                            array.Add(item);
                    }
                }
                break;
            case UpdateCommandNames.PULL_ALL:
                {
                    var propValue = Get(Target, expression);
                    if (propValue is not null && !propValue.IsBsonNull && !propValue.IsBsonUndefined)
                    {
                        var array = propValue.AsBsonArray;
                        var sortedSet = new SortedSet<BsonValue>(values.Select(x => ToBsonValue(x)));
                        var array2 = new List<BsonValue>(array);
                        array2.RemoveAll(sortedSet.Contains);
                        array.Clear();
                        foreach (var item in array2)
                            array.Add(item);
                    }
                }
                break;
            case UpdateCommandNames.PUSH_EACH:
                {
                    var propValue = Get(Target, expression);
                    if (propValue is not null && !propValue.IsBsonNull && !propValue.IsBsonUndefined)
                    {
                        var array = propValue.AsBsonArray;
                        foreach (var value in values)
                        {
                            array.Add(ToBsonValue(value));
                        }
                    }
                }
                break;
            default:
                break;
        }
    }

    public void Inject<TField>(string commandName, Expression<Func<TModel, IEnumerable<TField>>> expression, Expression<Func<TField, bool>> filter)
    {
        switch (commandName)
        {
            case UpdateCommandNames.PULL_FILTER:
                {
                    var pred = filter.Compile();
                    var propValue = Get(Target, expression);
                    if (propValue is not null && !propValue.IsBsonNull && !propValue.IsBsonUndefined)
                    {
                        var array = propValue.AsBsonArray;
                        var array2 = new List<BsonValue>(array);
                        array2.RemoveAll(x => pred(FromBsonValue<TField>(x)));
                        array.Clear();
                        foreach (var item in array2)
                            array.Add(item);
                    }
                }
                break;
            default:
                break;
        }
    }

    public void Inject(string commandName, Expression<Func<TModel, object>> expression)
    {
        switch (commandName)
        {
            case UpdateCommandNames.POP_FIRST:
                {
                    var propValue = Get(Target, expression);
                    if (propValue is not null && !propValue.IsBsonNull && !propValue.IsBsonUndefined)
                    {
                        propValue.AsBsonArray.RemoveAt(0);
                    }
                }
                break;
            case UpdateCommandNames.POP_LAST:
                {
                    var propValue = Get(Target, expression);
                    if (propValue is not null && !propValue.IsBsonNull && !propValue.IsBsonUndefined)
                    {
                        propValue.AsBsonArray.RemoveAt(propValue.AsBsonArray.Count);
                    }
                }
                break;
            case UpdateCommandNames.UNSET:
                {
                    var path = GetPath(expression);
                    if (path.Length == 0)
                        throw new NotSupportedException("can't unset element of an array or the root document");
                    var doc = Target.AsBsonDocument;
                    foreach (var p in path.Take(path.Length - 1))
                    {
                        if (!doc.Contains(p))
                            return;
                        doc = doc[p].AsBsonDocument;
                    }

                    if (!doc.Contains(path[^1]))
                        return;
                    doc.Remove(path[^1]);
                }
                break;
            default:
                break;
        }
    }

    public void Inject<TField>(string commandName, Expression<Func<TModel, IEnumerable<TField>>> expression, UpdateSpecification<TField> updateElement)
    {
        if (commandName != UpdateCommandNames.FOR_EACH)
            return;

        var pred = updateElement.Predicate.Compile();
        var propValue = Get(Target, expression);
        if (propValue is not null && !propValue.IsBsonNull && !propValue.IsBsonUndefined)
        {
            var array = propValue.AsBsonArray;

            foreach (var item in array.Where(x => pred(FromBsonValue<TField>(x))))
            {
                var injector = new UpdateInjector<TField>()
                {
                    Target = item
                };
                List<(int Index, BsonValue Value)> valuesToSet = new List<(int Index, BsonValue Value)>();
                var index = 0;
                foreach (var cmd in updateElement.Commands)
                {
                    injector.Result = null;
                    cmd.InjectField(injector);
                    if (injector.Result != null)
                        valuesToSet.Add((index, injector.Result));

                    index++;
                }

                foreach (var vs in valuesToSet)
                {
                    array[vs.Index] = vs.Value;
                }
            }
        }
    }

    private string[] GetPath<TField>(Expression<Func<TModel, TField>> expression)
    {
        if (IsIdentity(expression))
            return new string[0];

        var efd = new ExpressionFieldDefinition<TModel, TField>(expression);
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<TModel>();
        return efd.Render(documentSerializer, serializerRegistry, global::MongoDB.Driver.Linq.LinqProvider.V3).FieldName.Split('.', StringSplitOptions.RemoveEmptyEntries);
    }

    private BsonValue Get(BsonValue doc, string[] path)
    {
        for (int i = 0; i < path.Length; i++)
        {
            doc = doc[path[i]];
        }

        return doc;
    }

    private bool IsIdentity<TField>(Expression<Func<TModel, TField>> expression) => expression is LambdaExpression l && l.Parameters.Count == 1 && l.Parameters[0] == l.Body;

    private BsonValue Set(BsonValue doc, string[] path, BsonValue value)
    {
        if (path.Length == 0)
            throw new InvalidOperationException("can't replace root element");

        foreach (var p in path.Take(path.Length - 1))
        {
            doc = doc[p];
        }

        doc[path[^1]] = value;

        return doc;
    }

    private BsonValue Get<TField>(BsonValue doc, Expression<Func<TModel, TField>> expression) => Get(doc, GetPath(expression));

    private BsonValue Set<TField>(BsonValue doc, Expression<Func<TModel, TField>> expression, BsonValue value) => Set(doc, GetPath(expression), value);

    private BsonValue ToBsonValue<Y>(Y item)
    {
        var doc = new ValueContainer<Y> { Value = item }.ToBsonDocument();
        return doc["Value"];
    }

    private Y FromBsonValue<Y>(BsonValue d)
    {
        BsonDocument doc = new BsonDocument
        {
            ["Value"] = d
        };
        return BsonSerializer.Deserialize<ValueContainer<Y>>(doc).Value;
    }

    class ValueContainer<Y>
    {
        public required Y Value { get; set; }
    }
}
