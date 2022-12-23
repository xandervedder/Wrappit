namespace Wrappit.Exceptions;

public class WrappitException : Exception
{
    public WrappitException(string message) : base(message)
    {
    }

    public WrappitException(string message, Exception exception) : base(message, exception)
    {
    }
}
