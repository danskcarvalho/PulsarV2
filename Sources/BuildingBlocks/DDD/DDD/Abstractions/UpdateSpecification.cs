namespace Pulsar.BuildingBlocks.DDD.Abstractions;

public interface IUpdateSpecification<TModel>
{
    UpdateSpecification<TModel> GetSpec();
}

public class UpdateSpecification<TModel>
{
    public Expression<Func<TModel, bool>> Predicate { get; }
    public IReadOnlyCollection<UpdateCommand<TModel>> Commands { get; }

    public UpdateSpecification(Expression<Func<TModel, bool>> predicate, IEnumerable<UpdateCommand<TModel>>? commands)
    {
        Predicate = predicate;
        if (commands != null)
            Commands = new List<UpdateCommand<TModel>>(commands).AsReadOnly();
        else
            Commands = new List<UpdateCommand<TModel>>().AsReadOnly();
    }
}

public static class Update
{
    public static UpdateSpecificationBuilder<TModel> Where<TModel>(Expression<Func<TModel, bool>> predicate) => new UpdateSpecificationBuilder<TModel>(predicate);
}

public class UpdateSpecificationBuilder<TModel>
{
    private Expression<Func<TModel, bool>> _Predicate;
    private List<UpdateCommand<TModel>> _Commands { get; } = new List<UpdateCommand<TModel>>();

    internal UpdateSpecificationBuilder(Expression<Func<TModel, bool>> predicate)
    {
        _Predicate = predicate;
    }

    public UpdateSpecificationBuilder<TModel> Set<TField>(Expression<Func<TModel, TField>> Expression, TField Value)
    {
        _Commands.Add(new SetUpdateCommand<TModel, TField>(Expression, Value));
        return this;
    }

    public UpdateSpecificationBuilder<TModel> Unset<TField>(Expression<Func<TModel, object>> Expression)
    {
        _Commands.Add(new UnsetUpdateCommand<TModel>(Expression));
        return this;
    }

    public UpdateSpecificationBuilder<TModel> Inc<TField>(Expression<Func<TModel, TField>> Expression, TField Value)
    {
        _Commands.Add(new IncUpdateCommand<TModel, TField>(Expression, Value));
        return this;
    }

    public UpdateSpecificationBuilder<TModel> Push<TField>(Expression<Func<TModel, IEnumerable<TField>>> Expression, TField Value)
    {
        _Commands.Add(new PushUpdateCommand<TModel, TField>(Expression, Value));
        return this;
    }

    public UpdateSpecificationBuilder<TModel> AddToSet<TField>(Expression<Func<TModel, IEnumerable<TField>>> Expression, TField Value)
    {
        _Commands.Add(new AddToSetUpdateCommand<TModel, TField>(Expression, Value));
        return this;
    }

    public UpdateSpecificationBuilder<TModel> PopFirst<TField>(Expression<Func<TModel, object>> Expression)
    {
        _Commands.Add(new PopFirstUpdateCommand<TModel>(Expression));
        return this;
    }

    public UpdateSpecificationBuilder<TModel> PopLast<TField>(Expression<Func<TModel, object>> Expression)
    {
        _Commands.Add(new PopLastUpdateCommand<TModel>(Expression));
        return this;
    }

    public UpdateSpecification<TModel> Build()
    {
        return new UpdateSpecification<TModel>(_Predicate, _Commands);
    }
}

public static class UpdateCommandNames
{
    public const string Set = "Set";
    public const string Unset = "Unset";
    public const string Inc = "Inc";
    public const string Push = "Push";
    public const string AddToSet = "AddToSet";
    public const string PopFirst = "PopFirst";
    public const string PopLast = "PopLast";
}

public abstract record UpdateCommand<T>()
{
    public abstract void InjectField(IUpdateInjectField<T> updateInjectField);
}
public record SetUpdateCommand<T, TField>(Expression<Func<T, TField>> Expression, TField Value) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.Set, Expression, Value);
    }
}
public record UnsetUpdateCommand<T>(Expression<Func<T, object>> Expression) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.Unset, Expression);
    }
}
public record IncUpdateCommand<T, TField>(Expression<Func<T, TField>> Expression, TField Value) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.Inc, Expression, Value);
    }
}
public record PushUpdateCommand<T, TField>(Expression<Func<T, IEnumerable<TField>>> Expression, TField Value) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.Push, Expression, Value);
    }
}
public record AddToSetUpdateCommand<T, TField>(Expression<Func<T, IEnumerable<TField>>> Expression, TField Value) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.AddToSet, Expression, Value);
    }
}
public record PopFirstUpdateCommand<T>(Expression<Func<T, object>> Expression) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.PopFirst, Expression);
    }
}
public record PopLastUpdateCommand<T>(Expression<Func<T, object>> Expression) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.PopLast, Expression);
    }
}

public interface IUpdateInjectField<T>
{
    void Inject<TField>(string commandName, Expression<Func<T, TField>> expression, TField value);

    void Inject<TField>(string commandName, Expression<Func<T, IEnumerable<TField>>> expression, TField value);

    void Inject(string commandName, Expression<Func<T, object>> expression);
}
