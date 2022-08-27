using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections;

namespace Pulsar.BuildingBlocks.Utils.Bson;

public static class BSON
{
    public static BsonDocument Create(Func<BSONCreatorHelper, object> f)
    {
        return (BsonDocument)Serialize(f(new BSONCreatorHelper()));
    }

    private static object Serialize(object? obj)
    {
        if (obj is BsonDocument)
            return obj;
        else if (obj is BsonValue)
            return obj;
        else if (obj is ObjectId oi)
            return new BsonObjectId(oi);
        else if (obj is null)
            return BsonNull.Value;
        else if (obj is string s)
            return new BsonString(s);
        else if (obj is DateTime dt)
            return new BsonDateTime(dt);
        else if (obj is IDictionary dicInput)
        {
            Dictionary<string, object?> dic = new Dictionary<string, object?>();
            foreach (var prop in dicInput.Keys)
            {
                var propName = prop.ToString()!;
                dic[propName] = Serialize(dicInput[prop]);
            }
            return new BsonDocument(dic);
        }
        else if (obj is IEnumerable en)
            return new BsonArray(en.Cast<object>().Select(x => Serialize(x)));
        else if (obj is bool b)
            return new BsonBoolean(b);
        else if (obj is int i32)
            return new BsonInt32(i32);
        else if (obj is long i64)
            return new BsonInt64(i64);
        else if (obj is double d)
            return new BsonDouble(d);
        else
        {
            Dictionary<string, object?> dic = new Dictionary<string, object?>();
            foreach (var prop in obj.GetType().GetProperties())
            {
                dic[prop.Name] = Serialize(prop.GetValue(obj));
            }
            return new BsonDocument(dic);
        }
    }
}

public class BSONCreatorHelper
{
    public BsonDocument And(params object?[] conds)
    {
        return new BsonDocument
        {
            { "$and", new BsonArray(conds.Select(x => Serialize(x))) }
        };
    }

    public BsonDocument Or(params object?[] conds)
    {
        return new BsonDocument
        {
            { "$or", new BsonArray(conds.Select(x => Serialize(x))) }
        };
    }

    public BsonDocument Gt(object? v)
    {
        return new BsonDocument
        {
            { "$gt", (BsonValue)Serialize(v) }
        };
    }
    public BsonDocument Lt(object? v)
    {
        return new BsonDocument
        {
            { "$lt", (BsonValue)Serialize(v) }
        };
    }

    public BsonDocument Eq(object? v)
    {
        return new BsonDocument
        {
            { "$eq", (BsonValue)Serialize(v) }
        };
    }

    public BsonDocument Ne(object? v)
    {
        return new BsonDocument
        {
            { "$ne", (BsonValue)Serialize(v) }
        };
    }

    public BsonDocument In(params object?[] values)
    {
        return new BsonDocument
        {
            { "$in", new BsonArray(values.Select(x => Serialize(x))) }
        };
    }

    private object Serialize(object? obj)
    {
        if (obj is BsonDocument)
            return obj;
        else if (obj is BsonValue)
            return obj;
        else if (obj is ObjectId oi)
            return new BsonObjectId(oi);
        else if (obj is null)
            return BsonNull.Value;
        else if (obj is string s)
            return new BsonString(s);
        else if (obj is DateTime dt)
            return new BsonDateTime(dt);
        else if (obj is IDictionary dicInput)
        {
            Dictionary<string, object?> dic = new Dictionary<string, object?>();
            foreach (var prop in dicInput.Keys)
            {
                var propName = prop.ToString()!;
                dic[propName] = Serialize(dicInput[prop]);
            }
            return new BsonDocument(dic);
        }
        else if (obj is IEnumerable en)
            return new BsonArray(en.Cast<object>().Select(x => Serialize(x)));
        else if (obj is bool b)
            return new BsonBoolean(b);
        else if (obj is int i32)
            return new BsonInt32(i32);
        else if (obj is long i64)
            return new BsonInt64(i64);
        else if (obj is double d)
            return new BsonDouble(d);
        else
        {
            Dictionary<string, object?> dic = new Dictionary<string, object?>();
            foreach (var prop in obj.GetType().GetProperties())
            {
                dic[prop.Name] = Serialize(prop.GetValue(obj));
            }
            return new BsonDocument(dic);
        }
    }
}
