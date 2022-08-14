namespace Pulsar.BuildingBlocks.DDD.Abstractions;

public interface IDeleteSpecification<TModel>
{
    DeleteSpecification<TModel> GetSpec();
}

public class DeleteSpecification<TModel>
{
    public Expression<Func<TModel, bool>> Predicate { get; }

    public DeleteSpecification(Expression<Func<TModel, bool>> predicate)
    {
        Predicate = predicate;
    }
}

public static class Delete
{
    public static DeleteSpecificationBuilder<TModel> Where<TModel>(Expression<Func<TModel, bool>> predicate) => new DeleteSpecificationBuilder<TModel>(predicate);
}

public class DeleteSpecificationBuilder<TModel>
{
    internal Expression<Func<TModel, bool>> _Predicate;

    internal DeleteSpecificationBuilder(Expression<Func<TModel, bool>> predicate)
    {
        _Predicate = predicate;
    }
    public DeleteSpecification<TModel> Build()
    {
        return new DeleteSpecification<TModel>(_Predicate);
    }
}
