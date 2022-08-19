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

    public UpdateSpecificationBuilder<TModel> Unset(Expression<Func<TModel, object>> Expression)
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

    public UpdateSpecificationBuilder<TModel> PopFirst(Expression<Func<TModel, object>> Expression)
    {
        _Commands.Add(new PopFirstUpdateCommand<TModel>(Expression));
        return this;
    }

    public UpdateSpecificationBuilder<TModel> PopLast(Expression<Func<TModel, object>> Expression)
    {
        _Commands.Add(new PopLastUpdateCommand<TModel>(Expression));
        return this;
    }

    public UpdateSpecificationBuilder<TModel> AddToSetEach<TField>(Expression<Func<TModel, IEnumerable<TField>>> Expression, IEnumerable<TField> Values)
    {
        _Commands.Add(new AddToSetEachUpdateCommand<TModel, TField>(Expression, Values));
        return this;
    }
    public UpdateSpecificationBuilder<TModel> Max<TField>(Expression<Func<TModel, TField>> Expression, TField Value)
    {
        _Commands.Add(new MaxUpdateCommand<TModel, TField>(Expression, Value));
        return this;
    }
    public UpdateSpecificationBuilder<TModel> Min<TField>(Expression<Func<TModel, TField>> Expression, TField Value)
    {
        _Commands.Add(new MinUpdateCommand<TModel, TField>(Expression, Value));
        return this;
    }
    public UpdateSpecificationBuilder<TModel> Mul<TField>(Expression<Func<TModel, TField>> Expression, TField Value)
    {
        _Commands.Add(new MulUpdateCommand<TModel, TField>(Expression, Value));
        return this;
    }
    public UpdateSpecificationBuilder<TModel> Pull<TField>(Expression<Func<TModel, IEnumerable<TField>>> Expression, TField Value)
    {
        _Commands.Add(new PullUpdateCommand<TModel, TField>(Expression, Value));
        return this;
    }
    public UpdateSpecificationBuilder<TModel> PullAll<TField>(Expression<Func<TModel, IEnumerable<TField>>> Expression, IEnumerable<TField> Values)
    {
        _Commands.Add(new PullAllUpdateCommand<TModel, TField>(Expression, Values));
        return this;
    }
    public UpdateSpecificationBuilder<TModel> PullFilter<TField>(Expression<Func<TModel, IEnumerable<TField>>> Expression, Expression<Func<TField, bool>> Filter)
    {
        _Commands.Add(new PullFilterUpdateCommand<TModel, TField>(Expression, Filter));
        return this;
    }
    public UpdateSpecificationBuilder<TModel> PushEach<TField>(Expression<Func<TModel, IEnumerable<TField>>> Expression, IEnumerable<TField> Values)
    {
        _Commands.Add(new PushEachUpdateCommand<TModel, TField>(Expression, Values));
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
    public const string AddToSetEach = "AddToSetEach";
    public const string Max = "Max";
    public const string Min = "Min";
    public const string Mul = "Mul";
    public const string Pull = "Pull";
    public const string PullAll = "PullAll";
    public const string PullFilter = "PullFilter";
    public const string PushEach = "PushEach";
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
public record AddToSetEachUpdateCommand<T, TField>(Expression<Func<T, IEnumerable<TField>>> Expression, IEnumerable<TField> Values) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.AddToSetEach, Expression, Values);
    }
}
public record MaxUpdateCommand<T, TField>(Expression<Func<T, TField>> Expression, TField Value) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.Max, Expression, Value);
    }
}
public record MinUpdateCommand<T, TField>(Expression<Func<T, TField>> Expression, TField Value) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.Min, Expression, Value);
    }
}
public record MulUpdateCommand<T, TField>(Expression<Func<T, TField>> Expression, TField Value) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.Mul, Expression, Value);
    }
}
public record PullUpdateCommand<T, TField>(Expression<Func<T, IEnumerable<TField>>> Expression, TField Value) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.Pull, Expression, Value);
    }
}
public record PullAllUpdateCommand<T, TField>(Expression<Func<T, IEnumerable<TField>>> Expression, IEnumerable<TField> Values) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.PullAll, Expression, Values);
    }
}
public record PullFilterUpdateCommand<T, TField>(Expression<Func<T, IEnumerable<TField>>> Expression, Expression<Func<TField, bool>> Filter) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.PullFilter, Expression, Filter);
    }
}
public record PushEachUpdateCommand<T, TField>(Expression<Func<T, IEnumerable<TField>>> Expression, IEnumerable<TField> Values) : UpdateCommand<T>()
{
    public override void InjectField(IUpdateInjectField<T> updateInjectField)
    {
        updateInjectField.Inject(UpdateCommandNames.PushEach, Expression, Values);
    }
}

public interface IUpdateInjectField<T>
{
    void Inject<TField>(string commandName, Expression<Func<T, TField>> expression, TField value);
    void Inject<TField>(string commandName, Expression<Func<T, IEnumerable<TField>>> expression, TField value);
    void Inject<TField>(string commandName, Expression<Func<T, IEnumerable<TField>>> expression, IEnumerable<TField> values);
    void Inject<TField>(string commandName, Expression<Func<T, IEnumerable<TField>>> expression, Expression<Func<TField, bool>> filter);
    void Inject(string commandName, Expression<Func<T, object>> expression);
}
