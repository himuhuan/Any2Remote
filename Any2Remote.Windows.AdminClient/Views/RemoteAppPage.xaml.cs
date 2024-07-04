using Any2Remote.Windows.AdminClient.Helpers;
using Any2Remote.Windows.AdminClient.Models;
using Any2Remote.Windows.AdminClient.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;


namespace Any2Remote.Windows.AdminClient.Views;

public sealed partial class RemoteAppPage : Page
{
    public RemoteAppViewModel ViewModel
    {
        get;
    }

    private HubConnection _hubConnection = default!;

    private async void InitializeSignalRAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(CoreServerClient.ServerRemoteHub)
            .Build();

        // auto reconnect
        _hubConnection.Closed += async (_) =>
        {
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await _hubConnection.StartAsync();
        };

        _hubConnection.On("RefreshRequired", () =>
        {
            var client = (Application.Current as App)!.CoreServerClient;
            var remoteApps = client.GetApplicationsAsync().Result;
            DispatcherQueue.TryEnqueue(() =>
            {
                ViewModel.RefreshRemoteApps(remoteApps);
            });
        });

        _hubConnection.On<string>("ping", (_) =>
        {
            // Get the connection id from the server
        });
        try
        {
            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            ContentDialog dialog = new()
            {
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "连接到远程服务器失败",
                XamlRoot = XamlRoot,
                PrimaryButtonText = "好的",
                DefaultButton = ContentDialogButton.Primary,
                Content = new TextBlock()
                {
                    Text = $"连接到远程服务器失败，请检查服务器是否已启动并且网络连接正常。错误信息：{ex.Message}",
                    TextWrapping = TextWrapping.Wrap
                }
            };

            await dialog.ShowAsync();
        }
    }

    public RemoteAppPage()
    {
        // _coreServerService = coreServerService;
        ViewModel = App.GetService<RemoteAppViewModel>();

        InitializeComponent();
        InitializeSignalRAsync();
    }

    private void RemoteAppItemEditBtn_Click(object sender, RoutedEventArgs e)
    {
        RemoteApplicationListModel model = (RemoteApplicationListModel) ((MenuFlyoutItem) sender).DataContext;
        Frame.Navigate(typeof(EditRemoteAppPage), model);
    }

    private async void RemoteAppItemRemoveBtn_Click(object sender, RoutedEventArgs e)
    {
        RemoteApplicationListModel model = (RemoteApplicationListModel) ((MenuFlyoutItem) sender).DataContext;

        ContentDialog dialog = new()
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = $"删除 {model.DisplayName}",
            XamlRoot = XamlRoot,
            PrimaryButtonText = "取消",
            SecondaryButtonText = "好的",
            DefaultButton = ContentDialogButton.Primary,
            Content = new TextBlock()
            {
                Text =
                $"{model.DisplayName} （位于 {model.Path}, 工作目录：{model.WorkingDirectory}）将被从发布列表中移除，所有已连接到并正在使用本项 Remote App 的客户端并不会受到影响。删除此项并不会影响本地机器的应用程序。此操作不可撤回。",
                TextWrapping = TextWrapping.Wrap
            }
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Secondary)
        {
            await ViewModel.RemoveRemoteApp(model);
        }
    }

    private void NavToPublishRemoteAppBtn_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(PublishRemoteAppPage));
    }
}
