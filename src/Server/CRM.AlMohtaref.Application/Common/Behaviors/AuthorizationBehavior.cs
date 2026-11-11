using System;
using System.Reflection;
using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using CRM.AlMohtaref.Domain.Entities.Users;
using MediatR;
using System.Linq;

namespace CRM.AlMohtaref.Application.Common.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUser;

    public AuthorizationBehavior(ICurrentUserService currentUser)
        => _currentUser = currentUser;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var attributes = request.GetType()
            .GetCustomAttributes<AuthorizePermisionsAttribute>(inherit: true)
            .ToList();

        if (attributes.Count == 0)
            return await next();

        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var userPermissions = _currentUser.Permissions;

        if(userPermissions == null)
            throw new ForbiddenException("ليس لديك صلاحية لتنفيذ هذا الإجراء");

        bool allowed = attributes.Any(attribute =>
           userPermissions.Any(p =>
               p.Role.Name.Equals(attribute.Role, StringComparison.OrdinalIgnoreCase) &&
               p.Department.Name.Equals(attribute.Department, StringComparison.OrdinalIgnoreCase)));

        if (!allowed)
            throw new ForbiddenException("ليس لديك صلاحية لتنفيذ هذا الإجراء");

        return await next();
    }
}