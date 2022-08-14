using Microsoft.AspNetCore.Components;
using Pulsar.Services.Identity.Contracts.DTOs;
using Pulsar.Services.Identity.UI.Models;

namespace Pulsar.Services.Identity.UI.Pages;

public partial class Login
{
    [Parameter, SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    LoginModel _model = new();
    UsuarioLogadoDTO? _usuarioLogado;
    UsuarioLogadoDTO.DominioDTO? _dominioCorrente;
    bool _pulouDominio = false;
    bool _loading = false;

    async Task ContinueFromUsuarioSenha()
    {
        _loading = true;
        try
        {
            _usuarioLogado = await LoginClient.TestCredentials(new UsuarioSenhaDTO()
            {
                UsernameOrEmail = _model.UsernameOrEmail,
                Senha = _model.Password
            });

            if (_usuarioLogado == null)
            {
                _model.ErrorMessage = "O usuário ou senha informados estão incorretos.";
                return;
            }
            else if (!_usuarioLogado.IsAtivo)
            {

                _model.ErrorMessage = "O usuário informado está bloqueado.";
                return;
            }

            await Next();
        }
        catch (BackendException e)
        {
            ResetEverything();
            _model.ErrorMessage = e.Message;
        }
        finally
        {
            _loading = false;
        }
    }

    async Task Next()
    {
        if (_usuarioLogado == null)
            return;
        if (_usuarioLogado.IsSuperUsuario)
        {
            var result = (await LoginClient.Login(new LoginDTO()
            {
                ReturnUrl = ReturnUrl,
                UsernameOrEmail = _model.UsernameOrEmail,
                Senha = _model.Password
            }))!;
            if (result.Erro != null)
            {
                ResetEverything();
                _model.ErrorMessage = result.Erro;
            }
            else if (result.RedirectUrl != null)
            {
                NavManager.NavigateTo(result.RedirectUrl, forceLoad: true);
            }
        }
        else if (PodeLogarDominio(_usuarioLogado) is UsuarioLogadoDTO.DominioDTO dom1)
        {
            var result = (await LoginClient.Login(new LoginDTO()
            {
                ReturnUrl = ReturnUrl,
                UsernameOrEmail = _model.UsernameOrEmail,
                Senha = _model.Password,
                DominioId = dom1.Id
            }))!;
            if (result.Erro != null)
            {
                ResetEverything();
                _model.ErrorMessage = result.Erro;
            }
            else if (result.RedirectUrl != null)
            {
                NavManager.NavigateTo(result.RedirectUrl, forceLoad: true);
            }
        }
        else if (PodeLogarEstabelecimento(_usuarioLogado) is (UsuarioLogadoDTO.DominioDTO dom2, UsuarioLogadoDTO.EstabelecimentoDTO est2))
        {
            var result = (await LoginClient.Login(new LoginDTO()
            {
                ReturnUrl = ReturnUrl,
                UsernameOrEmail = _model.UsernameOrEmail,
                Senha = _model.Password,
                DominioId = dom2.Id,
                EstabelecimentoId = est2.Id
            }))!;
            if (result.Erro != null)
            {
                ResetEverything();
                _model.ErrorMessage = result.Erro;
            }
            else if (result.RedirectUrl != null)
            {
                NavManager.NavigateTo(result.RedirectUrl, forceLoad: true);
            }
        }
        else if (_model.Stage == LoginStage.UsuarioSenha && PodePularDominio(_usuarioLogado))
        {
            _pulouDominio = true;
            _model.Dominios = _usuarioLogado.Dominios.Select(d => new SelectOption(d.Id, d.Nome)).ToArray();
            _model.DominioId = _model.Dominios[0].Id;
            _model.Estabelecimentos = _usuarioLogado.Dominios.First().Estabelecimentos.Select(e => new SelectOption(e.Id, e.Nome)).ToArray();
            _model.EstabelecimentoId = _model.Estabelecimentos[0].Id;
            _model.Stage = LoginStage.Estabelecimento;
            _dominioCorrente = _usuarioLogado.Dominios.First(d => d.Id == _model.DominioId);
            _model.LoginOn = LoginOn.Dominio;
            if (!_dominioCorrente.PodeLogarDominio)
                _model.LoginOn = LoginOn.Estabelecimento;
        }
        else if (_model.Stage == LoginStage.UsuarioSenha)
        {
            _model.Dominios = _usuarioLogado.Dominios.Select(d => new SelectOption(d.Id, d.Nome)).ToArray();
            _model.DominioId = _model.Dominios[0].Id;
            _model.Stage = LoginStage.Dominio;
        }
        else if (_model.Stage == LoginStage.Dominio)
        {
            _model.Estabelecimentos = _usuarioLogado.Dominios.First().Estabelecimentos.Select(e => new SelectOption(e.Id, e.Nome)).ToArray();
            _model.EstabelecimentoId = _model.Estabelecimentos[0].Id;
            _model.Stage = LoginStage.Estabelecimento;
            _dominioCorrente = _usuarioLogado.Dominios.First(d => d.Id == _model.DominioId);
            _model.LoginOn = LoginOn.Dominio;
            if (!_dominioCorrente.PodeLogarDominio)
                _model.LoginOn = LoginOn.Estabelecimento;
        }
    }

    private bool PodePularDominio(UsuarioLogadoDTO usuarioLogado)
    {
        return _model.Stage == LoginStage.UsuarioSenha && usuarioLogado.Dominios.Count == 1 && usuarioLogado.Dominios.First().PodeLogarDominio && usuarioLogado.Dominios.First().Estabelecimentos.Any();
    }

    private (UsuarioLogadoDTO.DominioDTO? Dominio, UsuarioLogadoDTO.EstabelecimentoDTO? Estabelecimento) PodeLogarEstabelecimento(UsuarioLogadoDTO usuarioLogado)
    {
        if (_model.DominioId != null && _model.EstabelecimentoId != null && _model.Stage == LoginStage.Estabelecimento && _model.LoginOn == LoginOn.Estabelecimento)
        {
            var currentDominio = usuarioLogado.Dominios.First(d => d.Id == _model.DominioId);
            var currentEstabelecimento = currentDominio.Estabelecimentos.First(e => e.Id == _model.EstabelecimentoId);
            return (currentDominio, currentEstabelecimento);
        }
        else if (_model.DominioId != null && _model.Stage == LoginStage.Dominio)
        {
            var currentDominio = usuarioLogado.Dominios.First(d => d.Id == _model.DominioId);
            if (!currentDominio.PodeLogarDominio && currentDominio.Estabelecimentos.Count == 1)
                return (currentDominio, currentDominio.Estabelecimentos.First());
            else
                return (null, null);
        }
        else if (_model.Stage == LoginStage.UsuarioSenha)
        {
            if (usuarioLogado.Dominios.Count == 1 && !usuarioLogado.Dominios.First().PodeLogarDominio && usuarioLogado.Dominios.First().Estabelecimentos.Count == 1)
                return (usuarioLogado.Dominios.First(), usuarioLogado.Dominios.First().Estabelecimentos.First());
            else
                return (null, null);
        }
        else
            return (null, null);
    }

    private UsuarioLogadoDTO.DominioDTO? PodeLogarDominio(UsuarioLogadoDTO usuarioLogado)
    {
        if (_model.DominioId != null && _model.Stage == LoginStage.Dominio)
        {
            var currentDominio = usuarioLogado.Dominios.First(d => d.Id == _model.DominioId);
            if (currentDominio.PodeLogarDominio && currentDominio.Estabelecimentos.Count == 0)
                return currentDominio;
            else
                return null;
        }
        else if (_model.DominioId != null && _model.Stage == LoginStage.Estabelecimento && _model.LoginOn == LoginOn.Dominio)
        {
            var currentDominio = usuarioLogado.Dominios.First(d => d.Id == _model.DominioId);
            if (currentDominio.PodeLogarDominio)
                return currentDominio;
            else
                return null;
        }
        else if (_model.Stage == LoginStage.UsuarioSenha)
        {
            if (usuarioLogado.Dominios.Count == 1 && usuarioLogado.Dominios.First().PodeLogarDominio && usuarioLogado.Dominios.First().Estabelecimentos.Count == 0)
                return usuarioLogado.Dominios.First();
            else
                return null;
        }
        else
            return null;
    }

    async Task ContinueFromDominio()
    {
        _loading = true;
        try
        {
            await Next();
        }
        catch (BackendException e)
        {
            ResetEverything();
            _model.ErrorMessage = e.Message;
        }
        finally
        {
            _loading = false;
        }
    }

    async Task ContinueFromEstabelecimento()
    {
        _loading = true;
        try
        {
            await Next();
        }
        catch (BackendException e)
        {
            ResetEverything();
            _model.ErrorMessage = e.Message;
        }
        finally
        {
            _loading = false;
        }
    }

    void Previous()
    {
        switch (_model.Stage)
        {
            case LoginStage.Dominio:
                ResetEverything();
                _model.Stage = LoginStage.UsuarioSenha;
                break;
            case LoginStage.Estabelecimento:
                if (!_pulouDominio)
                {
                    _model.ErrorMessage = null;
                    _model.Estabelecimentos = new SelectOption[0];
                    _model.EstabelecimentoId = null;
                    _dominioCorrente = null;
                    _model.LoginOn = LoginOn.Dominio;
                    _model.Stage = LoginStage.Dominio;
                }
                else
                {
                    ResetEverything();
                    _model.Stage = LoginStage.UsuarioSenha;
                }
                break;
            default:
                break;
        }
    }

    private void ResetEverything()
    {
        _model.ErrorMessage = null;
        _model.Dominios = new SelectOption[0];
        _model.Estabelecimentos = new SelectOption[0];
        _model.DominioId = null;
        _model.EstabelecimentoId = null;
        _model.UsernameOrEmail = null;
        _model.Password = null;
        _model.LoginOn = LoginOn.Dominio;
        _usuarioLogado = null;
        _pulouDominio = false;
        _dominioCorrente = null;
        _model.Stage = LoginStage.UsuarioSenha;
    }
}
