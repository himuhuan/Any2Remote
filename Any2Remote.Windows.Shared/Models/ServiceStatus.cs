namespace Any2Remote.Windows.Shared.Models;

[Flags]
public enum ServiceStatus
{
    None = 0,
    NoRdpSupported = 1,
    NoEnhanceModeSupport = 2,
    NotInitializeServer = 4,
    InstalledEnhanceMode = 8,
    ServerRunning = 16,
    TermsrvRunning = 32,
    InternalError = 64,

    NotSupported = NoRdpSupported | NoEnhanceModeSupport,
    Running = ServerRunning | TermsrvRunning,
}
