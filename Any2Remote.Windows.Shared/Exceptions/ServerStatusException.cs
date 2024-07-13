using Any2Remote.Windows.Shared.Models;

namespace Any2Remote.Windows.Shared.Exceptions
{
    public class ServerStatusException : Any2RemoteException
    {
        public ServiceStatus Status { get; init; }

        public ServerStatusException(ServiceStatus status)
            : base($"Server status is {status}")
        {
            Status = status;
        }

        public ServerStatusException(ServiceStatus status, string message)
            : base(message)
        {
            Status = status;
        }
    }
}
