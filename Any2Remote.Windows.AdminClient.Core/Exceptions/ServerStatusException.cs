using Any2Remote.Windows.Shared.Models;

namespace Any2Remote.Windows.AdminClient.Core.Exceptions
{
    public class ServerStatusException : Any2RemoteException
    {
        public ServerStatus Status { get; init; }

        public ServerStatusException(ServerStatus status)
            : base($"Server status is {status}")
        {
            Status = status;
        }

        public ServerStatusException(ServerStatus status, string message)
            : base(message)
        {
            Status = status;
        }
    }
}
