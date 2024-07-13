using HimuRdp.Core;

namespace Any2Remote.Windows.AdminClient.Core.Models;

public class TsSessionModel : TermsrvSession
{
    public string StatusString
    {
        get
        {
            return Status switch
            {
                SessionConnectStatus.Active => "Active",
                SessionConnectStatus.Connected => "Connected",
                SessionConnectStatus.ConnectQuery => "ConnectQuery",
                SessionConnectStatus.Shadow => "Shadow",
                SessionConnectStatus.Disconnected => "Disconnected",
                SessionConnectStatus.Idle => "Idle",
                SessionConnectStatus.Listen => "Listen",
                SessionConnectStatus.Reset => "Reset",
                SessionConnectStatus.Down => "Down",
                SessionConnectStatus.Init => "Init",
                _ => "Unknown"
            };
        }
    }

    public string ConnectTimeString => ConnectTime.ToString("yyyy-MM-dd HH:mm:ss");

    public string FullUserName => string.IsNullOrEmpty(Domain) ? UserName : $"{Domain}\\{UserName}";

    public string FullAddress => string.IsNullOrEmpty(Address) ? "Unknown" : Address;

    public TsSessionModel(TermsrvSession session)
    {
        SessionId = session.SessionId;
        WinStationName = session.WinStationName;
        Status = session.Status;
        UserName = session.UserName;
        Domain = session.Domain;
        Address = session.Address;
        ConnectTime = session.ConnectTime;
    }
}
