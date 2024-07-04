using Microsoft.Win32;
using System.Diagnostics;
using System.Management.Automation;
using Any2Remote.Windows.Shared.Exceptions;
using Any2Remote.Windows.Shared.Helpers;
using Any2Remote.Windows.Shared.Models;

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
                SetServerRegisterKey();
                messageLabel.Text = "Any2Remote 正在配置 Microsoft Windows ...";
                break;
            case "startup":
                messageLabel.Text = "Any2Remote 正在配置 Microsoft Windows ...";
                SetServerRegisterKey();
                CreateNewCertificateDialog dialog = new();
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
                StartServer(StartupArgs[2]);
                break;
            case "start-dev":
                messageLabel.Text = "Any2Remote 正在启动 Any2Remote Server (开发模式) ...";
                if (StartupArgs.Length != 3)
                    throw new ArgumentException("Expected 3 arguments for 'start' action");
                StartServerWithShell(StartupArgs[2]);
                break;
            case "stop":
                messageLabel.Text = "Any2Remote 正在终止后台服务";
                StopServer();
                break;
            default:
                throw new ArgumentException(StartupArgs[1]);
        }

        await Task.Delay(1000);
        Environment.Exit(0);
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

    private static void StartServerWithShell(string path)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = Path.Combine(path, "Any2Remote.Windows.Server.exe"),
            UseShellExecute = true,
            Verb = "runas",
            WorkingDirectory = path
        };

        Process.Start(startInfo);
    }

    private static void SetServerRegisterKey(bool setServer = true)
    {
        var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Terminal Server", true);
        var remoteAppKey = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Terminal Server\TSAppAllowList", true);

        if (key == null || remoteAppKey == null)
        {
            throw new ServerStatusException(ServerStatus.NotSupported,
                "The current version of Windows is not supported");
        }
        try
        {
            key.SetValue("fDenyTSConnections", setServer ? 0 : 1, RegistryValueKind.DWord);
            remoteAppKey.SetValue("fDisabledAllowList", (setServer) ? 1 : 0, RegistryValueKind.DWord);
        }
        catch (Exception e)
        {
            throw new Any2RemoteException("Failed to initialize server.", e);
        }
    }

    private static void StartServer(string path)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = Path.Combine(path, "Any2Remote.Windows.Server.exe"),
            UseShellExecute = false,
            CreateNoWindow = true,
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
    }

    private enum ItemClearType
    {
        Register = 0,
        Directory = 1,
        Certificate = 2,
    }

    private static readonly KeyValuePair<ItemClearType, string>[] ItemToClear =
    {
        new(ItemClearType.Directory, WindowsCommon.Any2RemoteAppDataFolder),
        new(ItemClearType.Register,
            "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Terminal Server\\TSAppAllowList\\Applications"),
        new(ItemClearType.Certificate, "any2remote")
    };

    private static void ResetAll()
    {
        StopServer();
        SetServerRegisterKey(false);
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
                        string script = $"Get-ChildItem -Path Cert:\\LocalMachine\\Root\\ -DnsName *{item.Value}* " +
                                        $"| Remove-Item";
                        shell.AddScript(script);
                        shell.InvokeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    break;
                default:
                    // should not reach here!
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}