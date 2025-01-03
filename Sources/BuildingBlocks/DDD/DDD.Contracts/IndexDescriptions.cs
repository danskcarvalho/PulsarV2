﻿using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Pulsar.BuildingBlocks.DDD.Abstractions;

namespace Pulsar.BuildingBlocks.DDD;


public abstract class IndexDescriptions
{
    public Dictionary<string, IX> AllIndexes()
    {
        Dictionary<string, IX> result = new Dictionary<string, IX>();
        var keys = this.GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(IX))
            .Select(f => (f.Name, IX: f.GetValue(null) as IX));
        foreach (var k in keys)
        {
            result[k.Name.ToLowerInvariant()] = k.IX ?? throw new InvalidOperationException("null index");
        }
        return result;
    }

    public static IEnumerable<IndexDescriptions> AllDescriptors(Assembly assembly)
    {
        return assembly.GetTypes()
                        .Where(t => typeof(IndexDescriptions).IsAssignableFrom(t) && !t.IsAbstract)
                        .Select(t => (IndexDescriptions?)Activator.CreateInstance(t))
                        .Cast<IndexDescriptions>();
    }

    public abstract string CollectionName { get; }
    public abstract Type ModelType { get; }
}

public abstract class IndexDescriptions<TModel> : IndexDescriptions where TModel : class, IAggregateRoot
{
    static IndexDescriptions()
    {
        _ImplementationType = null;
        _Lock = new();
		IsSyncPending_v1 = Describe.Ascending(x => x.IsSyncPending);
	}
    
    private static Type? _ImplementationType;
    private static readonly object _Lock;
	// Index for IsSyncPending
	public static IX IsSyncPending_v1;

	protected static IndexBuilder<TModel> Describe
    {
        get
        {
            var type = GetImplementationType();
            var desc = (IndexDescriptionsImplementation<TModel>?)Activator.CreateInstance(type);
            return desc == null ? throw new InvalidOperationException("no implementation for index descriptions") : desc.CreateBuilder();
        }
    }

    private static Type GetImplementationType()
    {
        if (_ImplementationType == null)
        {
            lock (_Lock)
            {
                if (_ImplementationType == null)
                {
                    _ImplementationType = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetCustomAttribute<IndexDescriptionsImplementationAttribute>() is not null)
                        .Select(a => a.GetCustomAttribute<IndexDescriptionsImplementationAttribute>()!.Type)
                        .First()
                        .MakeGenericType(typeof(TModel));
                }
            }
        }
        return _ImplementationType ?? throw new InvalidOperationException("no implementation for index descriptions");
    }

    public sealed override Type ModelType => typeof(TModel);
}

public abstract class IndexDescriptionsImplementation<TModel>
{
    public abstract IndexBuilder<TModel> CreateBuilder();
}

public abstract class IX
{

}

public abstract class IndexBuilder<TModel> : IX
{
    public IndexBuilder() { }

    public abstract bool? IsUnique { get; }

    public abstract IndexBuilder<TModel> Ascending(Expression<Func<TModel, object?>> field);
    public abstract IndexBuilder<TModel> Descending(Expression<Func<TModel, object?>> field);
    public abstract IndexBuilder<TModel> Text(Expression<Func<TModel, object?>> field);
    public abstract IndexBuilder<TModel> Hashed(Expression<Func<TModel, object?>> field);
    public abstract IndexBuilder<TModel> Ascending(string field);
    public abstract IndexBuilder<TModel> Descending(string field);
    public abstract IndexBuilder<TModel> Text(string field);
    public abstract IndexBuilder<TModel> Hashed(string field);
    public abstract IndexBuilder<TModel> Unique();

}

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
public sealed class IndexDescriptionsImplementationAttribute : Attribute
{
    public IndexDescriptionsImplementationAttribute(Type type)
    {
        this.Type = type;
    }

    public Type Type { get; }
}
