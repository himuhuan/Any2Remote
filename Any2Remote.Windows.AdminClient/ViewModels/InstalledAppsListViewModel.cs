using Any2Remote.Windows.AdminClient.Core.Contracts.Services;
using Any2Remote.Windows.AdminClient.Models;
using Any2Remote.Windows.Shared.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;
using System.Diagnostics;
using Any2Remote.Windows.Grpc.Services;
using System.Collections.ObjectModel;

namespace Any2Remote.Windows.AdminClient.ViewModels;

public partial class InstalledAppsListViewModel : ObservableRecipient
{
    private readonly ILocalService _localService;

    [ObservableProperty]
    private Visibility _loadingVisibility = Visibility.Visible;

    public InstalledAppsListViewModel(ILocalService localService)
    {
        _localService = localService;
    }


    public void UninstallLocalApp(LocalApplicationShowModel model)
    {
        try
        {
            using Process process = new();
            var uninstallCmd = WindowsCommon.ParseCommandLine(model.UninstallString);
            if (uninstallCmd == null)
                return;
            process.StartInfo.FileName = uninstallCmd.Program;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";
            process.StartInfo.Arguments = string.Join(" ", uninstallCmd.ArgumentList);
            process.Start();
            process.WaitForExit();
        }
        catch (Win32Exception)
        {
            // user canceled
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

}

