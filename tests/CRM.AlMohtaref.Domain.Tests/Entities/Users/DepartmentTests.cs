using System;
using CRM.AlMohtaref.Domain.Entities.Users;
using Xunit;

namespace CRM.AlMohtaref.Domain.Tests.Entities.Users;

public class DepartmentTests
{
    [Fact]
    public void Constructor_SetsName_AndInitializesPermissions()
    {
        var department = new Department("Sales");

        Assert.Equal("Sales", department.Name);
        Assert.NotNull(department.UserPermissions);
        Assert.Empty(department.UserPermissions);
    }

    [Fact]
    public void ParameterlessConstructor_UsedByEf_InitializesNonNullMembers()
    {
        var department = (Department)Activator.CreateInstance(typeof(Department), nonPublic: true)!;

        Assert.Equal(string.Empty, department.Name);
        Assert.NotNull(department.UserPermissions);
    }

    [Fact]
    public void TwoDepartments_DoNotShareTheirPermissionCollection()
    {
        var a = new Department("A");
        var b = new Department("B");

        a.UserPermissions.Add(new UserPermission(Guid.NewGuid(), a.Id, Role.Employee));

        Assert.Empty(b.UserPermissions);
    }

    [Theory]
    [Trait("Category", "BugProbe")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankName_ShouldBeRejected(string? name) =>
        Assert.ThrowsAny<ArgumentException>(() => new Department(name!));
}
