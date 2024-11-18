using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pulsar.Services.Shared
{
	public static class JsonExtensions
	{
		public static string ToJsonString(this object obj)
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new JsonStringEnumConverter());
			return JsonSerializer.Serialize(obj, obj.GetType(), options);
		}
		public static T? FromJsonString<T>(this string json)
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new JsonStringEnumConverter());
			return JsonSerializer.Deserialize<T>(json, options);
		}
		public static object? FromJsonString(this string json, Type type)
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new JsonStringEnumConverter());
			return JsonSerializer.Deserialize(json, type, options);
		}
	}
}
