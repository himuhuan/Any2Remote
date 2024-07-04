using Any2Remote.Windows.AdminClient.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Any2Remote.Windows.AdminClient.Views;

public sealed partial class ServerPage : Page
{
    public ServerViewModel ViewModel
    {
        get;
    }

    public ServerPage()
    {
        ViewModel = App.GetService<ServerViewModel>();
        InitializeComponent();
    }

    private async void ResetButton_OnClick(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new()
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "重置 Any2Remote",
            Content = "注意: 重置 Any2Remote 将会删除所有的配置文件和数据。包括但不限于: 已发布的 Remote App 信息、" +
                      "其他用户的连接信息、上传的文件。\n\n 此操作不可撤回！",
            XamlRoot = XamlRoot,
            PrimaryButtonText = "是",
            SecondaryButtonText = "否",
            DefaultButton = ContentDialogButton.Secondary
        };

        var status = await dialog.ShowAsync();
        if (status == ContentDialogResult.Primary)
        {
            ViewModel.ResetApplication();
        }
    }
}
