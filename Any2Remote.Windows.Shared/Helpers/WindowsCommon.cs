using IWshRuntimeLibrary;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32;
using Windows.Win32.Foundation;
using Any2Remote.Windows.Shared.Models;
using Microsoft.Win32;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Drawing;
using Any2Remote.Windows.Grpc.Services;
using Any2Remote.Windows.Shared.Exceptions;

namespace Any2Remote.Windows.Shared.Helpers;

/// <summary>
/// Shared utilities and services for Windows.
/// </summary>
public static class WindowsCommon
{
#pragma warning disable CA1416 // validate platform compatibility

    /// <summary>
    /// Path to store app files from Any2Remote. 
    /// </summary>
    public static readonly string Any2RemoteAppDataFolder
        = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Any2Remote");

    private static readonly string[] InstallAppRegisterKeys = new[]
    {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
    };

    /// <summary>
    /// Read application information from a shortcut file.
    /// </summary>
    /// <param name="pathToLnk"> a path to ".lnk" file </param>
    public static ExecutableApplication? GetApplicationInfoFromInk(string pathToLnk)
    {
        WshShell shell = new();
        try
        {
            var shortcut = (IWshShortcut) shell.CreateShortcut(pathToLnk);
            var programDisplayName = Path.GetFileNameWithoutExtension(shortcut.FullName);
            return new ExecutableApplication
            {
                DisplayName = programDisplayName,
                Path = shortcut.TargetPath,
                CommandLine = shortcut.Arguments,
                WorkingDirectory = shortcut.WorkingDirectory,
                Description = shortcut.Description
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static string GetDesktopWallpaper()
    {
        UnsafeStackBuffer512 buffer = new();
        unsafe
        {
            if (PInvoke.SystemParametersInfo(SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETDESKWALLPAPER, 512, buffer.Buffer, 0))
            {
                string wallpaperPath = new(buffer.Buffer);
                return wallpaperPath;
            }
            else
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Get rdp services status from registry.
    /// </summary>
    /// <remarks> This method requires elevated permissions. </remarks>
    public static ServerStatus GetRdpServerStatus()
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


    public static ParseCommandLineResult ParseCommandLine(string commandLine)
    {
        if (System.IO.File.Exists(commandLine))
        {
            return new ParseCommandLineResult
            {
                Program = commandLine,
                ArgumentList = Array.Empty<string>()
            };
        }
        unsafe
        {
            var argv = CommandLineToArgvW(commandLine, out int pNumArgs);
            try
            {
                if (pNumArgs == 0) throw new Win32Exception("CommandLineToArgv ERROR");
                string[] args = new string[pNumArgs];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p)!;
                }
                return new ParseCommandLineResult
                {
                    Program = args[0],
                    ArgumentList = args.Skip(1).ToArray()
                };
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }
    }

    [DllImport("shell32.dll", SetLastError = true)]
    static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

    public static ParseIconUrlResult ParseIconUrl(string iconUrl)
    {
        string[] args = iconUrl.Split(',');
        return args.Length switch
        {
            0 => new ParseIconUrlResult { IconFilePath = string.Empty, IconIndex = null },
            1 => new ParseIconUrlResult { IconFilePath = args[0], IconIndex = null },
            _ => new ParseIconUrlResult { IconFilePath = args[0], IconIndex = int.Parse(args[1]) }
        };
    }

    public static Icon? ExtractIcon(string iconFilePath, int? iconIndex)
    {
        try
        {
            return ExtractIconNative(iconFilePath, iconIndex ?? 0, 48, false);
        }
        catch (Exception)
        {
            if (iconFilePath[0] == '\"') iconFilePath = iconFilePath[1..^1];
            if (System.IO.File.Exists(iconFilePath) && Path.GetExtension(iconFilePath).ToLower() == ".ico")
            {
                return new Icon(iconFilePath);
            }
            return null;
        }
    }

    /// <summary>
    /// 从指定的文件路径中提取图标。
    /// </summary>
    /// <param name="filePath">文件的路径。</param>
    /// <param name="id">要提取的图标的资源标识符。</param>
    /// <param name="size">图标的大小（以像素为单位）。</param>
    /// <param name="smallIcon">是否提取小图标。默认为false，提取大图标。</param>
    /// <returns>如果成功提取图标，则返回 <see cref="Icon"/> 对象；如果提取失败，则返回 null。</returns>
    /// <exception cref="ArgumentNullException">如果 filePath 为 null，则抛出此异常。</exception>
    /// <exception cref="ArgumentOutOfRangeException">如果 size 不在有效范围内，则抛出此异常。</exception>
    /// <exception cref="IOException">如果无法提取图标，则抛出此异常。</exception>
    /// <remarks>
    /// 此方法使用 PInvoke 调用 SHDefExtractIcon 函数来提取图标。
    /// 如果 result 为 HRESULT.S_FALSE，则表示没有图标可以提取。
    /// </remarks>
    private static unsafe Icon? ExtractIconNative(string filePath, int id, int size, bool smallIcon = false)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        Debug.Assert(size is >= 0 and <= ushort.MaxValue);

        HICON hicon = HICON.Null;
        HRESULT result;
        fixed (char* c = filePath)
        {
            result = PInvoke.SHDefExtractIcon(
                c,
                id,
                0,
                smallIcon ? null : &hicon,
                smallIcon ? &hicon : null,
                (uint) (ushort) size << 16 | (ushort) size);
        }

        if (result == HRESULT.S_FALSE)
        {
            return null;
        }

        try
        {
            Marshal.ThrowExceptionForHR((int) result);
        }
        catch (COMException ex)
        {
            // This API is only documented to return E_FAIL, which surfaces as COMException. Wrap in a "nicer"
            // ArgumentException.
            throw new IOException("Icon Cannot be extracted", ex);
        }

        return Icon.FromHandle(hicon);
    }

    /// <summary>
    /// 获取本地安装的应用程序列表, 我们使用以下策略：
    /// <list type="number">
    /// <item> 如果注册表中没有 DisplayName 和 UninstallString，我们将跳过此条目 </item>
    /// <item> 如果注册表中定义了 SystemComponent，且值为 1，则跳过此条目（可以设置 IncludeSystemComponent 来包含这些条目）</item>
    /// <item> 如果 1，2 都通过，IconUrl 一定不为空，如果 DisplayIcon 不为空，我们直接使用 DisplayIcon 否则我们返回 UninstallString 的第一个参数。</item>
    /// <item> 其它项如果没有值，就一律用空字符串或者默认值表示 </item>
    /// </list>
    /// </summary>
    public static List<LocalApp> GetLocalApps(bool includeSystemComponent)
    {
        List<LocalApp> localApps = new();
        foreach (string registerKey in InstallAppRegisterKeys)
        {
            localApps.AddRange(GetLocalAppsFromRegisterKey(registerKey, includeSystemComponent));
        }
        return localApps;
    }

    private static List<LocalApp> GetLocalAppsFromRegisterKey(string registerKey, bool includeSystemComponent)
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
            if (!includeSystemComponent && isSystemComponent)
                continue;
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
        return ParseCommandLine(uninstallString).Program;
    }

    /// <summary>
    /// 尝试根据可执行文件信息找出 Windows 中已安装程序对应项。
    /// </summary>
    public static List<LocalApp> GetAssociatedLocalApp(ExecutableApplication executable)
    {
        List<LocalApp> allApps = GetLocalApps(includeSystemComponent: false);
        List<LocalApp> possibleResult = new();
        foreach (LocalApp app in allApps)
        {
            string longerName = app.DisplayName, shorterName = executable.DisplayName;
            if (longerName.Length < shorterName.Length)
                (longerName, shorterName) = (shorterName, longerName);
            if (longerName.Contains(shorterName))
            {
                // certain result
                if (longerName.Length == shorterName.Length)
                    return new List<LocalApp> { app };
                possibleResult.Add(app);
            }
        }
        return possibleResult;
    }

}

public class ParseIconUrlResult
{
    public string IconFilePath { get; init; } = default!;
    public int? IconIndex { get; init; } = null;
}

public class ParseCommandLineResult
{
    public string Program { get; set; } = default!;
    public string[] ArgumentList { get; set; } = default!;
}

internal unsafe struct UnsafeStackBuffer512
{
    public fixed char Buffer[512];
}

#pragma warning restore CA1416 // validate platform compatibility


