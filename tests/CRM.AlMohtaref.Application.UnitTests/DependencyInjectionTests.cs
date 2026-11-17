using CRM.AlMohtaref.Application.Common.Exceptions;
using CRM.AlMohtaref.Application.Features.Auth;
using CRM.AlMohtaref.Application.Features.Campaigns;
using CRM.AlMohtaref.Application.Features.Contacts.GetContacts;
using CRM.AlMohtaref.Application.Features.Lookups;
using CRM.AlMohtaref.Application.Features.Lookups.Dtos;
using CRM.AlMohtaref.Application.Features.Questions;
using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests;

public class DependencyInjectionTests : IDisposable
{
    private readonly Mock<ILookupRepository> _lookup = new();
    private readonly ServiceProvider _provider;

    public DependencyInjectionTests()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddSingleton<ICurrentUserService>(Mock.Of<ICurrentUserService>());
        services.AddSingleton(_lookup.Object);
        _provider = services.BuildServiceProvider();
    }

    public void Dispose() => _provider.Dispose();

    [Theory]
    [InlineData(typeof(GetContactsQuery))]
    [InlineData(typeof(GetUsersQuery))]
    [InlineData(typeof(GetUserQuery))]
    [InlineData(typeof(GetLookupsQuery))]
    [InlineData(typeof(CreateUserCommand))]
    [InlineData(typeof(UpdateUserCommand))]
    [InlineData(typeof(LoginCommand))]
    [InlineData(typeof(UpdateCampaignCommand))]
    [InlineData(typeof(UpdateQuestionCommand))]
    public void ValidatorIsRegistered_ForEveryRequest(Type requestType)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(requestType);

        Assert.NotNull(_provider.GetService(validatorType));
    }

    [Fact]
    public async Task Pipeline_RejectsInvalidRequest_WithValidationException()
    {
        var mediator = _provider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<ValidationException>(
            () => mediator.Send(new GetLookupsQuery(new List<LookupType>())));
    }

    [Fact]
    public async Task Pipeline_RejectsUndefinedEnum_WithValidationException()
    {
        var mediator = _provider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<ValidationException>(
            () => mediator.Send(new GetLookupsQuery(new List<LookupType> { (LookupType)99 })));
    }

    [Fact]
    public async Task Pipeline_ExecutesHandler_ForValidRequest()
    {
        _lookup.Setup(l => l.GetTagsAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(new[] { new TagDto(Guid.NewGuid(), "VIP") });
        var mediator = _provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new GetLookupsQuery(new List<LookupType> { LookupType.Tags }));

        Assert.Single(result.Tags!);
    }


    [Fact]
    public async Task Pipeline_UnauthenticatedCallToSecuredRequest_ThrowsUnauthorizedException()
    {
        var mediator = _provider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<UnauthorizedException>(() => mediator.Send(new GetContactsQuery()));
    }
}
