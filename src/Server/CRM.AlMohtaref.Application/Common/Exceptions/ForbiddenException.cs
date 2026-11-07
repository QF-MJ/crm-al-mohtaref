namespace CRM.AlMohtaref.Application.Common.Exceptions
{
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message) : base(403, message)
        {
        }
    }
}
