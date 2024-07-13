using System.ComponentModel;
using System.Runtime.InteropServices;

namespace HimuRdp.Core;

/// <summary>
/// The status of the termsrv session from the WTS_CONNECTSTATE_CLASS enumeration
/// </summary>
public enum SessionConnectStatus
{
    Active,
    Connected,
    ConnectQuery,
    Shadow,
    Disconnected,
    Idle,
    Listen,
    Reset,
    Down,
    Init
}

/// <summary>
/// The wrapper class for the termsrv session.
/// </summary>
public class TermsrvSession
{
    #region Properties
    public long                 SessionId      { get; set; }
    public string               WinStationName { get; set; } = string.Empty;
    public SessionConnectStatus Status         { get; set; }
    public string               UserName       { get; set; } = string.Empty;
    public string               Domain         { get; set; } = string.Empty;
    public string               Address        { get; set; } = string.Empty;
    public DateTime             ConnectTime    { get; set; }
    #endregion
}
