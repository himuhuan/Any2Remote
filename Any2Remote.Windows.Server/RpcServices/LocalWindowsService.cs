using Any2Remote.Windows.Grpc.Services;
using Any2Remote.Windows.Shared.Exceptions;
using Any2Remote.Windows.Shared.Helpers;
using Grpc.Core;
using Microsoft.Win32;
using System.Text;

namespace Any2Remote.Windows.Server.RpcServices
{
    /// <summary>
    /// 为 Any2Remote.Windows 的其它部分提供封装本地 Windows 服务的 gRPC 服务。
    /// 这些服务通常需要管理员权限.
    /// </summary>
    public class LocalWindowsService : Local.LocalBase
    {
        private readonly ILogger<LocalWindowsService> _logger;

        public LocalWindowsService(ILogger<LocalWindowsService> logger)
        {
            _logger = logger;
        }

        private static readonly string[] InstallAppRegisterKeys = {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
        };

        /// <summary>
        /// 获取本地安装的应用程序列表, 我们使用以下策略：
        /// <list type="number">
        /// <item> 如果注册表中没有 DisplayName 和 UninstallString，我们将跳过此条目 </item>
        /// <item> 如果注册表中定义了 SystemComponent，且值为 1，则跳过此条目（request 中可以设置 IncludeSystemComponent 来包含这些条目）</item>
        /// <item> 如果 1，2 都通过，IconUrl 一定不为空，如果 DisplayIcon 不为空，我们直接使用 DisplayIcon 否则我们返回 UninstallString 的第一个参数。</item>
        /// <item> 其它项如果没有值，就一律用空字符串或者默认值表示 </item>
        /// </list>
        /// </summary>
        public override Task<LocalAppsResponse> GetLocalApps(LocalAppsRequest request, ServerCallContext context)
        {
            LocalAppsResponse response = new();
            foreach (string registerKey in InstallAppRegisterKeys)
            {
                response.Apps.AddRange(GetLocalAppsFrom(registerKey, request));
            }
            return Task.FromResult(response);
        }

        public override Task<AssociatedStartMenuLnkResponse> 
            GetAssociatedStartMenuLnk(LocalApp request, ServerCallContext context)
        {
            AssociatedStartMenuLnkResponse response = new();
            string startMenuPathUser = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            string startMenuPathCommon = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            var startMenuPaths = new List<string> { startMenuPathUser, startMenuPathCommon };
            string pattern = $"*{request.DisplayName}*.lnk";
            foreach (var path in startMenuPaths.Where(Directory.Exists))
            {
                DirectoryInfo di = new(path);
                foreach (var file in di.EnumerateFiles(pattern, new EnumerationOptions
                         {
                             IgnoreInaccessible = true,
                             MatchCasing = MatchCasing.CaseInsensitive,
                             RecurseSubdirectories = true
                         }))
                {
                    response.LnkFiles.Add(file.FullName);
                }
            }
            return Task.FromResult(response);
        }

        private List<LocalApp> GetLocalAppsFrom(string registerKey, LocalAppsRequest request)
        {
            List<LocalApp> apps = new();
            using RegistryKey key = Registry.LocalMachine.OpenSubKey(registerKey)
                ?? throw new Any2RemoteException("Cannot open registry key: " + registerKey);
            foreach (string subkeyName in key.GetSubKeyNames())
            {
                using RegistryKey subkey = key.OpenSubKey(subkeyName)!;

                // If the DisplayName or UninstallString is null, skip this entry
                if (subkey.GetValue("DisplayName") is not string displayName
                    || subkey.GetValue("UninstallString") is not string uninstallString)
                {
                    continue;
                }
                string iconUrl = subkey.GetValue("DisplayIcon") as string ?? GetIconUrlFromUnstallString(uninstallString);
                bool isSystemComponent = subkey.GetValue("SystemComponent") as int? == 1;
                // If the SystemComponent is 1, skip this entry
                if (!request.IncludeSystemComponent && isSystemComponent)
                    continue;
                _logger.LogDebug("Found app: {appId} ({appName}) with {icon}. (on {registerKey})", 
                    subkeyName, displayName, iconUrl, registerKey);
                apps.Add(new LocalApp
                {
                    Id = subkeyName,
                    DisplayName = displayName,
                    SystemComponent = isSystemComponent,
                    UninstallString = uninstallString,
                    IconUrl = iconUrl,
                });
            }
            return apps;
        }

        /// <summary>
        /// 从 UninstallString 中获取卸载程序与参数, 目前支持以下格式：
        /// 1. "C:\Program Files\Any2Remote\uninstall.exe" /S
        /// 2. C:\Program Files\Any2Remote\uninstall.exe
        /// 3. C:\Program Files\Any2Remote\uninstall.exe /S
        /// 卸载程序路径中可以有空格.
        /// </summary>
        public static string GetIconUrlFromUnstallString(string uninstallString)
        {
            return WindowsCommon.ParseCommandLine(uninstallString).Program;
        }
    }
}
