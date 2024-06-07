using System.Collections.ObjectModel;
using Any2Remote.Windows.AdminClient.Core.Contracts.Services;
using Any2Remote.Windows.AdminClient.Models;
using Any2Remote.Windows.AdminClient.ViewModels;
using Any2Remote.Windows.Grpc.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Any2Remote.Windows.AdminClient.Views;

public sealed partial class InstalledAppsListPage : Page
{
    public InstalledAppsListViewModel ViewModel
    {
        get;
    }

    private readonly ILocalService _localService;

    public InstalledAppsListPage()
    {
        _localService = App.GetService<ILocalService>();
        ViewModel = App.GetService<InstalledAppsListViewModel>();
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        await Task.Run(() =>
        {
            var localApplications = GetLocalApplication();
            DispatcherQueue.TryEnqueue(() =>
            {
                ObservableCollection<LocalApplicationShowModel> models = new();
                foreach (LocalApp localApp in localApplications)
                {
                    models.Add(new LocalApplicationShowModel(localApp));
                }
                LocalApplicationShowList.ItemsSource = models;
                ApplicationCount.Text = localApplications.Count.ToString();
                ViewModel.LoadingVisibility = Visibility.Collapsed;
            });
        });
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        Bindings.StopTracking(); 
        LocalApplicationShowList.ItemsSource = null;
        GC.Collect();
    }

    private async void LocalAppUninstall_Click(object sender, RoutedEventArgs e)
    {
        LocalApplicationShowModel model = (LocalApplicationShowModel) ((MenuFlyoutItem) sender).DataContext;
        ContentDialog dialog = new()
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = $"卸载 {model.DisplayName}",
            XamlRoot = XamlRoot,
            Content = new TextBlock
            {
                Text = $"您确定要卸载 {model.DisplayName} 吗?\n以下命令将会以管理员特权执行\n\n{model.UninstallString}\n\n此操作不可撤回！",
                TextWrapping = TextWrapping.WrapWholeWords
            },
            PrimaryButtonText = "好的",
            SecondaryButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
            ViewModel.UninstallLocalApp(model);
    }

    private List<LocalApp> GetLocalApplication()
    {
        var applications = _localService.GetInstalledApplications(
                                            new Grpc.Services.LocalAppsRequest
                                            {
                                                IncludeSystemComponent = false,
                                            })
                                        .ToList();

        applications.Sort((a, b) => string.Compare(
            a.DisplayName,
            b.DisplayName,
            StringComparison.InvariantCultureIgnoreCase));

        return applications;
    }

    private void LocalAppPublish_OnClick(object sender, RoutedEventArgs e)
    {
        if (((MenuFlyoutItem) sender).DataContext is LocalApplicationShowModel model)
        {
            Frame.Navigate(typeof(EditRemoteAppPage), model);
        }
    }
}
