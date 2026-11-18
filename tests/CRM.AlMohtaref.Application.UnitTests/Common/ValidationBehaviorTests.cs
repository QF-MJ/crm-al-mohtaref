using CRM.AlMohtaref.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Common;

public class ValidationBehaviorTests
{
    public sealed record Req(string Name) : IRequest<string>;

    private sealed class NameNotEmpty : AbstractValidator<Req>
    {
        public NameNotEmpty() => RuleFor(x => x.Name).NotEmpty();
    }

    private sealed class NameMinLength : AbstractValidator<Req>
    {
        public NameMinLength() => RuleFor(x => x.Name).MinimumLength(5);
    }

    private int _nextCalls;

    private Task<string> Next()
    {
        _nextCalls++;
        return Task.FromResult("ok");
    }

    [Fact]
    public async Task NoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<Req, string>(Array.Empty<IValidator<Req>>());

        var result = await behavior.Handle(new Req(""), Next, CancellationToken.None);

        Assert.Equal("ok", result);
        Assert.Equal(1, _nextCalls);
    }

    [Fact]
    public async Task ValidRequest_CallsNext()
    {
        var behavior = new ValidationBehavior<Req, string>(new IValidator<Req>[] { new NameNotEmpty() });

        var result = await behavior.Handle(new Req("abc"), Next, CancellationToken.None);

        Assert.Equal("ok", result);
        Assert.Equal(1, _nextCalls);
    }

    [Fact]
    public async Task InvalidRequest_ThrowsValidationException_AndDoesNotCallNext()
    {
        var behavior = new ValidationBehavior<Req, string>(new IValidator<Req>[] { new NameNotEmpty() });

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(new Req(""), Next, CancellationToken.None));

        Assert.Equal(0, _nextCalls);
    }

    [Fact]
    public async Task FailuresFromAllValidators_AreAggregated()
    {
        var behavior = new ValidationBehavior<Req, string>(
            new IValidator<Req>[] { new NameNotEmpty(), new NameMinLength() });

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(new Req(""), Next, CancellationToken.None));

        Assert.Equal(2, ex.Errors.Count());
    }
}
