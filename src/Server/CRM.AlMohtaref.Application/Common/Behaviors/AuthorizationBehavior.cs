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

        UserPermission[] parsedPermissions = userPermissions
            .Select(p => p.Split(':'))
            .Where(parts => parts.Length == 2)    
            .Select(parts => new UserPermission(parts[0], parts[1]))
            .ToArray();

        bool allowed = attributes.Any(attribute =>
           parsedPermissions.Any(p =>
               p.Role.Name.Equals(attribute.Role, StringComparison.OrdinalIgnoreCase) &&
               p.Department.Name.Equals(attribute.Department, StringComparison.OrdinalIgnoreCase)));

        if (!allowed)
            throw new ForbiddenException("ليس لديك صلاحية لتنفيذ هذا الإجراء");

        return await next();
    }
}