using Pulsar.Services.Shared.DTOs;

namespace Pulsar.Services.Identity.Contracts.Queries;

public interface IUsuarioQueries
{
    Task<UsuarioLogadoDTO?> TestUsuarioCredentials(string? usernameOrEmail, string? password);
    Task<BasicUserInfoDTO?> GetBasicUserInfo(string usuarioId);
    Task<UsuarioLogadoDTO?> GetUsuarioLogadoById(string usuarioId);
    Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuarios(UsuarioFiltroDTO filtro);
}
