namespace CRM.AlMohtaref.Application.Common.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
