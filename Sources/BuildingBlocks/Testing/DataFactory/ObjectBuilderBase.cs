using System.Runtime.CompilerServices;

namespace Pulsar.BuildingBlocks.DataFactory
{
    public abstract class ObjectBuilderBase
    {
        private static readonly ConditionalWeakTable<object, GeneratorBase> _generators = new ConditionalWeakTable<object, GeneratorBase>();
        internal abstract object CreateAutoCompletedObject();
        internal static GeneratorBase? GetGenerator(object buildFunction)
        {
            if (_generators.TryGetValue(buildFunction, out var generator))
                return generator;
            else
                return null;
        }
        protected static void SetGenerator(object buildFunction, GeneratorBase generator)
        {
            _generators.Add(buildFunction, generator);
        }
    }
}