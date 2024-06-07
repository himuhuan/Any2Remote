using Any2Remote.Windows.Server.Hubs;
using Any2Remote.Windows.Server.Services;
using Any2Remote.Windows.Server.Services.Contracts;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

///////////////////////////////////////// Program.cs /////////////////////////////////////////
///
/// This is a part of the Any2Remote project.
/// Any2Remote.Windows.Server is the server-side application (on Windows) that 
/// manages the remote connections and the remote desktops.
/// 
/// Any2Remote.Windows.Server also provides a gPRC service 
/// for other applications on the same machine.
///
/////////////////////////////////////////////////////////////////////////////////////////////

internal class Program
{
    public const string ServerReleaseVersion = "1.0.1.1";

    private static void Main(string[] args)
    {
        Console.WriteLine($"Any2Remote Server Version {ServerReleaseVersion} (Release)");
        Console.WriteLine("Copyright(c) 2024 Himu <himu.liuhuan@gmail.com>\n");

        var builder = WebApplication.CreateBuilder(args);

        // default limit is 512MB.
        long maxRequestBodySize 
            = builder.Configuration.GetValue<long>("MaxRequestBodySize", 536870912);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = maxRequestBodySize;
            // Rest api & SignalR can be accessed from any IP
            options.ListenAnyIP(7132, configure =>
            {
                configure.UseHttps();
            });
            // gRPC only can be accessed from localhost via https
            options.ListenLocalhost(7133, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
                listenOptions.UseHttps();
            });
        });

        builder.Services.Configure<FormOptions>(x =>
        {
            x.MultipartBodyLengthLimit = maxRequestBodySize;
            x.MemoryBufferThreshold = 10 * 1024 * 1024;
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddGrpc();
        builder.Services.AddSignalR();

        builder.Services.AddSingleton<IRemoteAppService, RemoteAppService>();

        builder.Services.AddLogging(logBuilder =>
        {
            logBuilder.AddConsole();
            logBuilder.SetMinimumLevel(LogLevel.Debug);
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policyBuilder =>
            {
                policyBuilder.SetIsOriginAllowed(_ => true)
                    .AllowCredentials()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHsts();
        }

        app.UseCors("AllowAll");

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseAuthorization();
        app.MapHub<RemoteAppHub>("/remotehub");
        app.MapGrpcService<Any2Remote.Windows.Server.RpcServices.LocalWindowsService>();
        app.MapControllers();

        app.Run();
    }
}