namespace Pulsar.BuildingBlocks.DDD.Abstractions;

public interface IFindSpecification<TModel>
{
    FindSpecification<TModel> GetSpec();
}
public interface IFindSpecification<TModel, TProjection>
{
    FindSpecification<TModel, TProjection> GetSpec();
}
public class FindSpecification<TModel> 
{
    public Expression<Func<TModel, bool>> Predicate { get; }
    public IReadOnlyCollection<Ordering<TModel>> OrderBy { get; }
    public int? Skip { get; }
    public int? Limit { get; }

    public FindSpecification(Expression<Func<TModel, bool>> predicate, IEnumerable<Ordering<TModel>>? orderBy, int? skip, int? limit)
    {
        Predicate = predicate;
        if(orderBy != null)
            OrderBy = new List<Ordering<TModel>>(orderBy).AsReadOnly();
        else
            OrderBy = new List<Ordering<TModel>>().AsReadOnly();
        Skip = skip;
        Limit = limit;
    }
}

public class FindSpecification<TModel, TProjection> 
{
    public Expression<Func<TModel, bool>> Predicate { get; }
    public Expression<Func<TModel, TProjection>> Projection { get; }
    public IReadOnlyList<Ordering<TModel>> OrderBy { get; }
    public int? Skip { get; }
    public int? Limit { get; }

    public FindSpecification(Expression<Func<TModel, bool>> predicate, Expression<Func<TModel, TProjection>> projection, IReadOnlyList<Ordering<TModel>> orderBy, int? skip, int? limit)
    {
        Predicate = predicate;
        Projection = projection;
        OrderBy = orderBy;
        Skip = skip;
        Limit = limit;
    }

}

public static class Find
{
    public static FindSpecificationBuilder<TModel> Where<TModel>(Expression<Func<TModel, bool>> predicate) => new FindSpecificationBuilder<TModel>(predicate);
}

public class FindSpecificationBuilder<TModel>
{
    internal Expression<Func<TModel, bool>> _Predicate;
    internal List<Ordering<TModel>> _OrderBy = new List<Ordering<TModel>>();
    internal int? _Skip;
    internal int? _Limit;

    internal FindSpecificationBuilder(Expression<Func<TModel, bool>> predicate)
    {
        _Predicate = predicate;
    }

    public FindSpecificationBuilder<TModel> Skip(int skip)
    {
        _Skip = skip;
        return this;
    }
    public FindSpecificationBuilder<TModel> Limit(int limit)
    {
        _Limit = limit;
        return this;
    }
    public FindSpecificationBuilder<TModel> OrderBy(Expression<Func<TModel, object>> expression)
    {
        _OrderBy.Clear();
        _OrderBy.Add(new Ascending<TModel>(expression));
        return this;
    }
    public FindSpecificationBuilder<TModel> ThenBy(Expression<Func<TModel, object>> expression)
    {
        Debug.Assert(_OrderBy.Count != 0);
        _OrderBy.Add(new Ascending<TModel>(expression));
        return this;
    }
    public FindSpecificationBuilder<TModel> OrderByDescending(Expression<Func<TModel, object>> expression)
    {
        _OrderBy.Clear();
        _OrderBy.Add(new Descending<TModel>(expression));
        return this;
    }
    public FindSpecificationBuilder<TModel> ThenByDescending(Expression<Func<TModel, object>> expression)
    {
        Debug.Assert(_OrderBy.Count != 0);
        _OrderBy.Add(new Descending<TModel>(expression));
        return this;
    }
    public FindSpecificationBuilder<TModel, TProjection> Select<TProjection>(Expression<Func<TModel, TProjection>> projection)
    {
        return new FindSpecificationBuilder<TModel, TProjection>(this, projection);
    }
    public FindSpecification<TModel> Build()
    {
        return new FindSpecification<TModel>(_Predicate, _OrderBy, _Skip, _Limit);
    }
}

public class FindSpecificationBuilder<TModel, TProjection>
{
    private FindSpecificationBuilder<TModel> _OriginalBuilder;
    private Expression<Func<TModel, TProjection>> _Projection;

    internal FindSpecificationBuilder(FindSpecificationBuilder<TModel> originalBuilder, Expression<Func<TModel, TProjection>> projection)
    {
        _OriginalBuilder = originalBuilder;
        _Projection = projection;
    }

    public FindSpecificationBuilder<TModel, TProjection> Skip(int skip)
    {
        _OriginalBuilder.Skip(skip);
        return this;
    }
    public FindSpecificationBuilder<TModel, TProjection> Limit(int limit)
    {
        _OriginalBuilder.Limit(limit);
        return this;
    }
    public FindSpecificationBuilder<TModel, TProjection> OrderBy(Expression<Func<TModel, object>> expression)
    {
        _OriginalBuilder.OrderBy(expression);
        return this;
    }
    public FindSpecificationBuilder<TModel, TProjection> ThenBy(Expression<Func<TModel, object>> expression)
    {
        _OriginalBuilder.ThenBy(expression);
        return this;
    }
    public FindSpecificationBuilder<TModel, TProjection> OrderByDescending(Expression<Func<TModel, object>> expression)
    {
        _OriginalBuilder.OrderByDescending(expression);
        return this;
    }
    public FindSpecificationBuilder<TModel, TProjection> ThenByDescending(Expression<Func<TModel, object>> expression)
    {
        _OriginalBuilder.ThenByDescending(expression);
        return this;
    }
    public FindSpecification<TModel, TProjection> Build()
    {
        return new FindSpecification<TModel, TProjection>(_OriginalBuilder._Predicate, _Projection, _OriginalBuilder._OrderBy, _OriginalBuilder._Skip, _OriginalBuilder._Limit);
    }
}

public abstract record Ordering<T>();
public record Ascending<T>(Expression<Func<T, object>> Expression) : Ordering<T>();
public record Descending<T>(Expression<Func<T, object>> Expression) : Ordering<T>();