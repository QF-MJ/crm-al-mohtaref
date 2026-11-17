using FluentValidation.Results;

namespace CRM.AlMohtaref.Application.UnitTests;

internal static class ValidationResultExtensions
{
    public static bool HasErrorFor(this ValidationResult result, string property) =>
        result.Errors.Any(e =>
            e.PropertyName == property ||
            e.PropertyName.StartsWith(property + "[") ||
            e.PropertyName.StartsWith(property + "."));
}

internal static class TestConstants
{
    public const string AdminDepartment = "\u0627\u0644\u0625\u062F\u0627\u0631\u0629";
}
