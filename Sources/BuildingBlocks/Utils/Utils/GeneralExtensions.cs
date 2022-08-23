using MongoDB.Bson;
using System.Text.Json;

namespace Pulsar.BuildingBlocks.Utils;

public static class GeneralExtensions
{
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
        return s.Replace("z", "zz").Replace("+", "za").Replace("/", "zd").Replace("=", "ze");
    }
    public static byte[] FromSafeBase64(this string base64)
    {
        base64 = base64.Replace("za", "+").Replace("zd", "/").Replace("ze", "=").Replace("zz", "z");
        return Convert.FromBase64String(base64);
       
    }
    public static string ToBase64Json(this object obj)
    {
        var json = JsonSerializer.Serialize(obj, obj.GetType(), new JsonSerializerOptions());
        return Encoding.UTF8.GetBytes(json).ToSafeBase64();
    }
    public static T? FromBase64Json<T>(this string json)
    {
        var og = Encoding.UTF8.GetString(json.FromSafeBase64());
        return JsonSerializer.Deserialize<T>(og, new JsonSerializerOptions());
    }
    public static BsonDocument ToTextSearch(this string? term)
    {
        if (term == null || term.IsEmpty())
            return new BsonDocument();

        var parts = term.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var searchTerms = new List<object>();
        foreach (var p in parts)
        {
            if (p.Length < 3)
                continue;

            searchTerms.Add(new BsonDocument { { "$text", new BsonDocument { { "$search", p } } } });
        }

        return new BsonDocument
        {
            { "$and", new BsonArray(searchTerms.ToArray()) }
        };
    }
    public static int Limit(this int limit) => Math.Max(Math.Min(limit, 1000), 1);
}
