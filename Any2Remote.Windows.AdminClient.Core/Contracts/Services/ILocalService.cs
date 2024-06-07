using Any2Remote.Windows.Grpc.Services;
using Any2Remote.Windows.Shared.Models;

namespace Any2Remote.Windows.AdminClient.Core.Contracts.Services;

public interface ILocalService
{
    List<string> GetStartMenuLnkNames(LocalApp app);

    Task<List<string>> GetStartMenuLnkNamesAsync(LocalApp app);

    List<LocalApp> GetInstalledApplications(LocalAppsRequest request);

    ServerStatus GetServerStatus();

    void StartServer();

    void StartDevServer();

    void StopServer();

    void InitServer();
}
