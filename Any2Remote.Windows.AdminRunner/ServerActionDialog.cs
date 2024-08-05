using Microsoft.Win32;
using System.Diagnostics;
using System.Management.Automation;
using System.ServiceProcess;
using Any2Remote.Windows.Shared.Exceptions;
using Any2Remote.Windows.Shared.Helpers;
using Any2Remote.Windows.Shared.Models;
using HimuRdp.Core;
using System.Management.Automation.Runspaces;

namespace Any2Remote.Windows.AdminRunner;

public partial class ServerActionDialog : Form
{
    public readonly string[] StartupArgs;

    public ServerActionDialog(string[] args)
    {
        StartupArgs = args;
        InitializeComponent();
        Icon icon = SystemIcons.Information;
        iconBox.Image = icon.ToBitmap();
    }

    private async void InitializeServerForm_Load(object sender, EventArgs e)
    {
        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.MarqueeAnimationSpeed = 30;

        switch (StartupArgs[1])
        {
            case "init":
                ConfigureCommonServices();
                messageLabel.Text = "Any2Remote 正在配置 Microsoft Windows ...";
                break;
            case "startup":
                messageLabel.Text = "Any2Remote 正在配置 Microsoft Windows ...";
                ConfigureCommonServices();
                ServerInitializeDialog dialog = new();
                dialog.ShowDialog();
                dialog.Dispose();
                break;
            case "reset":
                messageLabel.Text = "Any2Remote 正在执行重置操作，所有相关项将被移除。";
                ResetAll();
                break;
            case "start":
                messageLabel.Text = "Any2Remote 正在启动 Any2Remote Server ...";
                if (StartupArgs.Length != 3)
                    throw new ArgumentException("Expected 3 arguments for 'start' action");
                Start(StartupArgs[2]);
                break;
            case "restart":
                messageLabel.Text = "Any2Remote 正在重启服务 ...";
                Restart(StartupArgs[2]);
                break;
            case "start-dev":
                messageLabel.Text = "Any2Remote 正在启动 Any2Remote Server (开发模式) ...";
                if (StartupArgs.Length != 3)
                    throw new ArgumentException("Expected 3 arguments for 'start' action");
                Start(StartupArgs[2], false, true);
                break;
            case "stop":
                messageLabel.Text = "Any2Remote 正在终止后台服务...";
                StopServer();
                break;
            case "logoff":
                if (StartupArgs.Length != 4)
                    throw new ArgumentException("Expected 4 arguments for 'logoff' action");
                messageLabel.Text = $"Any2Remote 正在注销位于 {StartupArgs[3]} 的用户会话...";
                HimuRdpServices.LogoffSession(uint.Parse(StartupArgs[2]), true);
                break;
            case "disconnect":
                messageLabel.Text = $"Any2Remote 正在断开位于 {StartupArgs[3]} 的用户会话的连接...";
                if (StartupArgs.Length != 4)
                    throw new ArgumentException("Expected 4 arguments for 'disconnect' action");
                HimuRdpServices.DisconnectSession(uint.Parse(StartupArgs[2]), true);
                break;
            case "click-once":
                if (StartupArgs.Length != 3)
                    throw new ArgumentException("Expected 3 arguments for 'click-once' action");
                await Task.Run(() =>
                {
                    ConfigureCommonServices();
                    InstallEnhanceMode();
                    Invoke(() =>
                    {
                        messageLabel.Text = "Any2Remote 正在配置证书服务...";
                    });
                    ServerInitializeDialog certDialog = new(StartupArgs[2]);
                    certDialog.ShowDialog();
                    certDialog.Dispose();
                });
                break;
            default:
                throw new ArgumentException(StartupArgs[1]);
        }

        await Task.Delay(1000);
        Environment.Exit(0);
    }

    private static void ConfigureCommonServices(bool setServer = true)
    {
        var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server", true);
        var remoteAppKey = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\TSAppAllowList", true);

        if (key == null || remoteAppKey == null)
        {
            throw new ServerStatusException(ServiceStatus.NoRdpSupported,
                "The current version of Windows is not supported");
        }

        try
        {
            key.SetValue("fDenyTSConnections", setServer ? 0 : 1, RegistryValueKind.DWord);
            remoteAppKey.SetValue("fDisabledAllowList", (setServer) ? 1 : 0, RegistryValueKind.DWord);
            AddFirewallRule("7132", "Any2Remote HTTPS", "TCP");
            AddFirewallRule("7131", "Any2Remote HTTP", "TCP");
        }
        catch (Exception e)
        {
            throw new Any2RemoteException("Failed to initialize server.", e);
        }
    }

    private static bool IsServerRunning()
    {
        Process? process = Process.GetProcessesByName("Any2Remote.Windows.Server").FirstOrDefault();
        return process != null;
    }

    private static bool CheckTermsrvRunning()
    {
        ServiceController? service =
            ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "TermService");
        return service != null && service.Status == ServiceControllerStatus.Running;
    }

    private static void Start(string path, bool restartTermsrv = false, bool enableDevMode = false)
    {
        ServiceController? service = HimuRdpServices.GetTermsrvServiceController();
        if (service == null)
        {
            throw new Any2RemoteException("此设备上的 Windows 可能缺失了 Any2Remote 必须的文件，重新安装可能可以解决问题。（缺少服务 termsrv)");
        }

        if (IsServerRunning() && service.Status == ServiceControllerStatus.Running)
            return;

        ProcessStartInfo startInfo = new()
        {
            FileName = Path.Combine(path, "Any2Remote.Windows.Server.exe"),
            UseShellExecute = enableDevMode,
            CreateNoWindow = !enableDevMode,
            Verb = "runas",
            WorkingDirectory = path
        };
        try
        {
            _ = Process.Start(startInfo) ?? throw new ServerStartException();
        }
        catch (Exception ex)
        {
            throw new ServerStartException(ex.Message);
        }

        if (service.Status == ServiceControllerStatus.Running)
        {
            if (!restartTermsrv)
                return;
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
        }

        HimuRdpServices.ConfigureTermsrvDependenciesServices();
        service.StartServiceWithDepends();
        HimuRdpServices.ConfigureTermsrvFirewall(true);
    }

    private static void StopServer()
    {
        const string processName = "Any2Remote.Windows.Server";
        Process[] processes = Process.GetProcessesByName(processName);
        foreach (var process in processes)
        {
            if (!process.HasExited)
                process.Kill();
        }
    }

    private static void Restart(string path)
    {
        StopServer();
        Start(path, true);
    }

    private void InstallEnhanceMode()
    {
        HimuRdpServices installService = new();
        Invoke(() => messageLabel.Text = "Any2Remote 正在安装服务，请勿关闭计算机...");

        installService.Install();
        HimuRdpServices.ConfigureTermsrvDependenciesServices();
        HimuRdpServices.ConfigureTermsrvRegister(true);
        HimuRdpServices.ConfigureTermsrvFirewall(true);
        ServiceController? service = HimuRdpServices.GetTermsrvServiceController();
        if (service == null)
            throw new Any2RemoteException("此计算机上 Windows 可能缺少关键的系统文件，Any2Remote 无法启动 Termsrv 服务");
        if (service.Status == ServiceControllerStatus.Running)
        {
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
            service.StartServiceWithDepends();
            service.ConfigureStartMode(ServiceStartMode.Automatic);
        }
        Task.Delay(5000).Wait();
    }

    private enum ItemClearType
    {
        Register = 0,
        Directory = 1,
        Certificate = 2,
        EnhanceModePlugin = 3
    }

    private static readonly KeyValuePair<ItemClearType, string>[] ItemToClear =
    {
        new(ItemClearType.Directory, WindowsCommon.Any2RemoteAppDataFolder),
        new(ItemClearType.Register,
            "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Terminal Server\\TSAppAllowList\\Applications"),
        new(ItemClearType.Certificate, "any2remote"),
        new(ItemClearType.EnhanceModePlugin, string.Empty)
    };

    private static void ResetAll()
    {
        StopServer();
        ConfigureCommonServices(false);
        foreach (var item in ItemToClear)
        {
            switch (item.Key)
            {
                case ItemClearType.Directory:
                    WindowsCommon.DeleteDirectory(item.Value);
                    break;
                case ItemClearType.Register:
                    Registry.LocalMachine.DeleteSubKeyTree(item.Value, false);
                    break;
                case ItemClearType.Certificate:
                    using (PowerShell shell = PowerShell.Create())
                    {
                        string script = $"Get-ChildItem -Path Cert:\\LocalMachine\\Root\\ -DnsName *{item.Value}* "
                                        + $"| Remove-Item";
                        shell.AddScript(script);
                        shell.InvokeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    break;
                case ItemClearType.EnhanceModePlugin:
                    ServiceController? termsrv = HimuRdpServices.GetTermsrvServiceController();
                    bool installed = HimuRdpServices.CheckEnvironment() && HimuRdpServices.CheckInstallation().IsInstalled;
                    if (installed)
                    {
                        if (termsrv != null && termsrv.Status == ServiceControllerStatus.Running)
                        {
                            termsrv.Stop();
                            termsrv.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
                        }
                        HimuRdpServices services = new();
                        services.Uninstall();
                    }
                    termsrv?.Restart();
                    break;
                default:
                    // should not reach here!
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static void AddFirewallRule(string port, string name, string protocol)
    {
        string ruleName = $"Allow {name} on port {port}";
        string netshCommand = $"netsh advfirewall firewall add rule name=\"{ruleName}\" dir=in action=allow protocol={protocol} localport={port}";
        ProcessStartInfo startInfo = new()
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            Arguments = $"/c {netshCommand}",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        Process.Start(startInfo)?.WaitForExit();
    }
}
