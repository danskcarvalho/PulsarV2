using Microsoft.FluentUI.AspNetCore.Components;
using Pulsar.Web.Client.Exceptions;
using Pulsar.Web.Client.Models.Shared;
using System.Runtime.CompilerServices;

namespace Pulsar.Web.Client.Services;

public class ProtectedSectionService(IToastService toastService, IMessageService messageService)
{
    public async Task Protect(Func<Task> action, LoadingModel? loading = null)
    {
        if (loading != null)
        {
            loading.IsLoading = true;
        }
        try
        {
            await action();
        }
        catch (BackendException e)
        {
            toastService.ShowError(e.Message);
        }
        catch
        {
            toastService.ShowError("Erro desconhecido. Favor tentar novamente em alguns segundos.");
        }
        finally
        {
            if (loading != null)
            {
                loading.IsLoading = false;
            }
        }
    }

    public async Task ProtectDialog(Func<Task> action, LoadingModel? loading = null, string section = "MESSAGES_DIALOG")
    {
        if (loading != null)
        {
            loading.IsLoading = true;
        }
        try
        {

            await action();
        }
        catch (BackendException e)
        {

            messageService.ShowMessageBar(e.Message, MessageIntent.Error, section);
        }
        catch
        {
            messageService.ShowMessageBar("Erro desconhecido. Favor tentar novamente em alguns segundos.", MessageIntent.Error, section);
        }
        finally
        {
            if (loading != null)
            {
                loading.IsLoading = false;
            }
        }
    }
}
