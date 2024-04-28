using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;
using System.Text.Json;
using MongoDB.Bson;

namespace Pulsar.BuildingBlocks.Utils;

public static partial class GeneralExtensions
{
    public static string GetOrThrow(this IConfiguration configuration, string key)
    {
        var v = configuration[key];
        if (v is null)
            throw new InvalidOperationException($"configuration key {key} not found");
        else
            return v;
    }
    public static string? Truncate(this string? text, int length)
    {
        if (text is null)
            return null;
        if (text.Length > length)
            return text.Substring(0, length);
        else
            return text;
    }
    public static IEnumerable<List<T>> Partition<T>(this IEnumerable<T> e, int partitionSize)
    {
        List<T>? list = null;
        foreach (var item in e)
        {
            if (list == null)
                list = new List<T>();

            if (list.Count >= partitionSize)
            {
                yield return list;
                list = new List<T>();
            }

            list.Add(item);
        }

        if (list != null && list.Count != 0)
            yield return list;
    }
    public static bool IsEmpty(this string? text) => string.IsNullOrWhiteSpace(text);
    public static bool IsNotEmpty(this string? text) => !string.IsNullOrWhiteSpace(text);
    public static string SHA256Hash(this string text)
    {
        using (var sha256Hash = SHA256.Create())
        {
            var data = sha256Hash.ComputeHash(Encoding.GetEncoding("ISO-8859-1").GetBytes(text));
            var sBuilder = new StringBuilder();

            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2").ToLowerInvariant());
            }

            return sBuilder.ToString();
        }
    }
    public static string GetSalt(int maximumSaltLength = 128)
    {
        var salt = RandomNumberGenerator.GetBytes(maximumSaltLength);
        var sBuilder = new StringBuilder();

        for (var i = 0; i < salt.Length; i++)
        {
            sBuilder.Append(salt[i].ToString("x2").ToLowerInvariant());
        }

        return sBuilder.ToString();
    }
    public static string? OnlyNumbers(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return new string(text.Where(c => char.IsDigit(c)).ToArray());
    }
    public static Dictionary<TKey, List<T>> MapBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> key) where TKey : notnull
    {
        Dictionary<TKey, List<T>> dic = new Dictionary<TKey, List<T>>();
        foreach (var item in items)
        {
            var k = key(item);
            if (!dic.ContainsKey(k))
                dic[k] = new List<T>();

            dic[k].Add(item);
        }
        return dic;
    }
    public static Dictionary<TKey, T> MapByUnique<T, TKey>(this IEnumerable<T> items, Func<T, TKey> key) where TKey : notnull
    {
        Dictionary<TKey, T> dic = new Dictionary<TKey, T>();
        foreach (var item in items)
        {
            var k = key(item);
            dic[k] = item;
        }
        return dic;
    }
    public static Dictionary<TKey, List<TValue>> MapBy<T, TKey, TValue>(this IEnumerable<T> items, Func<T, TKey> key, Func<T, TValue> value) where TKey : notnull
    {
        Dictionary<TKey, List<TValue>> dic = new Dictionary<TKey, List<TValue>>();
        foreach (var item in items)
        {
            var k = key(item);
            if (!dic.ContainsKey(k))
                dic[k] = new List<TValue>();

            dic[k].Add(value(item));
        }
        return dic;
    }
    public static Dictionary<TKey, TValue> MapByUnique<T, TKey, TValue>(this IEnumerable<T> items, Func<T, TKey> key, Func<T, TValue> value) where TKey : notnull
    {
        Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>();
        foreach (var item in items)
        {
            var k = key(item);
            dic[k] = value(item);
        }
        return dic;
    }
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue, T>(this IEnumerable<T> items, Func<T, (TKey, TValue)> keyValuePairs) where TKey : notnull
    {
        Dictionary<TKey, TValue> dic = new();
        foreach (var item in items)
        {
            var kv = keyValuePairs(item);
            dic[kv.Item1] = kv.Item2;
        }
        return dic;

    }
    public static string? Tokenize(this string? str) => Pulsar.BuildingBlocks.Utils.Tokenize.Perform(str);
    public static string ToSafeBase64(this byte[] bytes)
    {
        var s = Convert.ToBase64String(bytes);
        return s.Replace("z", "zn").Replace("+", "za").Replace("/", "zd").Replace("=", "ze");
    }
    public static byte[] FromSafeBase64(this string base64)
    {
        base64 = base64.Replace("za", "+").Replace("zd", "/").Replace("ze", "=").Replace("zn", "z");
        return Convert.FromBase64String(base64);
       
    }
    public static string ToBase64Json(this object obj)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ObjectIdConverter());
        var json = JsonSerializer.Serialize(obj, obj.GetType(), options);
        return Encoding.UTF8.GetBytes(json).ToSafeBase64();
    }
    
    public static string ToJsonString(this object obj)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ObjectIdConverter());
        return JsonSerializer.Serialize(obj, obj.GetType(), options);
    }
    
    public static T? FromBase64Json<T>(this string json)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ObjectIdConverter());
        var og = Encoding.UTF8.GetString(json.FromSafeBase64());
        return JsonSerializer.Deserialize<T>(og, options);
    }
    public static T? FromJsonString<T>(this string json)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ObjectIdConverter());
        return JsonSerializer.Deserialize<T>(json, options);
    }
    public static object? FromJsonString(this string json, Type type)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ObjectIdConverter());
        return JsonSerializer.Deserialize(json, type, options);
    }
    
    public static object? FromBase64Json(this string json, Type type)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ObjectIdConverter());
        var og = Encoding.UTF8.GetString(json.FromSafeBase64());
        return JsonSerializer.Deserialize(og, type, options);
    }
    public static int Limit(this int limit) => Math.Max(Math.Min(limit, 1000), 1);
    public static bool IsValidExtension(this string fn, params string[] validExtensions)
    {
        var ext = Path.GetExtension(fn).ToLowerInvariant();
        return validExtensions.Any(e => string.Compare(e, ext, true) == 0);
    }

    public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expr1.Body)!;

        var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expr2.Body)!;

        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(left, right), parameter);
    }

    private class ReplaceExpressionVisitor
        : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression? Visit(Expression? node)
        {
            if (node == _oldValue)
                return _newValue;
            return base.Visit(node);
        }
    }
}
