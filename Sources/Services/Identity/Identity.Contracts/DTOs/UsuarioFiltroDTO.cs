namespace Pulsar.Services.Identity.Contracts.DTOs;

public class UsuarioFiltroDTO
{
    /// <summary>
    /// Filtro utilizado para filtrar os usuários. Opcional.
    /// </summary>
    public string? Filtro { get; set; }
    /// <summary>
    /// Cursor anterior para listar os próximos N usuários. Opcional.
    /// </summary>
    public string? Cursor { get; set; }
    /// <summary>
    /// Quantos usuários devem ser retornados. Opcional. Valor padrão é 50.
    /// </summary>
    public int? Limit { get; set; }
    /// <summary>
    /// Token de consistência. Opcional.
    /// </summary>
    public string? ConsistencyToken { get; set; }
}