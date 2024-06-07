namespace Any2Remote.Windows.Shared.Exceptions;

public class Any2RemoteException : Exception
{
    public Any2RemoteException()
    {

    }

    public Any2RemoteException(string message) : base(message)
    {

    }

    public Any2RemoteException(string message, Exception innerException) : base(message, innerException)
    {

    }
}
