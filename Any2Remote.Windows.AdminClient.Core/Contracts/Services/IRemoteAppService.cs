using Any2Remote.Windows.Shared.Models;

namespace Any2Remote.Windows.AdminClient.Core.Contracts.Services
{
    public interface IRemoteAppService
    {
        void PublishRemoteApp(RemoteApplication application);

        void RemoveRemoteApp(string appId);

        Dictionary<string, RemoteApplication> GetRemoteAppMap(bool overrideUrl);

        List<RemoteApplication> GetRemoteApplications();
    }
}
