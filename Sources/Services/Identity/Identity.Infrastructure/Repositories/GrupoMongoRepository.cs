using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Pulsar.BuildingBlocks.DDD.Abstractions;
using Pulsar.BuildingBlocks.Utils.Bson;
using System.Diagnostics.Metrics;

namespace Pulsar.Services.Identity.Infrastructure.Repositories
{
    public class GrupoMongoRepository : MongoRepository<IGrupoRepository, Grupo>, IGrupoRepository
    {
        public GrupoMongoRepository(MongoDbSession? session, MongoDbSessionFactory sessionFactory) : base(session, sessionFactory)
        {
        }

        protected override string CollectionName => Constants.CollectionNames.GRUPOS;

        public async Task AtualizarNumUsuarios(ObjectId usuarioLogadoId, ObjectId grupoId, List<ObjectId>? subgrupoIds)
        {
            var usuarioCollection = this.GetCollection<Usuario>(Constants.CollectionNames.USUARIOS);
            if (subgrupoIds == null || subgrupoIds.Count == 0)
            {
                var numUsuarios = await usuarioCollection.AsQueryable(this.Session!.CurrentHandle).Where(u => u.Grupos.Any(g => g.GrupoId == grupoId)).CountAsync();
                var updateGrupo = new UpdateSpecificationWrapper<Grupo>(
                    Update
                        .Where<Grupo>(g => g.Id == grupoId)
                        .Set(g => g.NumUsuarios, numUsuarios)
                        .Set(g => g.AuditInfo.EditadoPorUsuarioId, usuarioLogadoId)
                        .Set(g => g.AuditInfo.EditadoEm, DateTime.UtcNow)
                        .Inc(g => g.Version, 1)
                        .Build());
                await this.UpdateOneAsync(updateGrupo);
            }
            else
            {
                var numUsuarios = await usuarioCollection.AsQueryable(this.Session!.CurrentHandle).Where(u => u.Grupos.Any(g => g.GrupoId == grupoId)).CountAsync();
                Dictionary<ObjectId, int> numUsuariosSubGrupos = new Dictionary<ObjectId, int>();
                foreach (var subgrupoId in subgrupoIds)
                {
                    var nu = await usuarioCollection.AsQueryable(this.Session!.CurrentHandle).Where(u => u.Grupos.Any(g => g.GrupoId == grupoId && g.SubGrupoId == subgrupoId)).CountAsync();
                    numUsuariosSubGrupos[subgrupoId] = nu;
                }

                var setDocument = new Dictionary<string, object>
                {
                    ["NumUsuarios"] = numUsuarios,
                    ["AuditInfo.EditadoPorUsuarioId"] = usuarioLogadoId,
                    ["AuditInfo.EditadoEm"] = DateTime.UtcNow
                };

                foreach (var sg in numUsuariosSubGrupos)
                {
                    setDocument[$"SubGrupos.$[elem{sg.Key}].NumUsuarios"] = sg.Value;
                }

                var updateDocument = BSON.Create(b =>
                {
                    return new Dictionary<string, object>
                    {
                        ["$set"] = setDocument
                    };
                });
                var arrayFilters = new List<ArrayFilterDefinition>();
                foreach (var sg in numUsuariosSubGrupos)
                {
                    ArrayFilterDefinition<SubGrupo> optionsFilter = BSON.Create(b => new Dictionary<string, object>
                    {
                        [$"elem{sg.Key}.SubGrupoId"] = b.Eq(sg.Key)
                    });
                    arrayFilters.Add(optionsFilter);
                }

                var updateOptions = new UpdateOptions()
                {
                    ArrayFilters = arrayFilters
                };
                await Collection.UpdateOneAsync(this.Session!.CurrentHandle, g => g.Id == grupoId, updateDocument, updateOptions);
            }
        }

        protected override IGrupoRepository Clone(MongoDbSession? session, MongoDbSessionFactory sessionFactory)
        {
            return new GrupoMongoRepository(session, sessionFactory);
        }
    }
}
