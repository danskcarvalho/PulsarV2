namespace Pulsar.BuildingBlocks.DataFactory;

public static class BuildFunctionExtensions
{
    public static BuildFunction<TModel> Modify<TModel>(this BuildFunction<TModel> fn, Action<TModel, GeneratorBase> action)
    {
        return () =>
        {
            var model = fn();
            action(model, ObjectBuilderBase.GetGenerator(fn) ?? throw new InvalidOperationException("unknown build function"));
            return model;
        };
    }

    public static BuildFunction<TModel, TArg1> Modify<TModel, TArg1>(this BuildFunction<TModel, TArg1> fn, Action<TModel, GeneratorBase> action)
    {
        return (a1) =>
        {
            var model = fn(a1);
            action(model, ObjectBuilderBase.GetGenerator(fn) ?? throw new InvalidOperationException("unknown build function"));
            return model;
        };
    }

    public static BuildFunction<TModel, TArg1, TArg2> Modify<TModel, TArg1, TArg2>(this BuildFunction<TModel, TArg1, TArg2> fn, Action<TModel, GeneratorBase> action)
    {
        return (a1, a2) =>
        {
            var model = fn(a1, a2);
            action(model, ObjectBuilderBase.GetGenerator(fn) ?? throw new InvalidOperationException("unknown build function"));
            return model;
        };
    }

    public static BuildFunction<TModel, TArg1, TArg2, TArg3> Modify<TModel, TArg1, TArg2, TArg3>(this BuildFunction<TModel, TArg1, TArg2, TArg3> fn, Action<TModel, GeneratorBase> action)
    {
        return (a1, a2, a3) =>
        {
            var model = fn(a1, a2, a3);
            action(model, ObjectBuilderBase.GetGenerator(fn) ?? throw new InvalidOperationException("unknown build function"));
            return model;
        };
    }

    public static BuildFunction<TModel> Modify<TModel>(this BuildFunction<TModel> fn, Action<TModel> action)
    {
        return () =>
        {
            var model = fn();
            action(model);
            return model;
        };
    }

    public static BuildFunction<TModel, TArg1> Modify<TModel, TArg1>(this BuildFunction<TModel, TArg1> fn, Action<TModel> action)
    {
        return (a1) =>
        {
            var model = fn(a1);
            action(model);
            return model;
        };
    }

    public static BuildFunction<TModel, TArg1, TArg2> Modify<TModel, TArg1, TArg2>(this BuildFunction<TModel, TArg1, TArg2> fn, Action<TModel> action)
    {
        return (a1, a2) =>
        {
            var model = fn(a1, a2);
            action(model);
            return model;
        };
    }

    public static BuildFunction<TModel, TArg1, TArg2, TArg3> Modify<TModel, TArg1, TArg2, TArg3>(this BuildFunction<TModel, TArg1, TArg2, TArg3> fn, Action<TModel> action)
    {
        return (a1, a2, a3) =>
        {
            var model = fn(a1, a2, a3);
            action(model);
            return model;
        };
    }

    public static BuildFunction<TProj> Select<TModel, TProj>(this BuildFunction<TModel> fn, Func<TModel, TProj> select)
    {
        return () =>
        {
            var model = fn();
            return select(model);
        };
    }

    public static BuildFunction<TProj, TArg1> Select<TModel, TArg1, TProj>(this BuildFunction<TModel, TArg1> fn, Func<TModel, TProj> select)
    {
        return (a1) =>
        {
            var model = fn(a1);
            return select(model);
        };
    }

    public static BuildFunction<TProj, TArg1, TArg2> Select<TModel, TArg1, TArg2, TProj>(this BuildFunction<TModel, TArg1, TArg2> fn, Func<TModel, TProj> select)
    {
        return (a1, a2) =>
        {
            var model = fn(a1, a2);
            return select(model);
        };
    }

    public static BuildFunction<TProj, TArg1, TArg2, TArg3> Select<TModel, TArg1, TArg2, TArg3, TProj>(this BuildFunction<TModel, TArg1, TArg2, TArg3> fn, Func<TModel, TProj> select)
    {
        return (a1, a2, a3) =>
        {
            var model = fn(a1, a2, a3);
            return select(model);
        };
    }

    public static BuildFunction<IEnumerable<TModel>> Many<TModel>(this BuildFunction<TModel> fn, int numElements)
    {
        return () => fn.Multiply(numElements);
    }

    public static BuildFunction<IEnumerable<TModel>, TArg1> Many<TModel, TArg1>(this BuildFunction<TModel, TArg1> fn, int numElements)
    {
        return (a1) => fn.Multiply(numElements, a1);
    }

    public static BuildFunction<IEnumerable<TModel>, TArg1, TArg2> Many<TModel, TArg1, TArg2>(this BuildFunction<TModel, TArg1, TArg2> fn, int numElements)
    {
        return (a1, a2) => fn.Multiply(numElements, a1, a2);
    }

    public static BuildFunction<IEnumerable<TModel>, TArg1, TArg2, TArg3> Many<TModel, TArg1, TArg2, TArg3>(this BuildFunction<TModel, TArg1, TArg2, TArg3> fn, int numElements)
    {
        return (a1, a2, a3) => fn.Multiply(numElements, a1, a2, a3);
    }

    private static IEnumerable<TModel> Multiply<TModel>(this BuildFunction<TModel> fn, int numElements)
    {
        for (int i = 0; i < numElements; i++)
        {
            yield return fn();
        }
    }

    private static IEnumerable<TModel> Multiply<TModel, TArg1>(this BuildFunction<TModel, TArg1> fn, int numElements, TArg1 arg1)
    {
        for (int i = 0; i < numElements; i++)
        {
            yield return fn(arg1);
        }
    }

    private static IEnumerable<TModel> Multiply<TModel, TArg1, TArg2>(this BuildFunction<TModel, TArg1, TArg2> fn, int numElements, TArg1 arg1, TArg2 arg2)
    {
        for (int i = 0; i < numElements; i++)
        {
            yield return fn(arg1, arg2);
        }
    }

    private static IEnumerable<TModel> Multiply<TModel, TArg1, TArg2, TArg3>(this BuildFunction<TModel, TArg1, TArg2, TArg3> fn, int numElements, TArg1 arg1, TArg2 arg2, TArg3 arg3)
    {
        for (int i = 0; i < numElements; i++)
        {
            yield return fn(arg1, arg2, arg3);
        }
    }
}