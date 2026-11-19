using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Domain.Entities.Users;
using FluentValidation.TestHelper;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Users;

public class PermissionCommandValidatorTests
{
    private readonly PermissionCommandValidator _sut = new();

    [Theory]
    [InlineData(Role.Employee)]
    [InlineData(Role.Manager)]
    public void DefinedRoles_Pass(Role role)
        => _sut.TestValidate(new PermissionCommand(role, Guid.NewGuid())).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void EmptyDepartmentId_Fails()
        => _sut.TestValidate(new PermissionCommand(Role.Employee, Guid.Empty))
               .ShouldHaveValidationErrorFor(x => x.DepartmentId);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(3)]
    [InlineData(99)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void UndefinedRole_Fails(int role)
        => _sut.TestValidate(new PermissionCommand((Role)role, Guid.NewGuid()))
               .ShouldHaveValidationErrorFor(x => x.Role);

    [Fact]
    public void BothInvalid_ReportsTwoErrors()
    {
        var result = _sut.TestValidate(new PermissionCommand((Role)0, Guid.Empty));
        Assert.Equal(2, result.Errors.Count);
    }
}