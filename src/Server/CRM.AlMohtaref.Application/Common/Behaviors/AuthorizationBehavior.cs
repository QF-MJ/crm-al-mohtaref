using System.Reflection;
using CRM.AlMohtaref.Application.Common.Attributes;
using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Interfaces;
using MediatR;

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
            .GetCustomAttributes<AuthorizeAttribute>(inherit: true)
            .ToList();

        if (attributes.Count == 0)
            return await next();

        //if (!_currentUser.IsAuthenticated)
        //    throw new UnauthorizedAccessException();

        var userRoles = _currentUser.Roles;

        foreach (var attribute in attributes.Where(a => !string.IsNullOrWhiteSpace(a.Roles)))
        {
            var required = attribute.Roles!
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var authorized = required.Any(role =>
                userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));

            if (!authorized)
                throw new ForbiddenAccessException();
        }

        return await next();
    }
}