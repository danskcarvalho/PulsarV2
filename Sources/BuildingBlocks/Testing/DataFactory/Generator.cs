using MongoDB.Bson;
using System.Linq.Expressions;

namespace Pulsar.BuildingBlocks.DataFactory;

public class Generator<TModel> : GeneratorBase
{
    internal Generator(Random rng) : base(rng)
    {
    }

    public Func<TArg1, TArg2, TArg3, TModel> With<TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, TModel> generator) => (a, b, c) => generator(a, b, c);
    public Func<TArg1, TArg2, TModel> With<TArg1, TArg2>(Func<TArg1, TArg2, TModel> generator) => (a, b) => generator(a, b);
    public Func<TArg1, TModel> With<TArg1>(Func<TArg1, TModel> generator) => a => generator(a);
}