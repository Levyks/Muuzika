using Muuzika.Server.Enums.Misc;

namespace Muuzika.Server.Exceptions;

public abstract class BaseException : Exception
{
    public abstract ExceptionType Type { get; }

    public new object? Data { get; }
    
    protected BaseException(object? data = null)
    {
        Data = data;
    }
}