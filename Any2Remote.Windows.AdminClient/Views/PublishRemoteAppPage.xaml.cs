using Any2Remote.Windows.AdminClient.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.AspNetCore.SignalR.Client;
using Any2Remote.Windows.Shared.Helpers;
using Any2Remote.Windows.Shared.Models;
using Google.Protobuf.WellKnownTypes;
using Windows.Storage.Pickers;
using static System.String;

namespace Any2Remote.Windows.AdminClient.Views;

public sealed partial class PublishRemoteAppPage : Page
{
    public PublishRemoteAppViewModel ViewModel
    {
        get;
    }

    private HubConnection _hubConnection = default!;
    private async void InitializeSignalRAsync()
    {
        // TODO: https cretificate required
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7132/remotehub")
            .Build();

        // auto reconnect
        _hubConnection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await _hubConnection.StartAsync();
        };

        _hubConnection.On("RefreshRequired", () =>
        {

        });

        _hubConnection.On<string>("ping", (connectId) =>
        {
            // Get the connection id from the server
        });

        await _hubConnection.StartAsync();
    }

    public PublishRemoteAppPage()
    {
        ViewModel = App.GetService<PublishRemoteAppViewModel>();
        InitializeComponent();
        InitializeSignalRAsync();
    }

    private void PublishRemoteAppDropArea_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Link;
    }

    private async void PublishRemoteAppDropArea_Drop(object sender, DragEventArgs e)
    {
        ContentDialog errorDialog = new()
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "发布 Remote App 失败",
            XamlRoot = XamlRoot,
            PrimaryButtonText = "好的",
            DefaultButton = ContentDialogButton.Primary
        };

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            foreach (var item in items)
            {
                if (!item.Path.EndsWith(".lnk"))
                {
                    errorDialog.Content = new TextBlock()
                    {
                        Text = "非法格式：只接收快捷方式 (.lnk) 文件",
                    };
                    await errorDialog.ShowAsync();
                }
                else
                {
                    var app = new RemoteApplication(WindowsCommon.GetApplicationInfoFromInk(item.Path)!);
                    Frame.Navigate(typeof(EditRemoteAppPage), app);
                    // ViewModel.PublishRemoteApp(item.Path, _hubConnection);
                }
            }
        }
    }

    private void PublishInstalledAppBtn_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(InstalledAppsListPage));
    }

    private async void PublishFileSystemAppBtn_OnClick(object sender, RoutedEventArgs e)
    {
        var filePicker = new FileOpenPicker();
        var hWnd = App.MainWindow.GetWindowHandle();
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hWnd);
        filePicker.ViewMode = PickerViewMode.List;
        filePicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
        filePicker.FileTypeFilter.Add(".exe");
        var file = await filePicker.PickSingleFileAsync();
        if (file == null)
        {
            return;
        }
        var applicationToPublish = new ExecutableApplication
        {
            Path = file.Path,
            DisplayName = file.DisplayName,
            WorkingDirectory = Path.GetDirectoryName(file.Path) ?? string.Empty
        };

        Frame.Navigate(typeof(EditRemoteAppPage), new RemoteApplication(applicationToPublish));
    }

    private async void PublishLnkAppBtn_OnClick(object sender, RoutedEventArgs e)
    {
        var filePicker = new FileOpenPicker();
        var hWnd = App.MainWindow.GetWindowHandle();
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hWnd);
        filePicker.ViewMode = PickerViewMode.List;
        filePicker.SuggestedStartLocation = PickerLocationId.Desktop;
        filePicker.FileTypeFilter.Add(".lnk");
        var file = await filePicker.PickSingleFileAsync();
        if (file == null)
        {
            return;
        }
        var applicationToPublish = WindowsCommon.GetApplicationInfoFromInk(file.Path);
        if (applicationToPublish != null)
            Frame.Navigate(typeof(EditRemoteAppPage), new RemoteApplication(applicationToPublish));
    }
}
