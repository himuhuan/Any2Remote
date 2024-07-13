using Any2Remote.Windows.Shared.Exceptions;
using HimuRdp.Core;
using System.ServiceProcess;

namespace Any2Remote.Windows.AdminRunner;

public partial class ConfigureEnhanceModeDialog : Form
{
    // ReSharper disable once UnusedParameter.Local
    public ConfigureEnhanceModeDialog(string[] args)
    {
        InitializeComponent();
    }

    private async void buttonInstall_Click(object sender, EventArgs e)
    {
        HimuRdpServices installService = new();
        labelProgress.Visible = true;
        progressBar.Visible = true;
        buttonInstall.Enabled = false;

        await Task.Run(() =>
        {
            installService.Install();
            Invoke(() =>
            {
                labelProgress.Text = "Any2Remote 正在配置服务...";
            });
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
        });
        Environment.Exit(0);
    }

    private void ConfigureEnhanceModeDialog_Load(object sender, EventArgs e)
    {
        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.MarqueeAnimationSpeed = 30;
        progressBar.Visible = false;
        labelProgress.Visible = false;
        labelWindowsVersion.Text = Environment.OSVersion.ToString();
        labelTermsrvVersion.Text = HimuRdpServices.GetTermsrvVersion().ProductVersion;
        buttonUninstall.Enabled = buttonInstall.Enabled = false;
        bool envCheck = HimuRdpServices.CheckEnvironment();
        CheckResult termCheck = HimuRdpServices.CheckTermsrvVersion();
        CheckResult installCheck = HimuRdpServices.CheckInstallation();
        if (!envCheck)
            labelStatus.Text = "不支持";
        else if (termCheck.ErrorCode != HimuRdpError.Success)
            labelStatus.Text = $"不支持 ({termCheck.Message})";
        else if (installCheck.ErrorCode != HimuRdpError.Success)
        {
            labelStatus.Text = "未安装";
            buttonInstall.Enabled = true;
        }
        else if (installCheck.ErrorCode == HimuRdpError.Success)
        {
            labelStatus.Text = "已安装";
            buttonUninstall.Enabled = true;
        }
        else
            labelStatus.Text = $"不支持 ({installCheck.Message})";
    }

    private async void buttonUninstall_Click(object sender, EventArgs e)
    {
        HimuRdpServices installService = new();
        labelProgress.Text = "Any2Remote 正在卸载增强模式，并回退更改...";
        labelProgress.Visible = true;
        progressBar.Visible = true;
        buttonInstall.Enabled = false;

        await Task.Run(() =>
        {
            ServiceController? service = HimuRdpServices.GetTermsrvServiceController();
            if (service == null)
                throw new Any2RemoteException("此计算机上 Windows 可能缺少关键的系统文件，Any2Remote 无法启动 Termsrv 服务");
            if (service.Status == ServiceControllerStatus.Running)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
            }
            installService.Uninstall();
            Invoke(() =>
            {
                labelProgress.Text = "Any2Remote 正在配置服务...";
            });
            service.StartServiceWithDepends();
            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
            Task.Delay(5000).Wait();
        });
        Environment.Exit(0);
    }
}
