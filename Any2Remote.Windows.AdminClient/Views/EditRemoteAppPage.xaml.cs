using Any2Remote.Windows.AdminClient.Core.Contracts.Services;
using Any2Remote.Windows.AdminClient.Models;
using Any2Remote.Windows.AdminClient.ViewModels;
using Any2Remote.Windows.AdminClient.Views.DialogContent;
using Any2Remote.Windows.Shared.Helpers;
using Any2Remote.Windows.Shared.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using ABI.Microsoft.UI.Windowing;
using static System.String;

namespace Any2Remote.Windows.AdminClient.Views;

public sealed partial class EditRemoteAppPage : Page
{
    private readonly ILocalService _service;

    public EditRemoteAppPage()
    {
        _service = App.GetService<ILocalService>();
        ViewModel = App.GetService<EditRemoteAppViewModel>();
        InitializeComponent();
    }

    public EditRemoteAppViewModel ViewModel { get; }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        // base.OnNavigatedTo(e);
        ContentPanel.Visibility = Visibility.Collapsed;
        LoadingPanel.Visibility = Visibility.Visible;
        switch (e.Parameter)
        {
            case RemoteApplication applicationToEdit:
                OnInitializeAsync(applicationToEdit);
                break;
            case LocalApplicationShowModel model:
                OnInitializeAsync(model);
                break;
            default:
            {
                ContentDialog errorDialog = new()
                {
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "发生内部错误",
                    XamlRoot = XamlRoot,
                    Content = "EditRemoteAppPage.OnNavigatedTo: Unexpected type passed as e.Parameter",
                    PrimaryButtonText = "好的",
                    DefaultButton = ContentDialogButton.Primary
                };
                await errorDialog.ShowAsync();
                Frame.Navigate(typeof(MainPage));
                break;
            }
        }

    }

    // publish from .lnk
    private async void OnInitializeAsync(RemoteApplication applicationToEdit)
    {
        await Task.Run(() =>
        {
            if (applicationToEdit.LocalInfo == null)
            {
                var result = WindowsCommon.GetAssociatedLocalApp(applicationToEdit);

                async void UiTask()
                {
                    var content = new ChoosePossibleLocalAppDialogContent(result);
                    ContentDialog chooseDialog = new()
                    {
                        Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                        Title = "确定要发布的应用程序",
                        XamlRoot = XamlRoot,
                        Content = content,
                        PrimaryButtonText = "确定",
                        SecondaryButtonText = "忽略",
                        CloseButtonText = "取消",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    var userChoose = await chooseDialog.ShowAsync();
                    switch (userChoose)
                    {
                        case ContentDialogResult.Primary when content.SelectedLocalApp != null:
                            ViewModel.RemoteApplication =
                                new RemoteApplicationListModel(content.SelectedLocalApp, applicationToEdit);
                            break;
                        case ContentDialogResult.None:
                            Frame.Navigate(typeof(MainPage));
                            break;
                        case ContentDialogResult.Secondary:
                        default:
                            ViewModel.RemoteApplication = new RemoteApplicationListModel(applicationToEdit);
                            IgnoreLocalInfoTips.Visibility = Visibility.Visible;
                            break;
                    }

                    LoadingPanel.Visibility = Visibility.Collapsed;
                    ContentPanel.Visibility = Visibility.Visible;
                }

                DispatcherQueue.TryEnqueue(UiTask);
            }
        });
    }

    // publish from local app
    private async void OnInitializeAsync(LocalApplicationShowModel localApplicationModel)
    {
        var lnkFilePaths = await _service.GetStartMenuLnkNamesAsync(localApplicationModel.RawInfo);
        DispatcherQueue.TryEnqueue(() =>
        {
            if (lnkFilePaths.Count > 0)
            {
                ViewModel.RemoteApplication = new RemoteApplicationListModel(localApplicationModel, 
                    WindowsCommon.GetApplicationInfoFromInk(lnkFilePaths.First())!);
            }
            else
            {
                ViewModel.RemoteApplication = new RemoteApplicationListModel(localApplicationModel);
                RequriedExecutableInfoBar.Visibility = Visibility.Visible;
            }
            ContentPanel.Visibility = Visibility.Visible;
            LoadingPanel.Visibility = Visibility.Collapsed;
        });
    }

    private async void PublishRemoteAppBtn_OnClick(object sender, RoutedEventArgs e)
    {
        if (IsNullOrEmpty(ViewModel.RemoteApplication.DisplayName)
            || IsNullOrEmpty(ViewModel.RemoteApplication.AppId)
            || IsNullOrEmpty(ViewModel.RemoteApplication.Path))
        {
            ContentDialog errorDialog = new()
            {
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "缺少必要的信息",
                XamlRoot = XamlRoot,
                Content = "缺少发布 Remote App 必要的信息，请重新填写表单后再试一遍。",
                PrimaryButtonText = "确定",
                DefaultButton = ContentDialogButton.Primary
            };
            await errorDialog.ShowAsync();
            return;
        }

        DispatcherQueue.TryEnqueue(PublishUiTask);
    }

    private async void PublishUiTask()
    {
        PublishProgressBar.Visibility = Visibility.Visible;
        ViewModel.PublishRemoteApp();
        await Task.Delay(1000);
        Frame.Navigate(typeof(RemoteAppPage));
        PublishProgressBar.Visibility = Visibility.Collapsed;
    }

    private async void ParseExcInfoFromFileBtn_OnClick(object sender, RoutedEventArgs e)
    {
        var filePicker = new FileOpenPicker();
        var hWnd = App.MainWindow.GetWindowHandle();
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hWnd);
        // Set options for your file picker
        filePicker.ViewMode = PickerViewMode.List;
        filePicker.SuggestedStartLocation = PickerLocationId.Desktop;
        filePicker.FileTypeFilter.Add(".lnk");
        filePicker.FileTypeFilter.Add(".exe");
        var file = await filePicker.PickSingleFileAsync();
        if (file == null)
        {
            return;
        }

        if (file.FileType == ".lnk")
        {
            ViewModel.RemoteApplication.SetExecutableInfo(WindowsCommon.GetApplicationInfoFromInk(file.Path)!);
            RequriedExecutableInfoBar.Visibility = Visibility.Collapsed;
        }
        else
        {
            var temp = ViewModel.RemoteApplication;
            ViewModel.RemoteApplication = null!;
            temp.Path = file.Path;
            temp.WorkingDirectory = Path.GetDirectoryName(file.Path) ?? Empty;
            ViewModel.RemoteApplication = temp;
            RequriedExecutableInfoBar.Visibility = Visibility.Collapsed;
        }
    }

}