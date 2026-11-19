using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class AuthorizePermisionsAttribute : Attribute
{
    public Role Role { get; init; }

    public string Department { get; init; }
}