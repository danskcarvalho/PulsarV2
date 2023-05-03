using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.Contracts.Queries;
using Pulsar.Services.Shared.DTOs;

namespace Identity.UnitTests.Queries;

public class MockedUsuarioQueries : IUsuarioQueries
{
    public Task<PaginatedListDTO<UsuarioListadoDTO>> FindUsuarios(UsuarioFiltroDTO filtro)
    {
        throw new NotImplementedException();
    }

    public Task<BasicUserInfoDTO?> GetBasicUserInfo(string usuarioId, string? consistencyToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<BasicUserInfoDTO>> GetBasicUsersInfo(IEnumerable<string> usuarioIds, string? consistencyToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<UsuarioDetalhesDTO>> GetUsuarioDetalhes(IEnumerable<string> usuarioIds, string? consistencyToken)
    {
        throw new NotImplementedException();
    }

    public Task<UsuarioLogadoDTO?> GetUsuarioLogadoById(string usuarioId)
    {
        throw new NotImplementedException();
    }

    public Task<UsuarioLogadoDTO?> TestUsuarioCredentials(string? usernameOrEmail, string? password)
    {
        throw new NotImplementedException();
    }
}
