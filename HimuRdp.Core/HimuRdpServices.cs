using HimuRdp.Core.Exceptions;
using Microsoft.Win32;
using System.ServiceProcess;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using Windows.Win32;
using static System.Environment;
using TimeoutException = System.TimeoutException;
using Windows.Win32.Security;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Windows.Win32.Foundation;
using Windows.Win32.System.RemoteDesktop;

namespace HimuRdp.Core;

/// <summary>
/// HimuRdpServices is a class that provides a set of methods to manage the RDP Wrapper.
/// </summary>
public class HimuRdpServices
{
    public static bool CheckEnvironment()
    {
        return IsWindows7OrHigher() && IsSupportedArchitecture();
    }

    public static CheckResult CheckInstallation()
    {
        RegistryKey termServiceReg = RegisterKeyWrapper
            .OpenRegisterKey(@"SYSTEM\CurrentControlSet\Services\TermService", false).Key;
        string termServiceHost = termServiceReg.GetValue("ImagePath") as string
                                 ?? throw new InvalidRegisterException(RegistryHive.LocalMachine,
                                     @"SYSTEM\CurrentControlSet\Services\TermService\ImagePath");
        if (!termServiceHost.Contains("svchost.exe") && !termServiceHost.Contains("svchost -k"))
        {
            return new CheckResult(HimuRdpError.NotSupported,
                "TermService is hosted in a custom application (BeTwin, etc.) - unsupported.");
        }

        string serviceDllPath = termServiceReg.OpenSubKey("Parameters", false)?.GetValue("ServiceDll") as string
                                ?? throw new InvalidRegisterException(RegistryHive.LocalMachine,
                                    @"SYSTEM\CurrentControlSet\Services\TermService\Parameters\ServiceDll");
        bool isRdpWrapInstalled = serviceDllPath.Contains("rdpwrap.dll", StringComparison.InvariantCultureIgnoreCase);
        if (!serviceDllPath.Contains("termsrv.dll", StringComparison.InvariantCultureIgnoreCase) && !isRdpWrapInstalled)
        {
            return new CheckResult
            {
                Message = "Another third-party TermService library is installed - unsupported.",
                ErrorCode = HimuRdpError.NotSupported
            };
        }

        return new CheckResult(isRdpWrapInstalled ? HimuRdpError.Success : HimuRdpError.NotInstalled);
    }

    public static FileVersionInfo GetTermsrvVersion()
    {
        string serviceDllPath = LocalMachineRegister
                                    .OpenSubKey(@"SYSTEM\CurrentControlSet\Services\TermService\Parameters", false)
                                    ?.GetValue("ServiceDll") as string
                                ?? throw new InvalidRegisterException(RegistryHive.LocalMachine,
                                    @"SYSTEM\CurrentControlSet\Services\TermService\Parameters\ServiceDll");

        return FileVersionInfo.GetVersionInfo(serviceDllPath);
    }

    public static CheckResult CheckTermsrvVersion()
    {
        CheckResult result = new();
        FileVersionInfo termsrvVersion = GetTermsrvVersion();
        switch (termsrvVersion.ProductMajorPart)
        {
            // Windows XP
            case 5 when termsrvVersion.ProductMinorPart == 1:
                result.ErrorCode = HimuRdpError.NotSupported;
                result.Message = "Windows XP is not supported.";
                break;
            // Windows Server 2003 or XP 64-bit Edition
            case 5 when termsrvVersion.ProductMinorPart == 2:
                result.ErrorCode = HimuRdpError.NotSupported;
                result.Message = "Windows Server 2003 or XP 64-bit Edition is not supported.";
                break;
            // Windows Vista
            case 6 when termsrvVersion.ProductMinorPart == 0:
                result.ErrorCode = HimuRdpError.NotFullySupported;
                result.Message = "Windows Vista is not fully supported.";
                break;
            // Windows 7
            case 6 when termsrvVersion.ProductMinorPart == 1:
                result.ErrorCode = HimuRdpError.NotFullySupported;
                result.Message = "Windows 7 is not fully supported.";
                break;
            // Windows 8 or higher 
            default:
                result.ErrorCode = HimuRdpError.Success;
                break;
        }

        return result;
    }

    public static ServiceController? GetTermsrvServiceController()
    {
        return ServiceController.GetServices()
            .FirstOrDefault(service => service.ServiceName == "TermService");
    }

    public void OutputFiles()
    {
        if (!Directory.Exists(_installPathExpanded))
        {
            Directory.CreateDirectory(_installPathExpanded);
            // set full control for current user and Service group
            var dirInfo = new DirectoryInfo(_installPathExpanded);
            var dirSecurity = dirInfo.GetAccessControl();
            dirSecurity.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier("S-1-5-18"),
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));
            dirSecurity.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier("S-1-5-6"),
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));
            dirInfo.SetAccessControl(dirSecurity);
        }

        ResourceHelper.ExtractResource(HimuRdpResourceKey.RdpWrapperConfigIni,
            Path.Combine(_installPathExpanded, "rdpwrap.ini"));
        HimuRdpResourceKey clipRes = HimuRdpResourceKey.None, rfxRes = HimuRdpResourceKey.None;
        FileVersionInfo tsVersion = GetTermsrvVersion();
        if (RuntimeInformation.OSArchitecture == Architecture.X86)
        {
            ResourceHelper.ExtractResource(HimuRdpResourceKey.RdpWrap32Dll,
                Path.Combine(_installPathExpanded, "rdpwrap.dll"));
            switch (tsVersion.ProductMajorPart)
            {
                case 6 when tsVersion.ProductMinorPart == 0:
                    clipRes = HimuRdpResourceKey.RdpClip6032Exe;
                    break;
                case 6 when tsVersion.ProductMinorPart == 1:
                    clipRes = HimuRdpResourceKey.RdpClip6132Exe;
                    break;
                default:
                    rfxRes = HimuRdpResourceKey.RfxVmt32Dll;
                    break;
            }
        }
        else
        {
            ResourceHelper.ExtractResource(HimuRdpResourceKey.RdpWrap64Dll,
                Path.Combine(_installPathExpanded, "rdpwrap.dll"));
            switch (tsVersion.ProductMajorPart)
            {
                case 6 when tsVersion.ProductMinorPart == 0:
                    clipRes = HimuRdpResourceKey.RdpClip6064Exe;
                    break;
                case 6 when tsVersion.ProductMinorPart == 1:
                    clipRes = HimuRdpResourceKey.RdpClip6164Exe;
                    break;
                default:
                    rfxRes = HimuRdpResourceKey.RfxVmt64Dll;
                    break;
            }
        }

        string system32Path = GetFolderPath(SpecialFolder.System);
        if (clipRes != HimuRdpResourceKey.None)
            ResourceHelper.ExtractResource(clipRes, Path.Combine(system32Path, "rdpclip.exe"));
        if (rfxRes != HimuRdpResourceKey.None)
            ResourceHelper.ExtractResource(rfxRes, Path.Combine(system32Path, "rfxvmt.dll"));
    }

    public void RemoveFiles()
    {
        // For security reasons, we have to check the installation before removing files.
        CheckResult checkResult = CheckInstallation();
        if (checkResult.ErrorCode != HimuRdpError.Success)
            throw new InvalidOperationException("Try to remove files without installation.");
        string[] filesToRemove = { "rdpwrap.dll", "rdpwrap.ini" };
        foreach (string file in filesToRemove)
        {
            string filePath = Path.Combine(_installPathExpanded, file);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        // Some user may install in system32 folder... it's not recommended.
        if (_installPathExpanded != GetFolderPath(SpecialFolder.System))
            Directory.Delete(_installPathExpanded, true);
    }

    public void RegisterWrapperDll(bool enable)
    {
        RegistryKey termServiceReg =
            LocalMachineRegister.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\TermService\Parameters", true)
            ?? throw new InvalidRegisterException(RegistryHive.LocalMachine,
                @"SYSTEM\CurrentControlSet\Services\TermService\Parameters");
        termServiceReg.SetValue("ServiceDll",
            enable ? Path.Combine(_installPath, "rdpwrap.dll") : @"%SystemRoot%\System32\termsrv.dll",
            RegistryValueKind.ExpandString);
        termServiceReg.Close();
    }

    /// <summary>
    /// Configure the termsrv service dependencies.
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    public static void ConfigureTermsrvDependenciesServices()
    {
        var services = ServiceController.GetServices();
        ServiceController? certPropSvc = services.FirstOrDefault(service => service.ServiceName == "CertPropSvc");
        ServiceController? sessionEnv = services.FirstOrDefault(service => service.ServiceName == "SessionEnv");
        if (certPropSvc == null || sessionEnv == null)
            throw new NotSupportedException("CertPropSvc or SessionEnv service not found.");
        certPropSvc.ConfigureStartMode(ServiceStartMode.Manual);
        sessionEnv.ConfigureStartMode(ServiceStartMode.Manual);
    }

    /// <summary>
    /// Configure the termsrv service registry settings.
    /// </summary>
    public static void ConfigureTermsrvRegister(bool active)
    {
        RegisterKeyWrapper.OpenRegisterKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server", true)
            .SetValue("fDenyTSConnections", active ? 0 : 1, RegistryValueKind.DWord)
            .Close();
        if (!active)
            return;
        RegisterKeyWrapper.OpenRegisterKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server\Licensing Core", true)
            .SetValue("EnableConcurrentSessions", 1, RegistryValueKind.DWord)
            .Close();
        RegisterKeyWrapper.OpenRegisterKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true)
            .SetValue("AllowMultipleTSSessions", 1, RegistryValueKind.DWord)
            .Close();


        // Todo: Configure the AddIns registry settings
        // Require TrustedInstaller permission to modify the registry
        // May need Win32 API to get the privilege
        // For now, we just ignore this part. It's not necessary for most users.
#if false
        if (LocalMachineRegister.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server\AddIns", false) == null)
            return;
        var regKey = LocalMachineRegister.OpenSubKey(
            @"SYSTEM\CurrentControlSet\Control\Terminal Server\AddIns\Clip Redirector",
            RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
        regKey?.SetValue("Name", "RDPClip", RegistryValueKind.String);
        regKey?.SetValue("Type", 3, RegistryValueKind.DWord);
        regKey?.Close();
        RegisterKeyWrapper.OpenRegisterKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server\AddIns\DND Redirector",
                true)
            .SetValue("Name", "RDPDND", RegistryValueKind.String)
            .SetValue("Type", 3, RegistryValueKind.DWord)
            .Close();
        RegisterKeyWrapper.OpenRegisterKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server\AddIns\Dynamic VC", true)
            .SetValue("Type", -1, RegistryValueKind.DWord)
            .Close();
#endif
    }

    public static void ConfigureTermsrvFirewall(bool enable)
    {
        if (enable)
        {
            RunCmd("netsh",
                "advfirewall firewall add rule name=\"Remote Desktop\" dir=in protocol=tcp localport=3389 profile=any action=allow"
                , true);
            RunCmd("netsh",
                "advfirewall firewall add rule name=\"Remote Desktop\" dir=in protocol=udp localport=3389 profile=any action=allow"
                , true);
        }
        else
        {
            RunCmd("netsh",
                "advfirewall firewall delete rule name=\"Remote Desktop", true);
        }
    }

    public void DisableWow64Redirection()
    {
        unsafe
        {
            // Only 32-bit process on 64-bit OS needs to disable Wow64 redirection
            if (Is64BitOperatingSystem && !Is64BitProcess)
            {
                PInvoke.Wow64DisableWow64FsRedirection(out _oldWow64RedirectionValue);
            }
        }
    }

    public void RestoreWow64Redirection()
    {
        unsafe
        {
            // Only 32-bit process on 64-bit OS needs to restore Wow64 redirection
            if (Is64BitOperatingSystem && !Is64BitProcess)
            {
                PInvoke.Wow64RevertWow64FsRedirection(_oldWow64RedirectionValue);
            }
        }
    }

    public static void RequestPrivilege(string privilege)
    {
        bool success = PInvoke.OpenProcessToken(
            PInvoke.GetCurrentProcess_SafeHandle(),
            TOKEN_ACCESS_MASK.TOKEN_ADJUST_PRIVILEGES | TOKEN_ACCESS_MASK.TOKEN_QUERY,
            out SafeFileHandle tokenHandle);
        if (!success)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "OpenProcessToken failed.");
        success = PInvoke.LookupPrivilegeValue(null, privilege, out LUID lpLuid);
        if (!success)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "LookupPrivilegeValue failed.");
        TOKEN_PRIVILEGES tp = new()
        {
            PrivilegeCount = 1
        };
        tp.Privileges[0].Luid = lpLuid;
        tp.Privileges[0].Attributes = TOKEN_PRIVILEGES_ATTRIBUTES.SE_PRIVILEGE_ENABLED;
        unsafe
        {
            success = PInvoke.AdjustTokenPrivileges(
                tokenHandle, false, &tp, (uint) sizeof(TOKEN_PRIVILEGES), null,
                null);
        }

        if (!success)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "AdjustTokenPrivileges failed.");
    }

    public void Install()
    {
        RequestPrivilege("SeDebugPrivilege");
        DisableWow64Redirection();
        OutputFiles();
        RegisterWrapperDll(true);
        RestoreWow64Redirection();
    }

    public void Uninstall()
    {
        RequestPrivilege("SeDebugPrivilege");
        DisableWow64Redirection();
        RemoveFiles();
        RegisterWrapperDll(false);
        RestoreWow64Redirection();
    }

    // ReSharper disable once InconsistentNaming
    private static readonly HANDLE WTS_CURRENT_SERVER_HANDLE = HANDLE.Null;

    public static List<TermsrvSession> GetTermsrvSessions()
    {
        unsafe
        {
            WTS_SESSION_INFOW* sessionsInfo = null;
            PWSTR buffer = new PWSTR(null);
            try
            {
                if (!PInvoke.WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, out sessionsInfo,
                    out var count))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "WTSEnumerateSessions failed.");
                }
                List<TermsrvSession> sessions = new();
                for (uint i = 0; i < count; ++i)
                {
                    uint sessionId = sessionsInfo[i].SessionId;
                    // Skip the console and services session, local session
                    if (sessionId is < 2 or >= 65536)
                        continue;
                    TermsrvSession session = new()
                    {
                        WinStationName = sessionsInfo[i].pWinStationName.ToString(),
                        SessionId = sessionsInfo[i].SessionId,
                        Status = (SessionConnectStatus) sessionsInfo[i].State
                    };

                    if (session.Status == SessionConnectStatus.Active)
                    {
                        if (!PInvoke.WTSQuerySessionInformation(WTS_CURRENT_SERVER_HANDLE, sessionId,
                            WTS_INFO_CLASS.WTSSessionInfo, out buffer, out _))
                            throw new Win32Exception(Marshal.GetLastWin32Error(), "WTSQuerySessionInformation failed.");
                        WTSINFOW *info = (WTSINFOW*) buffer.Value;
                        session.UserName = info->UserName.ToString();
                        session.Domain = info->Domain.ToString();
                        session.ConnectTime = DateTime.FromFileTime(info->ConnectTime);
                        PInvoke.WTSFreeMemory(buffer.Value);
                        if (!PInvoke.WTSQuerySessionInformation(WTS_CURRENT_SERVER_HANDLE, sessionId,
                                WTS_INFO_CLASS.WTSClientAddress, out buffer, out _))
                            throw new Win32Exception(Marshal.GetLastWin32Error(), "WTSQuerySessionInformation failed.");
                        WTS_CLIENT_ADDRESS *address = (WTS_CLIENT_ADDRESS *) buffer.Value;
                        session.Address = ConvertRawBytesToIpAddress(address);
                    }
                    sessions.Add(session);
                }
                return sessions.ToList();
            }
            finally
            {
                PInvoke.WTSFreeMemory(sessionsInfo);
                PInvoke.WTSFreeMemory(buffer.Value);
            }
        }
    }

    public static bool LogoffSession(long sessionId, bool wait)
    {
        return PInvoke.WTSLogoffSession(WTS_CURRENT_SERVER_HANDLE, (uint) sessionId, wait);
    }

    public static bool DisconnectSession(long sessionId, bool wait)
    {
        return PInvoke.WTSDisconnectSession(WTS_CURRENT_SERVER_HANDLE, (uint) sessionId, wait);
    }

    #region Private Methods & Properties

    private static readonly RegistryKey LocalMachineRegister = (Is64BitOperatingSystem)
        ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
        : RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);

    private unsafe void* _oldWow64RedirectionValue = null;
    private readonly string _installPathExpanded;
    private readonly string _installPath;

    public HimuRdpServices(string installPath = @"%ProgramFiles%\RDP Wrapper")
    {
        _installPath = installPath;
        _installPathExpanded = ExpandEnvironmentVariables(installPath);
    }

    private static bool IsWindows7OrHigher()
    {
        OperatingSystem operatingSystem = OSVersion;
        return operatingSystem is { Platform: PlatformID.Win32NT, Version.Major: >= 6 };
    }

    private static bool IsSupportedArchitecture()
    {
        return RuntimeInformation.OSArchitecture == Architecture.X86
               || RuntimeInformation.OSArchitecture == Architecture.X64;
    }

    private readonly struct RegisterKeyWrapper
    {
        public RegisterKeyWrapper(RegistryKey key, bool writable)
        {
            Key = key;
            Writable = writable;
        }

        public RegistryKey Key { get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public bool Writable { get; init; }

        public static RegisterKeyWrapper OpenRegisterKey(string keyPath, bool writable)
        {
            var key = LocalMachineRegister.OpenSubKey(keyPath, writable)
                      ?? throw new InvalidRegisterException(RegistryHive.LocalMachine, keyPath);
            return new RegisterKeyWrapper(key, writable);
        }

        public RegisterKeyWrapper SetValue(string name, object value, RegistryValueKind valueKind)
        {
            Key.SetValue(name, value, valueKind);
            return this;
        }

        public void Close()
        {
            Key.Close();
        }
    }

    /// <summary>
    /// Run a command line program with arguments.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="TimeoutException"></exception>
    // ReSharper disable once UnusedMethodReturnValue.Local
    private static int RunCmd(string program, string arguments, bool assertFailed)
    {
        ProcessStartInfo startInfo = new()
        {
            CreateNoWindow = true,
            FileName = program,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = assertFailed,
        };
        using Process process =
            Process.Start(startInfo) ?? throw new InvalidOperationException("Process.Start failed.");
        process.WaitForExit(5000);
        if (!process.HasExited)
            throw new TimeoutException($"Process(\"{program} {arguments}\").WaitForExit timeout.");
        int code = process.ExitCode;
        // ReSharper disable once InvertIf
        if (code != 0 && assertFailed)
        {
            StreamReader reader = process.StandardOutput;
            throw new InvalidOperationException(
                $"Process(\"{program} {arguments}\") failed with code {code}.\n{reader.ReadToEnd()}");
        }

        return code;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static unsafe string ConvertRawBytesToIpAddress(WTS_CLIENT_ADDRESS* address)
    {
        const uint AF_INET = 2, AF_INET6 = 23;
        return address->AddressFamily switch
        {
            AF_INET => $"{address->Address[2]}.{address->Address[3]}.{address->Address[4]}.{address->Address[5]}",
            AF_INET6 =>
                $"{address->Address[2]:X2}{address->Address[3]:X2}:{address->Address[4]:X2}{address->Address[5]:X2}:{address->Address[6]:X2}{address->Address[7]:X2}:{address->Address[8]:X2}{address->Address[9]:X2}:{address->Address[10]:X2}{address->Address[11]:X2}:{address->Address[12]:X2}{address->Address[13]:X2}:{address->Address[14]:X2}{address->Address[15]:X2}",
            _ => string.Empty
        };
    }

    #endregion
}
