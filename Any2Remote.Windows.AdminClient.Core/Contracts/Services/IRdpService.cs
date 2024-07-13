using Any2Remote.Windows.Shared.Models;
using HimuRdp.Core;

namespace Any2Remote.Windows.AdminClient.Core.Contracts.Services;

public interface IRdpService
{
    public ServiceStatusInfo GetServiceStatus();

    public List<TermsrvSession> GetTermsrvSessions();

    public void LogoffSession(TermsrvSession session);

    public void DisconnectSession(TermsrvSession session);
}