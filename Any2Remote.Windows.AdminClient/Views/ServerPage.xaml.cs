using Any2Remote.Windows.AdminClient.ViewModels;

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
}
