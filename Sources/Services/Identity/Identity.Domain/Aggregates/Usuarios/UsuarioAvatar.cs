namespace Pulsar.Services.Identity.Domain.Aggregates.Usuarios;

public class UsuarioAvatar : ValueObject
{
    public string PublicUrl { get; private set; }
    public string PrivateUrl { get; private set; }

    public UsuarioAvatar(string publicUrl, string privateUrl)
    {
        PublicUrl = publicUrl ?? throw new ArgumentNullException(nameof(publicUrl));
        PrivateUrl = privateUrl ?? throw new ArgumentNullException(nameof(privateUrl));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PublicUrl;
        yield return PrivateUrl;
    }
}
