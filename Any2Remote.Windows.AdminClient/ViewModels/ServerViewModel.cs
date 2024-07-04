using Any2Remote.Windows.AdminClient.Core.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Any2Remote.Windows.AdminClient.ViewModels;

public partial class ServerViewModel : ObservableRecipient
{
    private readonly ILocalService _service;
    public ServerViewModel(ILocalService service)
    {
        _service = service;
    }
    
    public void ResetApplication()
    {
        _service.ResetApplication();
    }
}
