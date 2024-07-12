using Any2Remote.Windows.Shared.Models;

namespace Any2Remote.Windows.AdminClient.Core.Contracts.Services;

public interface IRdpService
{
    public ServiceStatusInfo GetServiceStatus();
}