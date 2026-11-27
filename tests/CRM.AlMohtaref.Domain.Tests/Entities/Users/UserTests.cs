using System;
using System.Collections.Generic;
using System.Linq;
using CRM.AlMohtaref.Domain.Entities;
using CRM.AlMohtaref.Domain.Entities.Users;
using Xunit;

namespace CRM.AlMohtaref.Domain.Tests.Entities.Users;


public class UserTests
{
    private static readonly Guid Dept1 = Guid.NewGuid();
    private static readonly Guid Dept2 = Guid.NewGuid();

    private static User NewUser() =>
        new(Guid.NewGuid(), "acc", "hash", new Person("Full Name", Guid.NewGuid()), "a@b.com");

    private static (Role, Guid)[] P(params (Role, Guid)[] items) => items;

    // ------------------------------------------------------------ construction

    [Fact]
    public void Constructor_SetsInitialState()
    {
        var creator = Guid.NewGuid();
        var id = Guid.NewGuid();

        var user = new User(id, "acc", "hash", new Person("Full Name", creator), "a@b.com");

        Assert.Equal(id, user.Id);
        Assert.Equal("acc", user.AccountName);
        Assert.Equal("hash", user.PasswordHash);
        Assert.Equal("a@b.com", user.Email);
        Assert.Equal("Full Name", user.FullName);
        Assert.Equal(creator, user.CreatedByUserId);
        Assert.True(user.IsActive);
        Assert.Equal(0, user.FailedLoginCount);
        Assert.Null(user.LockoutEnd);
        Assert.NotNull(user.UserPermissions);
        Assert.Empty(user.UserPermissions);
        Assert.NotNull(user.userRefreshTokens);
        Assert.Empty(user.userRefreshTokens);
    }

    [Fact]
    public void Constructor_WithNullFullName_KeepsNull()
    {
        var user = new User(Guid.NewGuid(), "acc", "hash", new Person(null, null), "a@b.com");

        Assert.Null(user.FullName);
        Assert.Null(user.CreatedByUserId);
    }

    [Fact]
    public void ParameterlessConstructor_UsedByEf_InitializesNonNullMembers()
    {
        var user = (User)Activator.CreateInstance(typeof(User), nonPublic: true)!;

        Assert.Equal(string.Empty, user.AccountName);
        Assert.Equal(string.Empty, user.PasswordHash);
        Assert.Equal(string.Empty, user.Email);
        Assert.NotNull(user.UserPermissions);
        Assert.NotNull(user.userRefreshTokens);
    }

    // ------------------------------------------------------------ failed login / lockout

    [Fact]
    public void RegisterFailedLogin_IncrementsTheCounter()
    {
        var user = NewUser();

        user.RegisterFailedLogin();
        user.RegisterFailedLogin();
        user.RegisterFailedLogin();

        Assert.Equal(3, user.FailedLoginCount);
    }

    [Fact]
    public void RegisterFailedLogin_DoesNotLockByItself()
    {
        var user = NewUser();

        for (var i = 0; i < 20; i++) user.RegisterFailedLogin();

        Assert.Null(user.LockoutEnd);
    }

    [Fact]
    public void LockAccount_SetsLockoutEnd_AndCanBeOverwritten()
    {
        var user = NewUser();
        var first = DateTime.UtcNow.AddHours(1);
        var second = DateTime.UtcNow.AddHours(5);

        user.LockAccount(first);
        Assert.Equal(first, user.LockoutEnd);

        user.LockAccount(second);
        Assert.Equal(second, user.LockoutEnd);
    }

    [Fact]
    public void ResetFailedLogin_ClearsCounterAndLock()
    {
        var user = NewUser();
        user.RegisterFailedLogin();
        user.RegisterFailedLogin();
        user.LockAccount(DateTime.UtcNow.AddDays(1));

        user.ResetFailedLogin();

        Assert.Equal(0, user.FailedLoginCount);
        Assert.Null(user.LockoutEnd);
    }

    [Fact]
    public void ResetFailedLogin_OnCleanUser_IsHarmless()
    {
        var user = NewUser();

        user.ResetFailedLogin();

        Assert.Equal(0, user.FailedLoginCount);
        Assert.Null(user.LockoutEnd);
    }

    // ------------------------------------------------------------ password / profile

    [Fact]
    public void ChangePassword_ReplacesTheHash()
    {
        var user = NewUser();

        user.ChangePassword("new-hash");

        Assert.Equal("new-hash", user.PasswordHash);
    }

    [Fact]
    public void UpdateProfile_ChangesFields()
    {
        var user = NewUser();

        user.UpdateProfile("new.acc", "new@x.com", true, "New Name");

        Assert.Equal("new.acc", user.AccountName);
        Assert.Equal("new@x.com", user.Email);
        Assert.Equal("New Name", user.FullName);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void UpdateProfile_IsActive_NullKeepsCurrent(bool? requested, bool expected)
    {
        var user = NewUser(); // active

        user.UpdateProfile("acc", "a@b.com", requested, "Full Name");

        Assert.Equal(expected, user.IsActive);
    }

    [Fact]
    public void UpdateProfile_NullKeepsInactiveUserInactive()
    {
        var user = NewUser();
        user.UpdateProfile("acc", "a@b.com", false, "Full Name");

        user.UpdateProfile("acc", "a@b.com", null, "Full Name");

        Assert.False(user.IsActive);
    }

    [Fact]
    public void UpdateProfile_DoesNotTouchPasswordIdLockoutOrPermissions()
    {
        var user = NewUser();
        var id = user.Id;
        user.SynchronizePermissions(P((Role.Manager, Dept1)));
        user.RegisterFailedLogin();
        user.LockAccount(DateTime.UtcNow.AddHours(1));
        var lockEnd = user.LockoutEnd;

        user.UpdateProfile("x", "x@x.com", true, "X");

        Assert.Equal(id, user.Id);
        Assert.Equal("hash", user.PasswordHash);
        Assert.Equal(1, user.FailedLoginCount);
        Assert.Equal(lockEnd, user.LockoutEnd);
        Assert.Single(user.UserPermissions);
    }

    // ------------------------------------------------------------ HasPermissionsChanged

    [Fact]
    public void HasPermissionsChanged_EmptyVsEmpty_IsFalse() =>
        Assert.False(NewUser().HasPermissionsChanged(P()));

    [Fact]
    public void HasPermissionsChanged_SameSet_IsFalse()
    {
        var user = NewUser();
        user.SynchronizePermissions(P((Role.Employee, Dept1), (Role.Manager, Dept2)));

        Assert.False(user.HasPermissionsChanged(P((Role.Employee, Dept1), (Role.Manager, Dept2))));
    }

    [Fact]
    public void HasPermissionsChanged_DifferentOrder_IsFalse()
    {
        var user = NewUser();
        user.SynchronizePermissions(P((Role.Employee, Dept1), (Role.Manager, Dept2)));

        Assert.False(user.HasPermissionsChanged(P((Role.Manager, Dept2), (Role.Employee, Dept1))));
    }

    [Fact]
    public void HasPermissionsChanged_DuplicatesInRequest_AreIgnored()
    {
        var user = NewUser();
        user.SynchronizePermissions(P((Role.Employee, Dept1)));

        Assert.False(user.HasPermissionsChanged(P((Role.Employee, Dept1), (Role.Employee, Dept1))));
    }

    [Fact]
    public void HasPermissionsChanged_Added_IsTrue()
    {
        var user = NewUser();
        user.SynchronizePermissions(P((Role.Employee, Dept1)));

        Assert.True(user.HasPermissionsChanged(P((Role.Employee, Dept1), (Role.Employee, Dept2))));
    }

    [Fact]
    public void HasPermissionsChanged_Removed_IsTrue()
    {
        var user = NewUser();
        user.SynchronizePermissions(P((Role.Employee, Dept1), (Role.Employee, Dept2)));

        Assert.True(user.HasPermissionsChanged(P((Role.Employee, Dept1))));
    }

    [Fact]
    public void HasPermissionsChanged_RoleChanged_IsTrue()
    {
        var user = NewUser();
        user.SynchronizePermissions(P((Role.Employee, Dept1)));

        Assert.True(user.HasPermissionsChanged(P((Role.Manager, Dept1))));
    }

    [Fact]
    public void HasPermissionsChanged_DepartmentChanged_IsTrue()
    {
        var user = NewUser();
        user.SynchronizePermissions(P((Role.Employee, Dept1)));

        Assert.True(user.HasPermissionsChanged(P((Role.Employee, Dept2))));
    }

    [Fact]
    public void HasPermissionsChanged_EmptyRequest_OnUserWithPermissions_IsTrue()
    {
        var user = NewUser();
        user.SynchronizePermissions(P((Role.Employee, Dept1)));

        Assert.True(user.HasPermissionsChanged(P()));
    }

    [Fact]
    public void HasPermissionsChanged_Null_ThrowsArgumentNull() =>
        Assert.Throws<ArgumentNullException>(() => NewUser().HasPermissionsChanged(null!));

    // ------------------------------------------------------------ SynchronizePermissions

    [Fact]
    public void Synchronize_AddsEveryRequestedPermission_WithTheUsersId()
    {
        var user = NewUser();

        user.SynchronizePermissions(P((Role.Employee, Dept1), (Role.Manager, Dept2)));

        Assert.Equal(2, user.UserPermissions.Count);
        Assert.All(user.UserPermissions, p => Assert.Equal(user.Id, p.UserId));
        Assert.Contains(user.UserPermissions, p => p.DepartmentId == Dept1 && p.Role == Role.Employee);
        Assert.Contains(user.UserPermissions, p => p.DepartmentId == Dept2 && p.Role == Role.Manager);
    }

    [Fact]
    public void Synchronize_RemovesPermissionsThatAreNotRequested()
    {
        var user = NewUser();
        user.SynchronizePermissions(P((Role.Employee, Dept1), (Role.Manager, Dept2)));

        user.SynchronizePermissions(P((Role.Manager, Dept2)));

        var remaining = Assert.Single(user.UserPermissions);
        Assert.Equal(Dept2, remaining.DepartmentId);
    }

    [Fact]
    public void Synchronize_KeepsExistingInstances_ForUnchangedPermissions()
    {
        var user = NewUser();
        user.SynchronizePermissions(P((Role.Employee, Dept1)));
        var original = user.UserPermissions.Single();

        user.SynchronizePermissions(P((Role.Employee, Dept1), (Role.Manager, Dept2)));

        Assert.Contains(user.UserPermissions, p => ReferenceEquals(p, original));
        Assert.Equal(2, user.UserPermissions.Count);
    }

    [Fact]
    public void Synchronize_IsIdempotent()
    {
        var user = NewUser();
        var request = P((Role.Employee, Dept1), (Role.Manager, Dept2));

        user.SynchronizePermissions(request);
        user.SynchronizePermissions(request);
        user.SynchronizePermissions(request);

        Assert.Equal(2, user.UserPermissions.Count);
        Assert.False(user.HasPermissionsChanged(request));
    }

    [Fact]
    public void Synchronize_Duplicates_AreStoredOnce()
    {
        var user = NewUser();

        user.SynchronizePermissions(P((Role.Manager, Dept1), (Role.Manager, Dept1), (Role.Manager, Dept1)));

        Assert.Single(user.UserPermissions);
    }

    [Fact]
    public void Synchronize_SameDepartment_DifferentRoles_StoresBoth()
    {
        var user = NewUser();

        user.SynchronizePermissions(P((Role.Manager, Dept1), (Role.Employee, Dept1)));

        Assert.Equal(2, user.UserPermissions.Count);
    }

    [Fact]
    public void Synchronize_RoleChangeInSameDepartment_ReplacesThePermission()
    {
        var user = NewUser();
        user.SynchronizePermissions(P((Role.Employee, Dept1)));

        user.SynchronizePermissions(P((Role.Manager, Dept1)));

        var only = Assert.Single(user.UserPermissions);
        Assert.Equal(Role.Manager, only.Role);
    }

    [Fact]
    public void Synchronize_EmptyRequest_ClearsEverything()
    {
        var user = NewUser();
        user.SynchronizePermissions(P((Role.Employee, Dept1), (Role.Manager, Dept2)));

        user.SynchronizePermissions(P());

        Assert.Empty(user.UserPermissions);
    }

    [Fact]
    public void Synchronize_Null_ThrowsArgumentNull() =>
        Assert.Throws<ArgumentNullException>(() => NewUser().SynchronizePermissions(null!));

    // ------------------------------------------------------------ BUG PROBES (expected to FAIL today)

    [Fact]
    [Trait("Category", "BugProbe")]
    public void Constructor_NullPerson_ShouldThrowArgumentNull_NotNullReference() =>
        Assert.Throws<ArgumentNullException>(
            () => new User(Guid.NewGuid(), "acc", "hash", null!, "a@b.com"));

    [Theory]
    [Trait("Category", "BugProbe")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankAccountName_ShouldBeRejected(string? accountName) =>
        Assert.ThrowsAny<ArgumentException>(
            () => new User(Guid.NewGuid(), accountName!, "hash", new Person("n", null), "a@b.com"));

    [Theory]
    [Trait("Category", "BugProbe")]
    [InlineData(null)]
    [InlineData("")]
    public void Constructor_BlankPasswordHash_ShouldBeRejected(string? hash) =>
        Assert.ThrowsAny<ArgumentException>(
            () => new User(Guid.NewGuid(), "acc", hash!, new Person("n", null), "a@b.com"));

    [Fact]
    [Trait("Category", "BugProbe")]
    public void Constructor_EmptyGuidId_ShouldBeRejected() =>
        Assert.ThrowsAny<ArgumentException>(
            () => new User(Guid.Empty, "acc", "hash", new Person("n", null), "a@b.com"));

    [Theory]
    [Trait("Category", "BugProbe")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangePassword_BlankHash_ShouldBeRejected(string? hash) =>
        Assert.ThrowsAny<ArgumentException>(() => NewUser().ChangePassword(hash!));

    [Theory]
    [Trait("Category", "BugProbe")]
    [InlineData(null, "a@b.com")]
    [InlineData("", "a@b.com")]
    [InlineData("acc", null)]
    [InlineData("acc", "")]
    public void UpdateProfile_BlankAccountOrEmail_ShouldBeRejected(string? account, string? email) =>
        Assert.ThrowsAny<ArgumentException>(() => NewUser().UpdateProfile(account!, email!, true, "Name"));

    [Fact]
    [Trait("Category", "BugProbe")]
    public void UserPermissions_ShouldNotHaveAPublicSetter()
    {
        // Anyone can replace (or null out) the collection, bypassing SynchronizePermissions.
        var setter = typeof(User).GetProperty(nameof(User.UserPermissions))!.SetMethod;

        Assert.True(setter is null || !setter.IsPublic, "User.UserPermissions has a public setter");
    }

    [Fact]
    [Trait("Category", "BugProbe")]
    public void Synchronize_UndefinedRole_ShouldBeRejected() =>
        Assert.ThrowsAny<ArgumentException>(
            () => NewUser().SynchronizePermissions(P(((Role)99, Dept1))));

    [Fact]
    [Trait("Category", "BugProbe")]
    public void Synchronize_EmptyDepartmentId_ShouldBeRejected() =>
        Assert.ThrowsAny<ArgumentException>(
            () => NewUser().SynchronizePermissions(P((Role.Employee, Guid.Empty))));

    [Fact]
    [Trait("Category", "BugProbe")]
    public void Synchronize_InvalidInput_ShouldNotLeaveTheUserPartiallyUpdated()
    {
        var user = NewUser();
        user.SynchronizePermissions(P((Role.Employee, Dept1)));

        Record.Exception(() => user.SynchronizePermissions(P(((Role)99, Dept2))));

        // existing permission must survive a rejected update
        var kept = Assert.Single(user.UserPermissions);
        Assert.Equal(Dept1, kept.DepartmentId);
    }
}
