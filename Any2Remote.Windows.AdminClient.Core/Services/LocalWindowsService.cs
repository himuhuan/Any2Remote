using Any2Remote.Windows.AdminClient.Core.Contracts.Services;
using Any2Remote.Windows.AdminClient.Core.Exceptions;
using Any2Remote.Windows.Grpc.Services;
using Microsoft.Win32;
using System.Diagnostics;
using Any2Remote.Windows.Shared.Models;

namespace Any2Remote.Windows.AdminClient.Core.Services;

public class LocalWindowsService : ILocalService
{
    private readonly Local.LocalClient _rpcClient;

    public LocalWindowsService(Local.LocalClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    public List<string> GetStartMenuLnkNames(LocalApp app)
    {
        return GetStartMenuLnkNamesAsync(app).GetAwaiter().GetResult();
    }

    public async Task<List<string>> GetStartMenuLnkNamesAsync(LocalApp app)
    {
        var resp = await _rpcClient.GetAssociatedStartMenuLnkAsync(app);
        return resp.LnkFiles.Select(s => s.ToString()).ToList();
    }

    public List<LocalApp> GetInstalledApplications(LocalAppsRequest request)
    {
        var response = _rpcClient.GetLocalAppsAsync(request).GetAwaiter().GetResult();
        return response.Apps.ToList();
    }

    public ServerStatus GetServerStatus()
    {
        var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server", false);
        var remoteAppKey = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\TSAppAllowList", false);

        if (key == null || remoteAppKey == null)
        {
            return ServerStatus.NotSupported;
        }
        var denyConnection = key.GetValue("fDenyTSConnections") as int?;
        var allowRemoteApps = remoteAppKey.GetValue("fDisabledAllowList") as int?;
        if (!denyConnection.HasValue || denyConnection.Value == 1
            || !allowRemoteApps.HasValue || allowRemoteApps.Value == 0)
        {
            return ServerStatus.NotInitialized;
        }

        Process[] processes = Process.GetProcessesByName("Any2Remote.Windows.Server");
        if (processes.Length == 0)
        {
            return ServerStatus.Disconnected;
        }

        return ServerStatus.Connected;
    }

    public void InitServer()
    {
        LanuchAdminRunner("server", "init");
    }

    public void StartDevServer()
    {
        string serverRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Assets\Any2RemoteServer");
        string serverRoot = $"\"{serverRootPath}\"";
        LanuchAdminRunner("server", "start-dev", serverRoot);
    }

    public void StartServer()
    {
        string serverRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Assets\Any2RemoteServer");
        // Ensure the server root path is enclosed in double quotes
        // Command Line in Windows does not recognize paths with spaces without quotes
        // If user installed the app in a path with spaces, the execution will fail.
        string serverRoot = $"\"{serverRootPath}\"";
        LanuchAdminRunner("server", "start", serverRoot);
    }

    public void StopServer()
    {
        LanuchAdminRunner("server", "stop");
    }

    private static void LanuchAdminRunner(params string[] args)
    {
        string runnerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            @"Assets\AdminRunner\Any2Remote.Windows.AdminRunner.exe");
        ProcessStartInfo startInfo = new()
        {
            FileName = runnerPath,
            UseShellExecute = true,
            CreateNoWindow = false,
            Arguments = string.Join(' ', args),
            Verb = "runas"
        };
        try
        {
            var process = Process.Start(startInfo);
            process?.WaitForExit();
        }
        catch (Exception ex)
        {
            throw new Any2RemoteException("Unable to lanuch AdminRunner", ex);
        }
    }
}
