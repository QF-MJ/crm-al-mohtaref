namespace CRM.AlMohtaref.Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class AuthorizeAttribute : Attribute
{
    public string? Roles { get; init; }
}