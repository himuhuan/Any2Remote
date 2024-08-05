using Any2Remote.Windows.AdminClient.Core.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Any2Remote.Windows.Shared.Models;
using System.Diagnostics;
using Any2Remote.Windows.AdminClient.Core.Helpers;
using Any2Remote.Windows.AdminClient.Contracts.Services;
using Any2Remote.Windows.AdminClient.Views;

namespace Any2Remote.Windows.AdminClient.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IRdpService _rdpService;
    private readonly INavigationService _navigationService;

    public MainViewModel(IRdpService service, INavigationService navigationService)
    {
        _rdpService = service;
        _navigationService = navigationService;
        ComputerName = Environment.MachineName;
        _info = _rdpService.GetServiceStatus();
    }

    [ObservableProperty] private bool _runningTask;

    public string ComputerName { get; set; }

    private ServiceStatusInfo _info;

    public ServiceStatusInfo Status
    {
        get => _info;
        set
        {
            SetProperty(ref _info, value);
            OnPropertyChanged(nameof(StatusTitle));
            OnPropertyChanged(nameof(StatusIcon));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(StatusDescription));
            OnPropertyChanged(nameof(ServerActionButtonContent));
        }
    }

    public string StatusTitle
    {
        get
        {
            if (_info.NotSupported)
                return "您的设备不支持远程桌面服务";
            if (_info.RunningService)
                return "就绪";
            if (_info.NotInitializeServer || _info.RequireEnhanceMode)
                return "需要初始化";
            if (_info.TermsrvOnly || _info.ServerOnly)
                return "服务未启动";
            throw new SystemException($"Impossible status {_info.Status} at {nameof(StatusTitle)}");
        }
    }

    public string StatusIcon => _info.RunningService ? "\uE930" : "\uEB90";

    public string StatusColor => _info.RunningService ? "Green" : "Red";

    public string StatusDescription
    {
        get
        {
            if (_info.NotSupported)
                return $"您的设备不支持远程桌面服务。(错误信息：{_info.Message})";
            if (_info.FullSupport)
                return "Any2Remote 正在以增强模式运行";
            if (_info.RunningService)
                return "Any2Remote 正在以标准模式运行";
            if (_info.NotInitializeServer)
                return "Any2Remote 服务器没有进行初始化。";
            if (_info.RequireEnhanceMode)
                return "无法在此设备上运行标准模式下的 Any2Remote 服务，需要在你的设备配置增强模式。";
            if (_info.ServerOnly)
                return "Remote Desktop Service 无法启动或被终止";
            if (_info.TermsrvOnly)
                return "Any2Remote 服务器未启动";
            throw new SystemException($"Impossible status {_info.Status} at {nameof(StatusDescription)}");
        }
    }

    public string ServerActionButtonContent
    {
        get
        {
            if (_info.NotSupported)
                return "获取帮助";
            if (_info.RunningService)
                return "停止";
            if (_info.NotInitializeServer || _info.RequireEnhanceMode)
                return "开始配置";
            if (_info.ServerOnly || _info.TermsrvOnly)
                return "启动";
            throw new SystemException($"Impossible status {_info.Status} at {nameof(StatusTitle)}");
        }
    }

    public void ExecuteServerAction(bool devMode, bool clickOnce = false)
    {
        if (_info.NotSupported)
            NotSupportedHandle();
        if (_info.RunningService)
            StopServer();
        if (clickOnce)
        {
            ClickOnce();
        }
        else
        {
            if (_info.NotInitializeServer)
                InitializeService();
            if (_info.RequireEnhanceMode)
                EnableEnhanceMode();
        }
        if (_info.ServerOnly)
            StartTermsrv();
        if (_info.TermsrvOnly)
        {
            if (devMode)
                StartDevServer();
            else
                StartServer();
        }

        Status = _rdpService.GetServiceStatus();
    }

    private static void StartServer()
    {
        string serverRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Assets\Any2RemoteServer");
        string serverRoot = $"\"{serverRootPath}\"";
        AdminRunnerHelper.StartRunner("server", "start", serverRoot);
    }

    private static void ClickOnce()
    {
        string randomPassword = Guid.NewGuid().ToString("N");
        AdminRunnerHelper.StartRunner("server", "click-once", randomPassword);
    }

    private static void StopServer()
    {
        AdminRunnerHelper.StartRunner("server", "stop");
    }

    private static void StartDevServer()
    {
        string serverRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Assets\Any2RemoteServer");
        string serverRoot = $"\"{serverRootPath}\"";
        AdminRunnerHelper.StartRunner("server", "start-dev", serverRoot);
    }

    private static void InitializeService()
    {
        AdminRunnerHelper.StartRunner("server", "startup");
    }

    private static void NotSupportedHandle()
    {
        const string helpUrl =
            @"https://learn.microsoft.com/zh-cn/windows-server/remote/remote-desktop-services/clients/remote-desktop-clients";
        ProcessStartInfo process = new()
        {
            FileName = helpUrl,
            UseShellExecute = true,
        };
        Process.Start(process);
    }

    private static void StartTermsrv()
    {
        string serverRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Assets\Any2RemoteServer");
        string serverRoot = $"\"{serverRootPath}\"";
        AdminRunnerHelper.StartRunner("server", "restart", serverRoot);
    }

    private void EnableEnhanceMode()
    {
        _navigationService.NavigateTo(nameof(ServerPage));
    }
}
