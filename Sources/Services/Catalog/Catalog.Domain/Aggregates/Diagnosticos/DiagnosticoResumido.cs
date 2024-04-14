namespace Pulsar.Services.Catalog.Domain.Aggregates.Diagnosticos
{
    public class DiagnosticoResumido
    {
        public ObjectId DiagnosticoId { get; set; }
        public TipoDiagnostico Tipo { get; set; }
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public Sexo? Sexo { get; set; }

        [BsonConstructor]
        public DiagnosticoResumido(ObjectId diagnosticoId, TipoDiagnostico tipo, string codigo, string nome, Sexo? sexo)
        {
            DiagnosticoId = diagnosticoId;
            Tipo = tipo;
            Codigo = codigo;
            Nome = nome;
            Sexo = sexo;
        }
    }
}
