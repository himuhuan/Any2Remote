using Any2Remote.Windows.AdminClient.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Any2Remote.Windows.AdminClient.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    private void NavToServerPageBtn_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(ServerPage));
    }

    private async void ServerActionButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RunningTask = true;
        try
        {
#if DEBUG
            if (ViewModel.Status.CanStartService)
            {
                ContentDialog dialog = new()
                {
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "开发者模式",
                    Content = "开发者模式将会显示后台服务器的运行暴露于控制台。",
                    XamlRoot = XamlRoot,
                    PrimaryButtonText = "以普通模式启动",
                    SecondaryButtonText = "以开发者模式启动",
                    DefaultButton = ContentDialogButton.Primary
                };

                var status = await dialog.ShowAsync();
                ViewModel.ExecuteServerAction(devMode: status == ContentDialogResult.Secondary);
            }
            else
            {
                ViewModel.ExecuteServerAction(devMode: false);
            }
#else
            ViewModel.ExecuteServerAction(devMode: false);
#endif
        }
        finally
        {
            ViewModel.RunningTask = false;
        }
    }

    private void NavToRemoteAppBtn_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(RemoteAppPage));
    }

    private void NavToSessionBtn_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(TermsrvSessionPage));
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Status.NotInitializeServer)
        {
            ContentDialog dialog = new()
            {
                Title = "立即初始化 Any2Remote",
                Content = "Any2Remote 即将准备就绪。\n\n要部署你的计算机上的 Any2Remote 服务，" +
                "请点击 \"立即配置\" 以一键配置 Any2Remote 服务。并在你的反病毒软件扫描白名单内添加:\n\n" +
                "C:\\Program Files\\HimuQAQ\\Any2Remote Manager\n" +
                "C:\\Program Files\\RDP Wrapper\n" +
                "\n你也可以选择\"稍后配置\"，并根据需要在各个页面选择要配置的服务。" +
                "\n\n警告：在配置 Any2Remote 服务完毕后，你需要及时恢复反病毒软件的实时扫描功能。",
                PrimaryButtonText = "立即配置 (推荐)",
                SecondaryButtonText = "稍后配置",
                XamlRoot = XamlRoot,
                DefaultButton = ContentDialogButton.Primary,
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.ExecuteServerAction(devMode: false, clickOnce: true);
            }
        }
    }
}
