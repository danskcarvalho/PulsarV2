using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Reflection;

namespace ServiceBus.Migrations.Core;

public class Migrator
{
    private List<Assembly> _assemblyList = new List<Assembly>();
    public IConfiguration Configuration { get; }
    public IReadOnlyList<Assembly> Assemblies { get; }

    public Migrator(IConfiguration configuration)
    {
        Configuration = configuration;
        Assemblies = _assemblyList.AsReadOnly();
    }

    public Migrator AddAssembly(Assembly assembly)
    {
        _assemblyList.Add(assembly);
        return this;
    }

    public async Task Migrate()
    {
        var funcEnum = new AzureFunctionEnumerator(Assemblies);
        var creator = new TopicSubscriptionCreator(Configuration);

        int num = 0;
        int total = funcEnum.Count();

        foreach (var func in funcEnum)
        {
            num++;
            try
            {
                await creator.Create(func);
                Print($"[{num}/{total}] azure function {func.FunctionName} was migrated", ConsoleColor.Green);
            }
            catch (Exception e)
            {
                Print($"[{num}/{total}] azure function {func.FunctionName} migration failed", ConsoleColor.Red);
                Print(ToJson(e), ConsoleColor.Red);
            }
        }
    }

    private static string ToJson(Exception e)
    {
        try
        {
            return JsonConvert.SerializeObject(e);
        }
        catch
        {
            return JsonConvert.SerializeObject(new
            {
                e.Message,
                e.StackTrace
            });
        }
    }

    private static void Print(string msg, ConsoleColor color)
    {
        var previousForegroundColor = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
        }
        finally
        {
            Console.ForegroundColor = previousForegroundColor;
        }
    }
}
