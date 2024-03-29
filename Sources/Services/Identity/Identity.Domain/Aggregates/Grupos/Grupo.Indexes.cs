﻿namespace Pulsar.Services.Identity.Domain.Aggregates.Grupos;

public partial class Grupo
{
    public class Indexes : IndexDescriptions<Grupo>
    {
        public static IX TermosBusca_Nome_Id_v1 = Describe.Text(g => g.TermosBusca).Ascending(g => g.Nome).Ascending(g => g.Id);
        public static IX DominioId_RemovidoEm_Nome_Id_v1 = Describe.Ascending(g => g.DominioId).Ascending(g => g.AuditInfo.RemovidoEm).Ascending(g => g.Nome).Ascending(g => g.Id);
        public static IX SubGrupos_UsuarioId_RemovidoEm_v1 = Describe.Ascending("SubGrupos.UsuarioIds").Ascending(g => g.AuditInfo.RemovidoEm);

        public override string CollectionName => Constants.CollectionNames.GRUPOS;
    }
}
