using Any2Remote.Windows.AdminClient.Models;
using Any2Remote.Windows.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.UI.Xaml;

namespace Any2Remote.Windows.AdminClient.ViewModels;

public partial class RemoteAppViewModel : ObservableRecipient
{
    private List<RemoteApplicationListModel> _remoteApps = default!;
    public List<RemoteApplicationListModel> RemoteApps
    {
        get => _remoteApps;
        set
        {
            SetProperty(ref _remoteApps, value);
            OnPropertyChanged(nameof(HasRemoteApps));
            OnPropertyChanged(nameof(RemoteAppTipsVisibility));
        }
    }

    public bool HasRemoteApps => RemoteApps.Count > 0;

    public Visibility RemoteAppTipsVisibility => RemoteApps.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

    public RemoteAppViewModel()
    {
        RemoteApps = new List<RemoteApplicationListModel>();
    }

    public void RefreshRemoteApps(List<RemoteApplication> remoteApps)
    {
        RemoteApps = remoteApps.Select(app => new RemoteApplicationListModel(app))
            .ToList();
    }

    public async Task RemoveRemoteApp(RemoteApplication remoteApp)
    {
        var client = (Application.Current as App)!.CoreServerClient;
        await client.RemoveRemoteApplicationAsync(remoteApp);
    }
}
