using Pulsar.Services.Identity.Domain.Aggregates.Dominios;
using Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

namespace Pulsar.Services.Identity.Domain.Specifications;

public class FindUsuariosBloqueadosDominioSpec : IFindSpecification<Usuario, ObjectId>
{
    public FindUsuariosBloqueadosDominioSpec(ObjectId dominioId, ObjectId? usuarioId)
    {
        DominioId = dominioId;
        UsuarioId = usuarioId;
    }

    public ObjectId DominioId { get; }
    public ObjectId? UsuarioId { get; }
    public FindSpecification<Usuario, ObjectId> GetSpec()
    {
        if (UsuarioId.HasValue)
            return Find.Where<Usuario>(u => u.DominiosBloqueados.Contains(DominioId) && u.Id == UsuarioId).Select(x => x.Id).Build();
        else

            return Find.Where<Usuario>(u => u.DominiosBloqueados.Contains(DominioId)).Select(x => x.Id).Build();
    }
}
