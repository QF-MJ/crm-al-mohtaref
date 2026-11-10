namespace CRM.AlMohtaref.Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class AuthorizePermisionsAttribute : Attribute
{
    public string? Role { get; init; }
    public string? Department { get; init; }
}