using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Application.Common.Behaviors;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Common;

public class AuthorizationBehaviorTests
{
    [AuthorizePermisions(Role = Role.Manager, Department = "Sales")]
    public sealed record SecuredRequest() : IRequest<string>;

    public sealed record OpenRequest() : IRequest<string>;

    private readonly Mock<ICurrentUserService> _user = new();
    private int _nextCalls;

    private Task<string> Next()
    {
        _nextCalls++;
        return Task.FromResult("ok");
    }

    [Fact]
    public async Task RequestWithoutAttributes_CallsNext_WithoutTouchingUser()
    {
        var behavior = new AuthorizationBehavior<OpenRequest, string>(_user.Object);

        var result = await behavior.Handle(new OpenRequest(), Next, CancellationToken.None);

        Assert.Equal("ok", result);
        Assert.Equal(1, _nextCalls);
        _user.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SecuredRequest_WhenNotAuthenticated_ThrowsUnauthorizedException()
    {
        _user.Setup(u => u.IsAuthenticated).Returns(false);
        var behavior = new AuthorizationBehavior<SecuredRequest, string>(_user.Object);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => behavior.Handle(new SecuredRequest(), Next, CancellationToken.None));

        Assert.Equal(0, _nextCalls);
    }

    [Fact]
    public async Task SecuredRequest_WhenPermissionsNull_ThrowsForbidden()
    {
        _user.Setup(u => u.IsAuthenticated).Returns(true);
        _user.Setup(u => u.Permissions).Returns((List<UserPermission>?)null);
        var behavior = new AuthorizationBehavior<SecuredRequest, string>(_user.Object);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => behavior.Handle(new SecuredRequest(), Next, CancellationToken.None));

        Assert.Equal(0, _nextCalls);
    }

    [Fact]
    public async Task SecuredRequest_WhenNoMatchingPermission_ThrowsForbidden()
    {
        _user.Setup(u => u.IsAuthenticated).Returns(true);
        _user.Setup(u => u.Permissions).Returns(new List<UserPermission>());
        var behavior = new AuthorizationBehavior<SecuredRequest, string>(_user.Object);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => behavior.Handle(new SecuredRequest(), Next, CancellationToken.None));

        Assert.Equal(0, _nextCalls);
    }
}
