using System.Linq.Expressions;
using static MongoDB.Driver.WriteConcern;

namespace Pulsar.BuildingBlocks.DDD.Mongo;

public class MongoIndexDescriptions<TModel> : IndexDescriptionsImplementation<TModel>
{
    public override IndexBuilder<TModel> CreateBuilder()
    {
        return new MongoIndexBuilder<TModel>();
    }
}

public class MongoIndexBuilder<TModel> : IndexBuilder<TModel>
{
    private IndexKeysDefinition<TModel>? _definition = null;

    private bool? _unique = null;
    public override bool? IsUnique => _unique;

    public MongoIndexBuilder() { }
    public IndexKeysDefinition<TModel> GetDefinition() => _definition ?? throw new InvalidOperationException("no index definition");

    public override IndexBuilder<TModel> Ascending(Expression<Func<TModel, object?>> field)
    {
        if (_definition == null)
            _definition = Builders<TModel>.IndexKeys.Ascending(field);
        else
            _definition = _definition.Ascending(field);

        return this;
    }

    public override IndexBuilder<TModel> Ascending(string field)
    {
        if (_definition == null)
            _definition = Builders<TModel>.IndexKeys.Ascending(field);
        else
            _definition = _definition.Ascending(field);

        return this;
    }

    public override IndexBuilder<TModel> Descending(Expression<Func<TModel, object?>> field)
    {
        if (_definition == null)
            _definition = Builders<TModel>.IndexKeys.Descending(field);
        else
            _definition = _definition.Descending(field);

        return this;
    }

    public override IndexBuilder<TModel> Descending(string field)
    {
        if (_definition == null)
            _definition = Builders<TModel>.IndexKeys.Descending(field);
        else
            _definition = _definition.Descending(field);

        return this;
    }

    public override IndexBuilder<TModel> Hashed(Expression<Func<TModel, object?>> field)
    {
        if (_definition == null)
            _definition = Builders<TModel>.IndexKeys.Hashed(field);
        else
            _definition = _definition.Hashed(field);

        return this;
    }

    public override IndexBuilder<TModel> Hashed(string field)
    {
        if (_definition == null)
            _definition = Builders<TModel>.IndexKeys.Hashed(field);
        else
            _definition = _definition.Hashed(field);

        return this;
    }

    public override IndexBuilder<TModel> Text(Expression<Func<TModel, object?>> field)
    {
        if (_definition == null)
            _definition = Builders<TModel>.IndexKeys.Text(field);
        else
            _definition = _definition.Text(field);

        return this;
    }

    public override IndexBuilder<TModel> Text(string field)
    {
        if (_definition == null)
            _definition = Builders<TModel>.IndexKeys.Text(field);
        else
            _definition = _definition.Text(field);

        return this;
    }

    public override IndexBuilder<TModel> Unique()
    {
        _unique = true;
        return this;
    }
}
