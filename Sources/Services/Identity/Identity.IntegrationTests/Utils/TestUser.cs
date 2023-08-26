namespace Identity.IntegrationTests.Utils;

public class TestUser
{
    public string Id { get; set; }
    public string? EstabelecimentoId { get; set; }
    public string? DominioId { get; set; }

    public TestUser(string id, string? estabelecimentoId, string? dominioId)
    {
        Id = id;
        EstabelecimentoId = estabelecimentoId;
        DominioId = dominioId;
    }
}
