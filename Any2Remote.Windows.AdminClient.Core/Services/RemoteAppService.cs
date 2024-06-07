using Any2Remote.Windows.AdminClient.Core.Contracts.Services;
using Any2Remote.Windows.AdminClient.Core.Exceptions;
using Any2Remote.Windows.Shared.Models;
using Any2Remote.Windows.Grpc.Services;
using Microsoft.Win32;

namespace Any2Remote.Windows.AdminClient.Core.Services
{
    public class RemoteAppService : IRemoteAppService
    {
        const string RemoteAppKeyPath = 
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\TSAppAllowList\Applications";

        public Dictionary<string, RemoteApplication> GetRemoteAppMap(bool overrideUrl)
        {
            var appsKey = Registry.LocalMachine.OpenSubKey(RemoteAppKeyPath, false)
                ?? Registry.LocalMachine.CreateSubKey(RemoteAppKeyPath, false);
            string[] appIds = appsKey.GetSubKeyNames();

            Dictionary<string, RemoteApplication> remoteApps = new();
            foreach (string appId in appIds)
            {
                RegistryKey appKey = appsKey.OpenSubKey(appId)!;
                string name = appKey.GetValue("Name") as string
                    ?? throw new ServerStatusException(ServerStatus.InternalError, $"no such value {RemoteAppKeyPath}\\Name");
                string path = appKey.GetValue("Path") as string
                    ?? throw new ServerStatusException(ServerStatus.InternalError, $"no such value {RemoteAppKeyPath}\\Path");
                string workingDir = appKey.GetValue("WorkingDirectory") as string
                    ?? string.Empty;
                string commandLine = appKey.GetValue("CommandLine") as string
                    ?? string.Empty;
                string description = appKey.GetValue("Description") as string
                    ?? string.Empty;
                string appIconUrl = appKey.GetValue("AppIconUrl") as string
                    ?? string.Empty;
                RemoteApplication application = new()
                {
                    AppId = appId,
                    DisplayName = name,
                    Path = path,
                    WorkingDirectory = workingDir,
                    CommandLine = commandLine,
                    Description = description,
                    // Only server can know the actual icon url in file system
                    // Client should get the icon from server via api
                    AppIconUrl = (overrideUrl) ? $"api/remoteapps/{appId}/icon" : appIconUrl
                };

                if (appKey.GetValue("UninstallString") is string uninstallString)
                {
                    int systemComponent = (int) (appKey.GetValue("SystemComponent") ?? 0);
                    application.LocalInfo = new LocalApp
                    {
                        DisplayName = name,
                        IconUrl = appIconUrl,
                        Id = appId,
                        UninstallString = uninstallString,
                        SystemComponent = systemComponent == 1
                    };
                }

                remoteApps.Add(appId, application);
            }
            return remoteApps;
        }

        public void RemoveRemoteApp(string appId)
        {
            var appsKey = Registry.LocalMachine.OpenSubKey(RemoteAppKeyPath, true)
                ?? throw new ServerStatusException(ServerStatus.InternalError, $"no such key {RemoteAppKeyPath}");
            appsKey.DeleteSubKeyTree(appId);
            appsKey.Close();
        }

        public void PublishRemoteApp(RemoteApplication application)
        {
            var appsKey = Registry.LocalMachine.OpenSubKey(
                RemoteAppKeyPath, true)
                ?? Registry.LocalMachine.CreateSubKey(RemoteAppKeyPath, true);
            try
            {
                RegistryKey appRegKey = appsKey.CreateSubKey(application.AppId);
                appRegKey.SetValue("Name", application.DisplayName);
                appRegKey.SetValue("Path", application.Path);
                appRegKey.SetValue("WorkingDirectory", application.WorkingDirectory);
                appRegKey.SetValue("CommandLine", application.CommandLine);
                appRegKey.SetValue("Description", application.Description);
                appRegKey.SetValue("AppIconUrl", application.AppIconUrl);
                if (application.LocalInfo != null)
                {
                    appRegKey.SetValue("UninstallString", application.LocalInfo.UninstallString);
                    appRegKey.SetValue("SystemComponent", application.LocalInfo.SystemComponent);
                }
                appRegKey.Close();
            }
            catch (Exception e)
            {
                throw new ServerStatusException(ServerStatus.InternalError, e.Message);
            }
        }

        public List<RemoteApplication> GetRemoteApplications()
        {
            Dictionary<string, RemoteApplication> localApps = GetRemoteAppMap(true);
            return localApps.Select(app => app.Value).ToList();
        }
    }
}