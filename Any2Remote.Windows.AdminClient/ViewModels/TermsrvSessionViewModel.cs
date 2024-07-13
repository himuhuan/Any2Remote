using Any2Remote.Windows.AdminClient.Contracts.ViewModels;
using Any2Remote.Windows.AdminClient.Core.Contracts.Services;
using Any2Remote.Windows.AdminClient.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;

namespace Any2Remote.Windows.AdminClient.ViewModels;

public partial class TermsrvSessionViewModel : ObservableRecipient, INavigationAware
{
    private readonly IRdpService _rdpServices;

    public TermsrvSessionViewModel(IRdpService rdpServices)
    {
        _rdpServices = rdpServices;
    }

    public                       ObservableCollection<TsSessionModel> Sessions { get; private set; } = new();
    [ObservableProperty] private Visibility                           _isDataLoading = Visibility.Visible;
    public Visibility NoSessionInHost => Sessions.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

    public void OnNavigatedTo(object parameter)
    {
        _isDataLoading = Visibility.Visible;
        LoadViewModelData();
        _isDataLoading = Visibility.Collapsed;
    }

    public void OnNavigatedFrom()
    {
    }

    private void LoadViewModelData()
    {
        Sessions.Clear();
        foreach (var session in _rdpServices.GetTermsrvSessions())
        {
            Sessions.Add(new TsSessionModel(session));
        }
    }

    public void LogoffSession(TsSessionModel model)
    {
        _rdpServices.LogoffSession(model);
        DispatcherQueue.GetForCurrentThread().TryEnqueue(() => Sessions.Remove(model));
    }

    public void DisconnectSession(TsSessionModel model)
    {
        _rdpServices.DisconnectSession(model);
        LoadViewModelData();
    }
}
