﻿using Any2Remote.Windows.AdminClient.ViewModels;
using Any2Remote.Windows.Shared.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
}
