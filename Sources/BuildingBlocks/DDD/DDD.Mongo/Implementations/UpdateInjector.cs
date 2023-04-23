using MongoDB.Driver;
using System.Linq.Expressions;

namespace Pulsar.BuildingBlocks.DDD.Mongo.Implementations;

class UpdateInjector<T> : IUpdateInjectField<T>
{
    public List<UpdateDefinition<T>> UpdateDefinitions { get; set; } = new List<UpdateDefinition<T>>();

    public UpdateDefinition<T> UpdateDefinition => Builders<T>.Update.Combine(UpdateDefinitions);
    public List<ArrayFilterDefinition<T>> ArrayFilters { get; set; } = new List<ArrayFilterDefinition<T>>();
    public string? FieldPrefix { get; set; }

    public void Inject<TField>(string commandName, Expression<Func<T, TField>> expression, TField value)
    {
        switch (commandName)
        {
            case UpdateCommandNames.INC:
                UpdateDefinitions.Add(Builders<T>.Update.Inc(GetField(expression), value));
                break;
            case UpdateCommandNames.SET:
                UpdateDefinitions.Add(Builders<T>.Update.Set(GetField(expression), value));
                break;
            case UpdateCommandNames.MAX:
                UpdateDefinitions.Add(Builders<T>.Update.Max(GetField(expression), value));
                break;
            case UpdateCommandNames.MIN:
                UpdateDefinitions.Add(Builders<T>.Update.Min(GetField(expression), value));
                break;
            case UpdateCommandNames.MUL:
                UpdateDefinitions.Add(Builders<T>.Update.Mul(GetField(expression), value));
                break;
            default:
                break;
        }
    }

    private FieldDefinition<T, TField> GetField<TField>(Expression<Func<T, TField>> expression)
    {
        string fieldName;
        if (expression is LambdaExpression l && l.Parameters.Count == 1 && l.Parameters[0] == l.Body) // -- is the identity function
            fieldName = string.Empty;
        else
        {
            var efd = new ExpressionFieldDefinition<T, TField>(expression);
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<T>();
            fieldName = efd.Render(documentSerializer, serializerRegistry, MongoDB.Driver.Linq.LinqProvider.V3).FieldName;
        }

        if (!string.IsNullOrEmpty(fieldName) && !string.IsNullOrEmpty(FieldPrefix))
            return $"{FieldPrefix}.{fieldName}";
        else if (string.IsNullOrEmpty(fieldName))
            return FieldPrefix ?? string.Empty;
        else if (string.IsNullOrEmpty(FieldPrefix))
            return fieldName ?? string.Empty;
        else
            throw new InvalidOperationException();
    }

    public void Inject(string commandName, Expression<Func<T, object>> expression)
    {
        switch (commandName)
        {
            case UpdateCommandNames.POP_FIRST:
                UpdateDefinitions.Add(Builders<T>.Update.PopFirst(GetField(expression)));
                break;
            case UpdateCommandNames.POP_LAST:
                UpdateDefinitions.Add(Builders<T>.Update.PopLast(GetField(expression)));
                break;
            case UpdateCommandNames.UNSET:
                UpdateDefinitions.Add(Builders<T>.Update.Unset(GetField(expression)));
                break;
            default:
                break;
        }
    }

    public void Inject<TField>(string commandName, Expression<Func<T, IEnumerable<TField>>> expression, TField value)
    {
        switch (commandName)
        {
            case UpdateCommandNames.ADD_TO_SET:
                UpdateDefinitions.Add(Builders<T>.Update.AddToSet(GetField(expression), value));
                break;
            case UpdateCommandNames.PUSH:
                UpdateDefinitions.Add(Builders<T>.Update.Push(GetField(expression), value));
                break;
            case UpdateCommandNames.PULL:
                UpdateDefinitions.Add(Builders<T>.Update.Pull(GetField(expression), value));
                break;
            default:
                break;
        }
    }

    public void Inject<TField>(string commandName, Expression<Func<T, IEnumerable<TField>>> expression, IEnumerable<TField> values)
    {
        switch (commandName)
        {
            case UpdateCommandNames.ADD_TO_SET_EACH:
                UpdateDefinitions.Add(Builders<T>.Update.AddToSetEach(GetField(expression), values));
                break;
            case UpdateCommandNames.PULL_ALL:
                UpdateDefinitions.Add(Builders<T>.Update.PullAll(GetField(expression), values));
                break;
            case UpdateCommandNames.PUSH_EACH:
                UpdateDefinitions.Add(Builders<T>.Update.PushEach(GetField(expression), values));
                break;
            default:
                break;
        }
    }

    public void Inject<TField>(string commandName, Expression<Func<T, IEnumerable<TField>>> expression, Expression<Func<TField, bool>> filter)
    {
        switch (commandName)
        {
            case UpdateCommandNames.PULL_FILTER:
                UpdateDefinitions.Add(Builders<T>.Update.PullFilter(GetField(expression), new ExpressionFilterDefinition<TField>(filter)));
                break;
            default:
                break;
        }
    }

    public void Inject<TField>(string commandName, Expression<Func<T, IEnumerable<TField>>> expression, UpdateSpecification<TField> updateElement)
    {
        if (commandName != UpdateCommandNames.FOR_EACH)
            return;

        var prefixId = !IsTruePredicate(updateElement.Predicate) ? Guid.NewGuid() : (Guid?)null;
        if (prefixId.HasValue)
            this.ArrayFilters.Add(RenderArrayFilter(prefixId.Value, updateElement.Predicate));
        var prefix = RenderFieldPrefix(prefixId, expression);

        var arrayFieldInjector = new UpdateInjector<TField>()
        {
            FieldPrefix = CombinePaths(FieldPrefix, prefix)
        };
        foreach (var cmd in updateElement.Commands)
        {
            cmd.InjectField(arrayFieldInjector);
        }
        foreach (var def in arrayFieldInjector.UpdateDefinitions)
        {
            this.UpdateDefinitions.Add(RenderUpdateDefinition(def));
        }
        foreach (var af in arrayFieldInjector.ArrayFilters)
        {
            this.ArrayFilters.Add(RenderArrayFilter(af));
        }
    }

    private bool IsTruePredicate<TField>(Expression<Func<TField, bool>> predicate)
    {
        return predicate.Body is ConstantExpression ce && ce.Value is bool b && b == true;
    }

    private string CombinePaths(string? path1, string? path2)
    {
        if (!string.IsNullOrEmpty(path1) && !string.IsNullOrEmpty(path1))
            return $"{path1}.{path2}";
        else if (string.IsNullOrEmpty(path1))
            return path2 ?? string.Empty;
        else if (string.IsNullOrEmpty(path2))
            return path1 ?? string.Empty;
        else
            throw new InvalidOperationException();
    }

    private ArrayFilterDefinition<T> RenderArrayFilter<TField>(ArrayFilterDefinition<TField> af)
    {
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<TField>();
        var bson = af.Render(documentSerializer, serializerRegistry, MongoDB.Driver.Linq.LinqProvider.V3);
        return new BsonDocumentArrayFilterDefinition<T>(bson as BsonDocument);
    }

    private UpdateDefinition<T> RenderUpdateDefinition<TField>(UpdateDefinition<TField> def)
    {
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<TField>();
        var bson = def.Render(documentSerializer, serializerRegistry, MongoDB.Driver.Linq.LinqProvider.V3);
        return new BsonDocumentUpdateDefinition<T>(bson as BsonDocument);
    }

    private string RenderFieldPrefix<TField>(Guid? prefixId, Expression<Func<T, IEnumerable<TField>>> expression)
    {
        var prefix = GetPrefix(expression);
        return prefixId.HasValue ? $"{prefix}.$[{GetArrayFilterName(prefixId.Value)}]" : $"{prefix}.$[]";
    }

    private string GetPrefix<TField>(Expression<Func<T, IEnumerable<TField>>> expression)
    {
        var efd = new ExpressionFieldDefinition<T, IEnumerable<TField>>(expression);
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<T>();
        return efd.Render(documentSerializer, serializerRegistry, MongoDB.Driver.Linq.LinqProvider.V3).FieldName;
    }

    private ArrayFilterDefinition<T> RenderArrayFilter<TField>(Guid prefixId, Expression<Func<TField, bool>> predicate)
    {
        var recontextualized = predicate.Recontextualize();
        FilterDefinition<ArrayFilterContext<TField>> filterDef = recontextualized;
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<ArrayFilterContext<TField>>();
        var bson = filterDef.Render(documentSerializer, serializerRegistry, MongoDB.Driver.Linq.LinqProvider.V3);
        return bson.RenameContext(GetArrayFilterName(prefixId));
    }

    private static string GetArrayFilterName(Guid prefixId)
    {
        return $"af{prefixId.ToString("N").ToLowerInvariant()}";
    }
}

class ArrayFilterContext<T>
{
    // -- to ensure uniqueness of the name
    public T ContextACCEB0E72973437C8D49ABF4F40B3595 { get; set; }

    public ArrayFilterContext(T context)
    {
        ContextACCEB0E72973437C8D49ABF4F40B3595 = context;
    }
}

static class ArrayFilterContext
{
    public static Expression<Func<ArrayFilterContext<T>, bool>> Recontextualize<T>(this Expression<Func<T, bool>> func)
    {
        var member = typeof(ArrayFilterContext<T>).GetProperty("ContextACCEB0E72973437C8D49ABF4F40B3595");
        var parameter = Expression.Parameter(typeof(ArrayFilterContext<T>), $"p{Guid.NewGuid().ToString("N")}");

        var visitor = new ReplaceExpressionVisitor(func.Parameters[0], Expression.MakeMemberAccess(parameter, member!));
        var body = visitor.Visit(func.Body)!;


        return Expression.Lambda<Func<ArrayFilterContext<T>, bool>>(
            body, parameter);
    }

    public static BsonDocument RenameContext(this BsonDocument source, string renameTo)
    {
        BsonDocument output = new BsonDocument();
        foreach (var item in source.Elements)
        {
            if (item.Name.Contains("ContextACCEB0E72973437C8D49ABF4F40B3595"))
                output[item.Name.Replace("ContextACCEB0E72973437C8D49ABF4F40B3595", renameTo)] = RenameContext(item.Value, renameTo);
            else
                output[item.Name] = RenameContext(item.Value, renameTo);
        }
        return output;
    }

    private static BsonValue RenameContext(BsonValue value, string renameTo)
    {
        if (value is BsonDocument bd)
            return RenameContext(bd, renameTo);
        else if (value is BsonArray array)
            return new BsonArray(array.Select(a => RenameContext(a, renameTo)).ToList());
        else if (value is BsonString bs)
            return new BsonString(bs.Value.Replace("ContextACCEB0E72973437C8D49ABF4F40B3595", renameTo));
        else
            return value;
    }
}

class ReplaceExpressionVisitor
        : System.Linq.Expressions.ExpressionVisitor
{
    private readonly Expression _oldValue;
    private readonly Expression _newValue;

    public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
    {
        _oldValue = oldValue;
        _newValue = newValue;
    }

    public override Expression? Visit(Expression? node)
    {
        if (node == _oldValue)
            return _newValue;
        return base.Visit(node);
    }
}
