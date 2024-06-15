using Microsoft.OpenApi.Models;
using Pulsar.Services.Shared.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Globalization;
using System.Reflection;

namespace Pulsar.Services.Shared.API.Filters;

public class SwaggerExcludeFilter : ISchemaFilter
{
    #region ISchemaFilter Members

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties == null || schema.Properties.Count == 0 || context?.Type == null)
            return;

        var excludedProperties = context.Type.GetProperties().Where(t => t.GetCustomAttribute<SwaggerExcludeAttribute>() != null);

        foreach (var excludedProperty in excludedProperties)
        {
            foreach (var propKey in schema.Properties.Keys.ToList())
            {
                if (string.Compare(propKey, excludedProperty.Name, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase) == 0)
                    schema.Properties.Remove(propKey);
            }
        }
    }

    #endregion
}
