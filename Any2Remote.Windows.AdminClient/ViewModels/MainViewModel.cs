using Any2Remote.Windows.AdminClient.Core.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Any2Remote.Windows.Shared.Models;

namespace Any2Remote.Windows.AdminClient.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    #region Initialization & Dependencies Injection
    private readonly ILocalService _localService;

    public MainViewModel(ILocalService service)
    {
        _localService = service;
        ComputerName = Environment.MachineName;
        _status = _localService.GetServerStatus();
    }
    #endregion

    [ObservableProperty]
    private bool _runningTask;

    public string ComputerName
    {
        get; set;
    }

    private ServerStatus _status;
    public ServerStatus Status
    {
        get => _status;
        set
        {
            SetProperty(ref _status, value);
            OnPropertyChanged(nameof(StatusTitle));
            OnPropertyChanged(nameof(StatusIcon));
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(StatusDescription));
            OnPropertyChanged(nameof(ServerActionButtonContent));
        }
    }

    public string StatusTitle => _status switch
    {
        ServerStatus.Connected => "准备就绪",
        ServerStatus.NotInitialized => "未初始化",
        ServerStatus.Disconnected => "服务器离线",
        ServerStatus.NotSupported => "您的系统不支持远程桌面连接服务",
        _ => "Unknown"
    };

    public string StatusIcon => _status switch
    {
        ServerStatus.Connected => "\uE930",
        _ => "\uEB90",
    };

    public string StatusColor => _status switch
    {
        ServerStatus.Connected => "Green",
        _ => "Red"
    };

    public string StatusDescription => _status switch
    {
        ServerStatus.Connected => "服务器已启动，可以接收远程连接请求",
        ServerStatus.NotInitialized => "服务器未初始化，点击右侧按钮进行初始设置",
        ServerStatus.Disconnected => "服务器离线或无法启动",
        ServerStatus.NotSupported
            => "您的系统不支持远程桌面连接服务, Remote Desktop Service 服务需要 Windows 专业版或更高版本的支持。",
        _ => "未知错误，系统内部错误。"
    };

    public ICommand ServerActionCommand => new RelayCommand(() =>
    {
        ExecuteServerAction(false);
    });

    public void ExecuteServerAction(bool devMode)
    {
        switch (_status)
        {
            case ServerStatus.Connected:
                StopServer();
                break;
            case ServerStatus.Disconnected:
                if (devMode) 
                    StartDevServer();
                else 
                    StartServer();
                break;
            case ServerStatus.NotInitialized:
                InitServer();
                break;
            default:
                break;
        }
    }

    public string ServerActionButtonContent => _status switch
    {
        ServerStatus.Connected => "停止服务器",
        ServerStatus.Disconnected => "启动服务器",
        ServerStatus.NotInitialized => "立刻初始化",
        ServerStatus.NotSupported => "升级到 Windows 专业版",
        _ => "立刻初始化"
    };

    private void StartServer()
    {
        RunningTask = true;
        _localService.StartServer();
        Status = ServerStatus.Connected;
        RunningTask = false;
    }

    private void StopServer()
    {
        RunningTask = true;
        _localService.StopServer();
        Status = ServerStatus.Disconnected;
        RunningTask = false;
    }

    private void StartDevServer()
    {
        RunningTask = true;
        _localService.StartDevServer();
        Status = ServerStatus.Connected;
        RunningTask = false;
    }

    private void InitServer()
    {
        RunningTask = true;
        _localService.InitServer();
        StartServer();
    }
}
