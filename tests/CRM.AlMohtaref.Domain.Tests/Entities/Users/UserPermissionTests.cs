using System;
using CRM.AlMohtaref.Domain.Entities.Users;
using Xunit;

namespace CRM.AlMohtaref.Domain.Tests.Entities.Users;

public class UserPermissionTests
{
    private const string ManagerLabel = "\u0645\u062F\u064A\u0631";   // مدير
    private const string EmployeeLabel = "\u0645\u0648\u0638\u0641";   // موظف
    private const string AdminDepartment = "\u0627\u0644\u0625\u062F\u0627\u0631\u0629";

    [Fact]
    public void Role_NumericValues_AreStable()
    {
        // These values are persisted in the database: changing them silently changes everyone's permissions.
        Assert.Equal(1, (int)Role.Employee);
        Assert.Equal(2, (int)Role.Manager);
    }

    [Fact]
    public void GuidConstructor_SetsFields()
    {
        var user = Guid.NewGuid();
        var dept = Guid.NewGuid();

        var permission = new UserPermission(user, dept, Role.Manager);

        Assert.Equal(user, permission.UserId);
        Assert.Equal(dept, permission.DepartmentId);
        Assert.Equal(Role.Manager, permission.Role);
    }

    [Fact]
    public void StringConstructor_Manager_MapsToManagerRole()
    {
        var permission = new UserPermission(AdminDepartment, ManagerLabel);

        Assert.Equal(Role.Manager, permission.Role);
        Assert.Equal(AdminDepartment, permission.Department.Name);
    }

    [Fact]
    public void StringConstructor_Employee_MapsToEmployeeRole()
    {
        var permission = new UserPermission(AdminDepartment, EmployeeLabel);

        Assert.Equal(Role.Employee, permission.Role);
    }

    [Fact]
    public void ParameterlessConstructor_UsedByEf_DefaultsToEmployee()
    {
        var permission = (UserPermission)Activator.CreateInstance(typeof(UserPermission), nonPublic: true)!;

        Assert.Equal(Role.Employee, permission.Role);
    }

    // ------------------------------------------------------------ BUG PROBES (expected to FAIL today)

    [Theory]
    [Trait("Category", "BugProbe")]
    [InlineData("manager")]
    [InlineData(ManagerLabel + " ")]   // trailing space
    [InlineData(" " + ManagerLabel)]
    [InlineData("")]
    [InlineData("garbage")]
    public void StringConstructor_UnknownRole_ShouldBeRejected_NotSilentlyBecomeEmployee(string role) =>
        Assert.ThrowsAny<ArgumentException>(() => new UserPermission(AdminDepartment, role));

    [Theory]
    [Trait("Category", "BugProbe")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void StringConstructor_BlankDepartment_ShouldBeRejected(string? department) =>
        Assert.ThrowsAny<ArgumentException>(() => new UserPermission(department!, ManagerLabel));

    [Fact]
    [Trait("Category", "BugProbe")]
    public void GuidConstructor_UndefinedRole_ShouldBeRejected() =>
        Assert.ThrowsAny<ArgumentException>(() => new UserPermission(Guid.NewGuid(), Guid.NewGuid(), (Role)99));

    [Fact]
    [Trait("Category", "BugProbe")]
    public void GuidConstructor_EmptyDepartmentId_ShouldBeRejected() =>
        Assert.ThrowsAny<ArgumentException>(() => new UserPermission(Guid.NewGuid(), Guid.Empty, Role.Employee));

    [Fact]
    [Trait("Category", "BugProbe")]
    public void GuidConstructor_EmptyUserId_ShouldBeRejected() =>
        Assert.ThrowsAny<ArgumentException>(() => new UserPermission(Guid.Empty, Guid.NewGuid(), Role.Employee));
}
