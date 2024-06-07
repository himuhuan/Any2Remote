using Any2Remote.Windows.AdminClient.Helpers;
using Any2Remote.Windows.AdminClient.Models;
using Any2Remote.Windows.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Windows.Storage;

namespace Any2Remote.Windows.AdminClient.ViewModels;

public partial class EditRemoteAppViewModel : ObservableRecipient
{
    [ObservableProperty]
    private RemoteApplicationListModel _remoteApplication = default!;

    [ObservableProperty]
    private Visibility _loadingVisibility = Visibility.Visible;

    public async void PublishRemoteApp()
    {
        string userDataPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Any2Remote");
        if (!Directory.Exists(userDataPath))
            Directory.CreateDirectory(userDataPath);
        StorageFolder userDataFolder =
            await StorageFolder.GetFolderFromPathAsync(userDataPath);
        StorageFolder applicationFolder =
            await userDataFolder.CreateFolderAsync("RemoteApplications", CreationCollisionOption.OpenIfExists);
        StorageFolder folder =
            await applicationFolder.CreateFolderAsync($"{RemoteApplication.AppId}", CreationCollisionOption.OpenIfExists);
        StorageFile file = await folder.CreateFileAsync("icon.png", CreationCollisionOption.ReplaceExisting);
        await ModelInfoConverter.SaveAppIcon(RemoteApplication.Path, file);
        RemoteApplication.AppIconUrl = file.Path;
        var client = (Application.Current as App)!.CoreServerClient;
        await client.PublishRemoteApplicationAsync(RemoteApplication);
    }

}
