namespace Pulsar.BuildingBlocks.DataFactory;

public class BuilderRoot
{

    private Random _rng;
    private Dictionary<Type, ObjectBuilderBase> _builders = new();
    private int _level = 0;
    private int _maxLevel = 6;

    internal BuilderRoot(int seed)
    {
        this._rng = new Random(seed);
    }

    public ObjectBuilder<T> For<T>() where T : class => new(_rng, this);
    public BuilderRoot MaxRecursion(int maxRecursion)
    {
        if (maxRecursion <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRecursion), "maxRecursion must be > 0");
        _maxLevel = maxRecursion + 1;
        return this;
    }

    internal object? AutoComplete(Type type, bool allowNull)
    {
        if (allowNull && _rng.NextDouble() <= Constants.NULL_PROBABILITY)
            return null;

        return _builders[type].CreateAutoCompletedObject();
    }

    internal bool CanBeAutoCompleted(Type type)
    {
        return _builders.ContainsKey(type);
    }

    internal void RegisterForAutoComplete(Type type, ObjectBuilderBase builder)
    {
        _builders[type] = builder;
    }

    internal T UpOneLevel<T>(Func<T> value)
    {
        _level++;
        try
        {
            if (_level >= _maxLevel)
                return default(T)!;

            return value();
        }
        finally
        {
            _level--;
        }
    }
}