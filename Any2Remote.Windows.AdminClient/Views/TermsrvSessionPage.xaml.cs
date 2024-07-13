using Any2Remote.Windows.AdminClient.Core.Models;
using Any2Remote.Windows.AdminClient.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Any2Remote.Windows.AdminClient.Views;

public sealed partial class TermsrvSessionPage : Page
{
    public TermsrvSessionViewModel ViewModel { get; }

    public TermsrvSessionPage()
    {
        ViewModel = App.GetService<TermsrvSessionViewModel>();
        InitializeComponent();
    }

    private async void TerminateConnectionBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TsSessionModel model = (TsSessionModel)((Button)sender).DataContext;
        ContentDialog dialog = new()
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = $"中断来自 {model.FullAddress} 的连接吗？",
            XamlRoot = XamlRoot,
            Content = new TextBlock
            {
                Text =
                    $"中断来自 {model.FullAddress} (Id = {model.SessionId}) 的连接将会使得连接到该会话的客户端上的远程连接" +
                    $"与 Remote App 立刻终止。" +
                    $"\n\n指定的远程桌面服务会话将被立即注销，如果还有正在运行的工作，相关数据将会丢失。",
                TextWrapping = TextWrapping.WrapWholeWords
            },
            PrimaryButtonText = "是",
            SecondaryButtonText = "否",
            DefaultButton = ContentDialogButton.Primary
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
            ViewModel.LogoffSession(model);
    }

    private async void DisconnectBtn_Click(object sender, RoutedEventArgs e)
    {
        TsSessionModel model = (TsSessionModel)((Button)sender).DataContext;
        ContentDialog dialog = new()
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = $"断开来自 {model.FullAddress} 的连接吗？",
            XamlRoot = XamlRoot,
            Content = new TextBlock
            {
                Text =
                    $"断开来自 {model.FullAddress} (Id = {model.SessionId}) 的连接将会使得连接到该会话的客户端上的远程连接" +
                    $"与 Remote App 失去画面。" +
                    $"\n\n断开已登录用户与指定的远程桌面服务会话的连接，而不关闭会话。 " +
                    $"如果用户随后登录到同一远程桌面会话主机 (RD 会话主机) 服务器，则用户将重新连接到同一会话。",
                TextWrapping = TextWrapping.WrapWholeWords
            },
            PrimaryButtonText = "是",
            SecondaryButtonText = "否",
            DefaultButton = ContentDialogButton.Primary
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
            ViewModel.DisconnectSession(model);
    }
}
