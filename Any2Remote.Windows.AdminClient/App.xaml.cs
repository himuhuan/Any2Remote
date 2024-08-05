using Any2Remote.Windows.AdminClient.Activation;
using Any2Remote.Windows.AdminClient.Contracts.Services;
using Any2Remote.Windows.AdminClient.Core.Contracts.Services;
using Any2Remote.Windows.AdminClient.Core.Services;
using Any2Remote.Windows.AdminClient.Helpers;
using Any2Remote.Windows.AdminClient.Models;
using Any2Remote.Windows.AdminClient.Services;
using Any2Remote.Windows.AdminClient.ViewModels;
using Any2Remote.Windows.AdminClient.Views;
using Any2Remote.Windows.Grpc.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Any2Remote.Windows.AdminClient;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public CoreServerClient CoreServerClient { get; } = new CoreServerClient();

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder().UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Core Services
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Any2Remote Services
            services.AddGrpcClient<Local.LocalClient>(o =>
            {
                o.Address = new Uri("https://any2remote.local:7133");
            });
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<ILocalService, LocalWindowsService>();
            services.AddSingleton<IRdpService, RdpServiceProvider>();
            services.AddSingleton<ICoreServerClient, CoreServerClient>();

            // Views and ViewModels
            services.AddTransient<TermsrvSessionViewModel>();
            services.AddTransient<TermsrvSessionPage>();
            services.AddTransient<EditRemoteAppViewModel>();
            services.AddTransient<EditRemoteAppPage>();
            services.AddTransient<InstalledAppsListViewModel>();
            services.AddTransient<InstalledAppsListPage>();
            services.AddTransient<PublishRemoteAppViewModel>();
            services.AddTransient<PublishRemoteAppPage>();
            services.AddTransient<RemoteAppViewModel>();
            services.AddTransient<RemoteAppPage>();
            services.AddTransient<ServerViewModel>();
            services.AddTransient<ServerPage>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "内部错误",
            Content = $"Any2Remote 内部发生了错误。你请求的操作可能未完成或已失败。\n\n " +
            $"错误信息:\n\n{e.Message}\n\n请重试或联系支持。",
            CloseButtonText = "确定",
            XamlRoot = MainWindow.Content.XamlRoot,
        };
        await dialog.ShowAsync();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        await GetService<IActivationService>().ActivateAsync(args);
    }
}
