using Microsoft.Azure.Functions.Worker;
using System.Collections;
using System.Collections.Immutable;
using System.Reflection;

namespace ServiceBus.Migrations.Core
{
    public class AzureFunctionEnumerator : IEnumerable<AzureFunctionInformation>
    {
        private readonly AzureFunctionInspector inspector = new AzureFunctionInspector();
        public IReadOnlyList<Assembly> Assemblies { get; set; }

        public AzureFunctionEnumerator(IEnumerable<Assembly> assemblies)
        {
            Assemblies = assemblies.ToImmutableList();
        }

        public IEnumerator<AzureFunctionInformation> GetEnumerator()
        {
            var types = from assembly in Assemblies
                        from assemblyType in assembly.GetTypes()
                        where assemblyType.IsClass && assemblyType.GetMethods().Any(m => m.GetCustomAttribute<FunctionAttribute>() is not null)
                        select assemblyType;

            foreach (var type in types)
            {
                var info = inspector.GetInformation(type);
                if (info != null)
                    yield return info;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
