namespace Any2Remote.Windows.AdminClient.Core.Models;

public enum ServerStatus
{
    // Windows Home Editions do not support RDP Server.
    NotSupported,

    // Local configuration is not correct.
    // Maybe the Registry key is not set correctly or missing.
    InternalError,

    NotInitialized,
    Disconnected,
    Connected
}
