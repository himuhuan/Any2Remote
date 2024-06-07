using Any2Remote.Windows.AdminClient.Core.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Any2Remote.Windows.AdminClient.ViewModels;

public partial class PublishRemoteAppViewModel : ObservableRecipient
{
    private readonly ILocalService _localServices;

    public PublishRemoteAppViewModel(ILocalService localServices)
    {
        _localServices = localServices;
    }

}
