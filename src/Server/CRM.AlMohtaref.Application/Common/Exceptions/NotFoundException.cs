namespace CRM.AlMohtaref.Application.Common.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(404, message)
    {
    }
}
