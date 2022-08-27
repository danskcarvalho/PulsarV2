namespace Pulsar.Services.Identity.Domain.Aggregates.Grupos;

public interface IGrupoRepository : IRepository<IGrupoRepository, Grupo>
{
    public Task AtualizarNumUsuarios(ObjectId usuarioLogadoId, ObjectId grupoId, List<ObjectId>? subgrupoIds);
}
