using Any2Remote.Windows.Server.Helpers;
using Any2Remote.Windows.Server.Hubs;
using Any2Remote.Windows.Server.Services;
using Any2Remote.Windows.Server.Services.Contracts;

///////////////////////////////////////// Program.cs /////////////////////////////////////////
namespace Any2Remote.Windows.Server;

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
    public const string ServerReleaseVersion = "1.1.0";
    public const string ReleaseCode = "Yoshino";

    private static void Main(string[] args)
    {
        Console.WriteLine($"Any2Remote Server Version {ServerReleaseVersion} (Release {ReleaseCode})");
        Console.WriteLine("Copyright(c) 2024 Himu <himu.liuhuan@gmail.com>\n");

        var builder = WebApplication.CreateBuilder(args);

        builder.ConfigureAny2RemoteServer();

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

        app.UseCors("AllowAll");
        // app.UseHttpsRedirection(); 
        app.UseStaticFiles();

        app.UseAuthorization();
        app.MapHub<RemoteAppHub>("/remotehub");
        app.MapGrpcService<RpcServices.LocalWindowsService>();
        app.MapControllers();

        app.Run();
    }
}