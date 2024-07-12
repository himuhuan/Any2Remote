using System.Diagnostics;
using System.ServiceProcess;
using Any2Remote.Windows.AdminClient.Core.Contracts.Services;
using Any2Remote.Windows.Shared.Helpers;
using Any2Remote.Windows.Shared.Models;
using HimuRdp.Core;
using Microsoft.Win32;

namespace Any2Remote.Windows.AdminClient.Core.Services;

public class RdpServiceHelper : IRdpService
{
    public ServiceStatusInfo GetServiceStatus()
    {
        ServiceStatusInfo result = new();
        if (!HimuRdpServices.CheckEnvironment())
        {
            result.Status = ServiceStatus.NotSupported;
            result.Message = "Unsupported processor architecture or operating system.";
            return result;
        }
        result.Status |= GetServerStatus() | GetTermsrvStatus();

        // check enhance mode
        var checkEnhanceModeInstalled = HimuRdpServices.CheckInstallation();
        if (checkEnhanceModeInstalled.ErrorCode == HimuRdpError.NotSupported)
        {
            result.Status |= ServiceStatus.NoEnhanceModeSupport;
            result.Message = checkEnhanceModeInstalled.Message;
            return result;
        }
        if (checkEnhanceModeInstalled.ErrorCode != HimuRdpError.Success)
            return result;
        var checkEnhanceModeVersion = HimuRdpServices.CheckTermsrvVersion();
        if (checkEnhanceModeVersion.ErrorCode != HimuRdpError.Success)
        {
            result.Status |= ServiceStatus.NoEnhanceModeSupport;
            result.Message = checkEnhanceModeVersion.Message;
            return result;
        }
        result.Status |= ServiceStatus.InstalledEnhanceMode;
        return result;
    }

    private static ServiceStatus GetServerStatus()
    {
        var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server", false);
        var remoteAppKey = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\TSAppAllowList", false);

        if (key == null || remoteAppKey == null)
        {
            return ServiceStatus.NoRdpSupported;
        }

        var denyConnection = key.GetValue("fDenyTSConnections") as int?;
        var allowRemoteApps = remoteAppKey.GetValue("fDisabledAllowList") as int?;
        if (!denyConnection.HasValue || denyConnection.Value == 1
                                     || !allowRemoteApps.HasValue || allowRemoteApps.Value == 0)
        {
            return ServiceStatus.NotInitializeServer;
        }

        // check CA certificate
        string certificatePath = Path.Combine(WindowsCommon.Any2RemoteAppDataFolder, "certificate.json");
        if (!File.Exists(certificatePath))
        {
            return ServiceStatus.NotInitializeServer;
        }

        Process[] processes = Process.GetProcessesByName("Any2Remote.Windows.Server");
        return processes.Length == 0 ? ServiceStatus.None : ServiceStatus.ServerRunning;
    }

    private static ServiceStatus GetTermsrvStatus()
    {
        ServiceController? termService = HimuRdpServices.GetTermsrvServiceController();
        if (termService != null && termService.Status == ServiceControllerStatus.Running)
        {
            return ServiceStatus.TermsrvRunning;
        }
        return ServiceStatus.None;
    }
}