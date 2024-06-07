namespace Any2Remote.Windows.AdminClient.Core.Exceptions;

public class UnauthorizedException : Any2RemoteException
{
    public UnauthorizedException()
    {

    }

    public UnauthorizedException(string message) : base(message)
    {

    }

    public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
    {

    }
}
