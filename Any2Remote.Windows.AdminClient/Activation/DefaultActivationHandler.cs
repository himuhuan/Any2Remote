using Any2Remote.Windows.AdminClient.Contracts.Services;
using Any2Remote.Windows.AdminClient.ViewModels;

using Microsoft.UI.Xaml;

namespace Any2Remote.Windows.AdminClient.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the ActivationHandlers has handled the activation.
        return _navigationService.Frame?.Content == null;
    }

    protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        string[] commandArguments = Environment.GetCommandLineArgs();
        if (commandArguments.Length != 2) 
            _navigationService.NavigateTo(typeof(MainViewModel).FullName!, args.Arguments);
        else
        {
            switch (commandArguments[1])
            {
                case "publish": 
                    _navigationService.NavigateTo(typeof(PublishRemoteAppViewModel).FullName!, args.Arguments);
                    break;
                case "remote-app":
                    _navigationService.NavigateTo(typeof(RemoteAppViewModel).FullName!, args.Arguments);
                    break;
                case "installed-app":
                    _navigationService.NavigateTo(typeof(InstalledAppsListViewModel).FullName!, args.Arguments);
                    break;
                default:
                    _navigationService.NavigateTo(typeof(MainViewModel).FullName!, args.Arguments);
                    break;
            }
        }
        await Task.CompletedTask;
    }
}
