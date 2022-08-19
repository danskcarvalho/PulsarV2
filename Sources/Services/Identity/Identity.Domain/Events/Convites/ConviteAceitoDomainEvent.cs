using Pulsar.Services.Identity.Domain.Aggregates.Convites;

namespace Pulsar.Services.Identity.Domain.Events.Convites;

public class ConviteAceitoDomainEvent : INotification
{
    public ObjectId DominioId { get; set; }
    public ObjectId ConviteId { get; set; }
    public ObjectId UsuarioId { get; set; }
    public string PrimeiroNome { get; set; }
    public string? Sobrenome { get; set; }
    public string NomeUsuario { get; set; }
    public string Senha { get; set; }
    public string Email { get; set; }
    public bool AdministrarDominio { get; set; }
    public List<ConviteGrupo> Grupos { get; set; }
    public ObjectId? CriadoPorUsuarioId { get; }

    public ConviteAceitoDomainEvent(ObjectId dominioId, ObjectId conviteId, ObjectId usuarioId, string primeiroNome, string? sobrenome, string nomeUsuario, string senha, string email, bool administrarDominio, 
        IEnumerable<ConviteGrupo> grupos, ObjectId? criadoPorUsuarioId)
    {
        DominioId = dominioId;
        ConviteId = conviteId;
        UsuarioId = usuarioId;
        PrimeiroNome = primeiroNome;
        Sobrenome = sobrenome;
        NomeUsuario = nomeUsuario;
        Senha = senha;
        Email = email;
        AdministrarDominio = administrarDominio;
        Grupos = grupos.ToList();
        CriadoPorUsuarioId = criadoPorUsuarioId;
    }
}
