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
using FileAttributes = Windows.Storage.FileAttributes;

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
            var item = (await e.DataView.GetStorageItemsAsync())[0];
            string extension = Path.GetExtension(item.Path);
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (extension == ".lnk")
            {
                var app = new RemoteApplication(WindowsCommon.GetApplicationInfoFromInk(item.Path)!);
                Frame.Navigate(typeof(EditRemoteAppPage), app);
            }
            else if (extension == ".exe")
            {
                var applicationToPublish = new ExecutableApplication
                {
                    Path = item.Path,
                    DisplayName = item.Name,
                    WorkingDirectory = Path.GetDirectoryName(item.Path) ?? string.Empty
                };

                Frame.Navigate(typeof(EditRemoteAppPage), new RemoteApplication(applicationToPublish));
            }
            else if (item.Name == "Internet Explorer")
            {
                ContentDialog warningDialog = new()
                {
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "警告：过时的应用程序",
                    XamlRoot = XamlRoot,
                    Content = new TextBlock
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Text = "Internet Explorer 已不受现代 Microsoft Windows 支持。Any2Remote " +
                               "目前已对 Internet Explorer 做出特殊处理，您可以继续发布程序，" +
                               "但 Any2Remote 不保证在之后的 Windows 版本可用。\n\n" +
                               "如果 Internet Explorer 无法在 Windows 上运行，" +
                               "请使用 \"Any2Remote 工具\" -> \"Internet Explorer 支持\""
                    },
                    PrimaryButtonText = "是，仍然发布",
                    SecondaryButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                };
                var userChoose = await warningDialog.ShowAsync();
                if (userChoose == ContentDialogResult.Primary)
                {
                    RemoteApplication ieApp = new()
                    {
                        AppId = "Internet Explorer",
                        DisplayName = "Internet Explorer",
                        Path = "C:\\Program Files\\Internet Explorer\\iexplore.exe",
                        AppIconUrl = "C:\\Program Files\\Internet Explorer\\iexplore.exe",
                        WorkingDirectory = "C:\\Program Files\\Internet Explorer"
                    };
                    Frame.Navigate(typeof(EditRemoteAppPage), ieApp);
                }
            }
            else
            {
                errorDialog.Content = new TextBlock()
                {
                    Text = $"Any2Remote 无法分析类型为 \"{item.Attributes}\" 的文件"
                };
                await errorDialog.ShowAsync();
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
