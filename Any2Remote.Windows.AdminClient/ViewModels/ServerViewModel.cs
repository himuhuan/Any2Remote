using Any2Remote.Windows.AdminClient.Core.Contracts.Services;
using Any2Remote.Windows.AdminClient.Core.Helpers;
using Any2Remote.Windows.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using HimuRdp.Core;

namespace Any2Remote.Windows.AdminClient.ViewModels;

public partial class ServerViewModel : ObservableRecipient
{
    private ServiceStatusInfo _statusInfo;
    private readonly IRdpService _rdpService;

    #region Properties
    public ServiceStatusInfo StatusInfo
    {
        get => _statusInfo;
        set
        {
            SetProperty(ref _statusInfo, value);
            OnPropertyChanged(nameof(ServiceStatusText));
            OnPropertyChanged(nameof(EnhancedModeStatusText));
            OnPropertyChanged(nameof(ServerRunningStatusText));
            OnPropertyChanged(nameof(TermsrvVersion));
        }
    }

    public string ServiceStatusText
    {
        get
        {
            if (_statusInfo.FullSupport) return "运行中（增强模式）";
            if (_statusInfo.RunningService) return "运行中";
            return _statusInfo.NotInitializeServer ? "未初始化" : "服务异常";
        }
    }
    
    public string WindowsVersionText => Environment.OSVersion.ToString();
    public string TermsrvVersion => HimuRdpServices.GetTermsrvVersion().ProductVersion ?? "未找到 termsrv 或服务已损坏";
    public string ServerRunningStatusText => StatusInfo.Status.HasFlag(ServiceStatus.ServerRunning) ? "运行中" : "未运行";
    public string TermsrvStatusText => StatusInfo.Status.HasFlag(ServiceStatus.TermsrvRunning) ? "运行中" : "未运行";
    public string EnhancedModeStatusText => StatusInfo.Status.HasFlag(ServiceStatus.InstalledEnhanceMode) ? "是" : "否";
    #endregion

    public ServerViewModel(IRdpService rdpService)
    {
        _rdpService = rdpService;
        _statusInfo = _rdpService.GetServiceStatus();
    }

    public void ResetApplication()
    {
        AdminRunnerHelper.StartRunner("server", "reset");
        StatusInfo = _rdpService.GetServiceStatus();
    }

    public void RestartApplication()
    {
        string serverRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Assets\Any2RemoteServer");
        string serverRoot     = $"\"{serverRootPath}\"";
        AdminRunnerHelper.StartRunner("server", "restart", serverRoot);
        StatusInfo = _rdpService.GetServiceStatus();
    }

    public void ConfigureEnhancedMode()
    {
        AdminRunnerHelper.StartRunner("config-enhance-mode");
        StatusInfo = _rdpService.GetServiceStatus();
    }

    public void EnableInternetExplorerSupport()
    {
        throw new NotImplementedException();
    }
}
