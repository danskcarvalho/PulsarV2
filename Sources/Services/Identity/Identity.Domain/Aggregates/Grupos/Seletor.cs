namespace Pulsar.Services.Identity.Domain.Aggregates.Grupos;

public class Seletor : ValueObject
{
    public ObjectId? EstabelecimentoId { get; set; }
    public ObjectId? RedeEstabelecimentoId { get; set; }

    [BsonConstructor]
    public Seletor(ObjectId? estabelecimentoId, ObjectId? redeEstabelecimentoId)
    {
        EstabelecimentoId = estabelecimentoId;
        RedeEstabelecimentoId = redeEstabelecimentoId;
        if (estabelecimentoId == null && redeEstabelecimentoId == null)
            throw new InvalidOperationException("estabelecimentoId e redeEstabelecimentoId são nulos");
    }

    public static Seletor Estabelecimento(ObjectId estabelecimentoId) => new Seletor(estabelecimentoId, null);
    public static Seletor RedeEstabelecimento(ObjectId redeEstabelecimentoId) => new Seletor(null, redeEstabelecimentoId);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return EstabelecimentoId;
        yield return RedeEstabelecimentoId;
    }
}
