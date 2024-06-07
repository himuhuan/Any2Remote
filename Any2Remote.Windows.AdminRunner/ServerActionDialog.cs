using Microsoft.Win32;
using System.Diagnostics;
using Any2Remote.Windows.Shared.Exceptions;
using Any2Remote.Windows.Shared.Models;

namespace Any2Remote.Windows.AdminRunner
{
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
                    InitializeServer();
                    messageLabel.Text = "Any2Remote 正在配置 Microsoft Windows ...";
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
            string processName = "Any2Remote.Windows.Server";
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

        private static void InitializeServer()
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
                key.SetValue("fDenyTSConnections", 0, RegistryValueKind.DWord);
                remoteAppKey.SetValue("fDisabledAllowList", 1, RegistryValueKind.DWord);
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
                var process = Process.Start(startInfo) ?? throw new ServerStartException();
            }
            catch (Exception ex)
            {
                throw new ServerStartException(ex.Message);
            }
        }
    }
}
