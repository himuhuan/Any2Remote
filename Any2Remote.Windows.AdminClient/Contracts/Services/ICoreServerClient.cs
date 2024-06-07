using Any2Remote.Windows.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Any2Remote.Windows.AdminClient.Contracts.Services
{
    public interface ICoreServerClient
    {
        Task<List<RemoteApplication>> GetApplicationsAsync();

        Task RemoveRemoteApplicationAsync(RemoteApplication application);

        Task PublishRemoteApplicationAsync(RemoteApplication application);
    }
}
