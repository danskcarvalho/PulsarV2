using System.Xml.Linq;

namespace Pulsar.BuildingBlocks.EventBus.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class EventNameAttribute : Attribute
{
    public string Name { get; }

    public EventNameAttribute(string name)
    {
        this.Name = name;
    }

    public static string? GetEventName(Type type)
    {
        return type.GetCustomAttributes(typeof(EventNameAttribute), true).Cast<EventNameAttribute>().FirstOrDefault()?.Name;
    }
    public static string? GetEventName<T>() => GetEventName(typeof(T));

    private static Dictionary<string, Type> _eventNames = new Dictionary<string, Type>();
    private static bool _hasBuiltEventDictionary = false;
    public static Type? GetTypeByEventName(string eventName)
    {
        lock (_eventNames)
        {
            if (!_hasBuiltEventDictionary)
            {
                return _eventNames.ContainsKey(eventName) ? _eventNames[eventName] : null;
            }
            else
            {
                BuildEventDictionary();
                return _eventNames.ContainsKey(eventName) ? _eventNames[eventName] : null;
            }
        }
    }

    private static void BuildEventDictionary()
    {
        _hasBuiltEventDictionary = true;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var tt in assembly.GetTypes().Where(t => t.IsClass && !t.IsGenericTypeDefinition))
            {
                var n = GetEventName(tt);
                if (n is not null)
                    _eventNames[n] = tt;
            }
        }
    }
}
