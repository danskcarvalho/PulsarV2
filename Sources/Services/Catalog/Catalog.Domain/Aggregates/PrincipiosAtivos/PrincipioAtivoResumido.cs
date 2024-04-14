namespace Pulsar.Services.Catalog.Domain.Aggregates.PrincipiosAtivos;
public class PrincipioAtivoResumido
{
    public ObjectId PrincipioAtivoId { get; set; }
    public string Nome { get; set; }
    public string CodigoEsus { get; set; }
    public CategoriaMedicamento Categoria { get; set; }
    public TipoMedicamento Tipo { get; set; }

    [BsonConstructor]
    public PrincipioAtivoResumido(ObjectId principioAtivoId, string nome, string codigoEsus, CategoriaMedicamento categoria, TipoMedicamento tipo)
    {
        PrincipioAtivoId = principioAtivoId;
        Nome = nome;
        CodigoEsus = codigoEsus;
        Categoria = categoria;
        Tipo = tipo;
    }
}

