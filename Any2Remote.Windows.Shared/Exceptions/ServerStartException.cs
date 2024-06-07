namespace Any2Remote.Windows.Shared.Exceptions
{
    public class ServerStartException : Any2RemoteException
    {
        public ServerStartException() : base("Failed to start server.")
        {
        }

        public ServerStartException(string message) : base(message)
        {
        }

        public ServerStartException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
