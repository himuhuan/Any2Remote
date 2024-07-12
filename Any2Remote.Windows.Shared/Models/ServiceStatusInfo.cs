namespace Any2Remote.Windows.Shared.Models;

public class ServiceStatusInfo
{
    public ServiceStatus Status  { get; set; } = ServiceStatus.None;
    public string        Message { get; set; } = string.Empty;

    public bool CanStartService => !Status.HasFlag(ServiceStatus.NotInitializeServer)
                                   && (!Status.HasFlag(ServiceStatus.NotInitializeServer)
                                       || Status.HasFlag(ServiceStatus.InstalledEnhanceMode))
                                   && !RunningService;

    public bool RequireEnhanceMode => Status.HasFlag(ServiceStatus.NoRdpSupported)
                                      && !Status.HasFlag(ServiceStatus.NoEnhanceModeSupport);

    public bool TermsrvOnly => Status.HasFlag(ServiceStatus.TermsrvRunning)
                               && !Status.HasFlag(ServiceStatus.ServerRunning);

    public bool ServerOnly => Status.HasFlag(ServiceStatus.ServerRunning)
                              && !Status.HasFlag(ServiceStatus.TermsrvRunning);

    public bool FullSupport => Status.HasFlag(ServiceStatus.Running)
                               && Status.HasFlag(ServiceStatus.InstalledEnhanceMode);

    public bool RunningService => Status.HasFlag(ServiceStatus.Running);

    public bool NotInitializeServer => Status.HasFlag(ServiceStatus.NotInitializeServer);

    public bool NotSupported => Status.HasFlag(ServiceStatus.NotSupported);
}
