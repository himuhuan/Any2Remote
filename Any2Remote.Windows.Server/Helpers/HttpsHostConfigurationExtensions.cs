using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Any2Remote.Windows.Shared.Exceptions;
using Any2Remote.Windows.Shared.Helpers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Any2Remote.Windows.Server.Helpers;

public static class HttpsHostConfigurationExtensions
{
    public static WebApplicationBuilder ConfigureAny2RemoteServer(this WebApplicationBuilder builder)
    {
        // default limit is 512MB.
        long maxRequestBodySize 
            = builder.Configuration.GetValue<long>("MaxRequestBodySize", 536870912);
        string certConfigPath = Path.Combine(WindowsCommon.Any2RemoteAppDataFolder, "certificate.json");

        using FileStream stream = File.OpenRead(certConfigPath);
        CertificateConfiguration configuration = JsonSerializer.Deserialize<CertificateConfiguration>(stream) ??
                                                 throw new ServerStartException("Bad certificate configuration");
        Console.WriteLine("[Startup][{0}] using certificate \"{1}\"({2})", nameof(ConfigureAny2RemoteServer),
            configuration.CertificatePath, configuration.DnsName);
        
        var certificate = new X509Certificate2(configuration.CertificatePath, configuration.Password);
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = maxRequestBodySize;
            // Rest api & SignalR can be accessed from any IP
            options.ListenAnyIP(7132, listenOptions =>
            {
                listenOptions.UseHttps(certificate);
            });
            // also, we need a way to download self-signed certificate
            options.ListenAnyIP(7131);
            // gRPC only can be accessed from localhost via https
            options.ListenLocalhost(7133, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
                listenOptions.UseHttps(certificate);
            });
        });

        builder.Services.Configure<FormOptions>(x =>
        {
            x.MultipartBodyLengthLimit = maxRequestBodySize;
            x.MemoryBufferThreshold = 10 * 1024 * 1024;
        });

        return builder;
    }


    internal class CertificateConfiguration
    {
        public string DnsName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string CertificatePath { get; set; } = default!;
    }
}