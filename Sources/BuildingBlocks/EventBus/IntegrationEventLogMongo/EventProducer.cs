using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.BuildingBlocks.IntegrationEventLogMongo
{
    public class EventProducer
    {
        [BsonId]
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime CheckedOn { get; set; }
    }
}
