using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.Contracts.Queries;

public interface IUsuarioQueries
{
    Task<UsuarioLogadoDTO?> TestUsuarioCredentials(string? usernameOrEmail, string? password);
    Task<BasicUserInfoDTO?> GetBasicUserInfo(string usuarioId, string? consistencyToken);
    Task<List<BasicUserInfoDTO>> GetBasicUsersInfo(IEnumerable<string> usuarioIds, string? consistencyToken);
    Task<List<UsuarioDetalhesDTO>> GetUsuarioDetalhes(IEnumerable<string> usuarioIds, string? consistencyToken);
    Task<UsuarioLogadoDTO?> GetUsuarioLogadoById(string usuarioId);
    Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuarios(UsuarioFiltroDTO filtro);
}
