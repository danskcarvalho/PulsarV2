using Duende.IdentityServer.Models;

namespace Pulsar.Services.Identity.API.Stores;

public class MongoPersistedGrant : PersistedGrant
{
    public MongoPersistedGrant(PersistedGrant grant)
    {
        this.Id = ObjectId.GenerateNewId();
        this.Data = grant.Data;
        this.ClientId = grant.ClientId;
        this.SubjectId = grant.SubjectId;
        this.ConsumedTime = grant.ConsumedTime;
        this.CreationTime = grant.CreationTime;
        this.Description = grant.Description;
        this.Expiration = grant.Expiration;
        this.Key = grant.Key;
        this.SessionId = grant.SessionId;
        this.Type = grant.Type;
    }
    public MongoPersistedGrant()
    {
    }

    public ObjectId Id { get; set; }
}