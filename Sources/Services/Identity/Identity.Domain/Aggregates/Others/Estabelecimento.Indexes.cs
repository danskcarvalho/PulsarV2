﻿namespace Pulsar.Services.Identity.Domain.Aggregates.Others;

public partial class Estabelecimento
{
    public class Indexes : IndexDescriptions<Estabelecimento>
    {
        public static IX Redes_v1 = Describe.Ascending(e => e.Redes);

        public override string CollectionName => Constants.CollectionNames.ESTABELECIMENTOS;
    }
}
