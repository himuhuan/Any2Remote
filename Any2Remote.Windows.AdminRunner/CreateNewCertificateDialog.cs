using System.Management.Automation;
using System.Security;
using System.Text.Json;
using Any2Remote.Windows.Shared.Helpers;
using FileStream = System.IO.FileStream;

namespace Any2Remote.Windows.AdminRunner;

public partial class CreateNewCertificateDialog : Form
{
    public CreateNewCertificateDialog()
    {
        InitializeComponent();
    }

    private async void buttonCreateCert_Click(object sender, EventArgs e)
    {
        buttonCreateCert.Enabled = false;
        progressBar1.Style = ProgressBarStyle.Marquee;
        progressBar1.MarqueeAnimationSpeed = 30;
        progressBar1.Visible = true;
        labelTips.Visible = true;

        try
        {
            using PowerShell shell = PowerShell.Create();
            string thumbprint = await CreateSelfSignedCertificate(shell);
            string certificatePath = await ExportCertificate(shell, thumbprint);
            await InstallCertificate(shell, certificatePath);
            await ExportCertificateInfo(certificatePath);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"{ex.Source} 报告了一个致命错误，操作已被终止。\n\n {ex.Message}:\n {ex.StackTrace}",
                "发生致命错误",
                MessageBoxButtons.OK,
                MessageBoxIcon.Stop);
            Environment.Exit(1);
        }
    }

    private async Task<string> CreateSelfSignedCertificate(PowerShell shell)
    {
        Invoke(() => labelInfo.Text = "Any2Remote 正在创建自签名证书···");
        DateTime endDate = DateTime.Now.AddYears(100);
        shell.AddCommand("New-SelfSignedCertificate")
            .AddParameter("CertStoreLocation", "Cert:\\LocalMachine\\My")
            .AddParameter("DnsName", certDnsName.Text)
            .AddParameter("NotAfter", endDate);
        var createdCertificate = (await shell.InvokeAsync().ConfigureAwait(false))[0];
        var thumbprint = createdCertificate.Members["Thumbprint"].Value as string
                         ?? throw new RuntimeException("CreateSelfSignedCertificate Failed!");
        shell.Commands.Clear();
        return thumbprint;
    }

    private async Task<string> ExportCertificate(PowerShell shell, string thumbprint)
    {
        Invoke(() => labelInfo.Text = "Any2Remote 正在导出证书···");
        string certificatePath = Path.Combine(WindowsCommon.Any2RemoteAppDataFolder, "Certificates");
        if (!Directory.Exists(certificatePath))
            Directory.CreateDirectory(certificatePath);
        string pfxScript = "Export-PfxCertificate " +
                           $"-Cert \"Cert:\\LocalMachine\\My\\{thumbprint}\" " +
                           $"-FilePath \"{certificatePath}\\any2remote.pfx\" " +
                           $"-Password (ConvertTo-SecureString -String \"{certPassword.Text}\" -Force -AsPlainText)";
        shell.AddScript(pfxScript);
        await shell.InvokeAsync().ConfigureAwait(false);
        shell.Commands.Clear();
        shell.AddCommand("Export-Certificate")
            .AddParameter("Cert", $"Cert:\\LocalMachine\\My\\{thumbprint}")
            .AddParameter("FilePath", $"{certificatePath}\\any2remote.cer");
        await shell.InvokeAsync().ConfigureAwait(false);
        shell.Commands.Clear();
        return certificatePath;
    }

    private async Task InstallCertificate(PowerShell shell, string certificatePath)
    {
        Invoke(() => labelInfo.Text = "Any2Remote 正在安装证书···");
        shell.AddCommand("Import-Certificate")
            .AddParameter("FilePath", $"{certificatePath}\\any2remote.cer")
            .AddParameter("CertStoreLocation", "Cert:\\LocalMachine\\Root");
        await shell.InvokeAsync().ConfigureAwait(false);
        shell.Commands.Clear();

        // DNS
        await using StreamWriter streamWriter = File.AppendText("C:\\Windows\\System32\\drivers\\etc\\hosts");
        await streamWriter.WriteLineAsync("\r\n# Added by Any2Remote");
        await streamWriter.WriteLineAsync($"127.0.0.1 {certDnsName.Text}");
        shell.AddScript("ipconfig /flushdns");
        await shell.InvokeAsync().ConfigureAwait(false);
        shell.Commands.Clear();
    }

    private async Task ExportCertificateInfo(string certificatePath)
    {
        Invoke(() => labelInfo.Text = "Any2Remote 正在导出证书信息···");
        var objectToExport = new
        {
            DnsName = certDnsName.Text,
            Password = certPassword.Text,
            CertificatePath = Path.Combine(certificatePath, "any2remote.pfx")
        };

        string exportPath = Path.Combine(WindowsCommon.Any2RemoteAppDataFolder, "certificate.json");
        await using FileStream createStream = File.Create(exportPath);
        await JsonSerializer.SerializeAsync(createStream, objectToExport);
    }
}