using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.Services.Catalog.Migrations
{
    public static class Extensions
    {
        public static T FromBson<T>(this string str)
        {
            return BsonSerializer.Deserialize<T>(str);
        }
        public static string ToBson<T>(this T obj)
        {
            return obj.ToBsonDocument().ToJson();
        }
    }
}
