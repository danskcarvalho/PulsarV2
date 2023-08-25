using Pulsar.Services.Identity.Migrations.Models;

namespace Pulsar.Services.Identity.Migrations.Schema;

[Migration(20230419224200)]
public class AddIndexesForKeyStoreAndGrantStore : Migration
{
    public override async Task Up()
    {
        if (!(await this.Database.CollectionExists("_KeyStore")))
            await this.Database.CreateCollectionAsync("_KeyStore");

        if (!(await this.Database.CollectionExists("_GrantStore")))
            await this.Database.CreateCollectionAsync("_GrantStore");

        var grantCol = this.Database.GetCollection<GrantStoreModel>("_GrantStore");
        var IX_PersistedGrants_ConsumedTime = Builders<GrantStoreModel>.IndexKeys.Ascending(x => x.ConsumedTime);
        var IX_PersistedGrants_Expiration = Builders<GrantStoreModel>.IndexKeys.Ascending(x => x.Expiration);
        var IX_PersistedGrants_Key = Builders<GrantStoreModel>.IndexKeys.Ascending(x => x.Key);
        var IX_PersistedGrants_SubjectId_ClientId_Type = Builders<GrantStoreModel>.IndexKeys.Ascending(x => x.SubjectId).Ascending(x => x.ClientId).Ascending(x => x.Type);
        var IX_PersistedGrants_SubjectId_SessionId_Type = Builders<GrantStoreModel>.IndexKeys.Ascending(x => x.SubjectId).Ascending(x => x.SessionId).Ascending(x => x.Type);

        await grantCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<GrantStoreModel>(IX_PersistedGrants_ConsumedTime, new CreateIndexOptions()
        {
            Name = "IX_PersistedGrants_ConsumedTime"
        }));
        await grantCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<GrantStoreModel>(IX_PersistedGrants_Expiration, new CreateIndexOptions()
        {
            Name = "IX_PersistedGrants_Expiration"
        }));
        await grantCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<GrantStoreModel>(IX_PersistedGrants_Key, new CreateIndexOptions()
        {
            Name = "IX_PersistedGrants_Key"
        }));
        await grantCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<GrantStoreModel>(IX_PersistedGrants_SubjectId_ClientId_Type, new CreateIndexOptions()
        {
            Name = "IX_PersistedGrants_SubjectId_ClientId_Type"
        }));
        await grantCol.Indexes.CreateOneAsync(new MongoDB.Driver.CreateIndexModel<GrantStoreModel>(IX_PersistedGrants_SubjectId_SessionId_Type, new CreateIndexOptions()
        {
            Name = "IX_PersistedGrants_SubjectId_SessionId_Type"
        }));
    }
}
