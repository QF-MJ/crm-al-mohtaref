using CRM.AlMohtaref.Application.Features.Users;
using CRM.AlMohtaref.Domain.Entities.Users;
using FluentValidation.TestHelper;
using Xunit;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Users;

public class GetUsersQueryValidatorTests
{
    private readonly GetUsersQueryValidator _sut = new();

    [Fact]
    public void DefaultQuery_Passes()
        => _sut.TestValidate(new GetUsersQuery()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void PageNumber_NonPositive_Fails(int page)
        => _sut.TestValidate(new GetUsersQuery(PageNumber: page)).ShouldHaveValidationErrorFor(x => x.PageNumber);

    [Fact]
    public void PageNumber_One_Passes()
        => _sut.TestValidate(new GetUsersQuery(PageNumber: 1)).ShouldNotHaveValidationErrorFor(x => x.PageNumber);

    [Theory]
    [InlineData(200, false)]
    [InlineData(201, true)]
    public void Name_Length(int length, bool shouldFail)
        => Check(new GetUsersQuery(Name: new string('a', length)), x => x.Name, shouldFail);

    [Theory]
    [InlineData(100, false)]
    [InlineData(101, true)]
    public void AccountName_Length(int length, bool shouldFail)
        => Check(new GetUsersQuery(AccountName: new string('a', length)), x => x.AccountName, shouldFail);

    [Theory]
    [InlineData(256, false)]
    [InlineData(257, true)]
    public void Email_Length(int length, bool shouldFail)
        => Check(new GetUsersQuery(Email: new string('a', length)), x => x.Email, shouldFail);

    [Fact]
    public void Role_Null_Passes()
        => _sut.TestValidate(new GetUsersQuery(Role: null)).ShouldNotHaveValidationErrorFor(x => x.Role);

    [Theory]
    [InlineData(Role.Employee)]
    [InlineData(Role.Manager)]
    public void Role_Defined_Passes(Role role)
        => _sut.TestValidate(new GetUsersQuery(Role: role)).ShouldNotHaveValidationErrorFor(x => x.Role);

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(99)]
    public void Role_Undefined_Fails(int role)
        => _sut.TestValidate(new GetUsersQuery(Role: (Role)role))
               .ShouldHaveValidationErrorFor(x => x.Role);

    [Fact]
    public void DepartmentId_Null_Passes()
        => _sut.TestValidate(new GetUsersQuery(DepartmentId: null)).ShouldNotHaveValidationErrorFor(x => x.DepartmentId);

    [Fact]
    public void DepartmentId_Empty_Fails()
        => _sut.TestValidate(new GetUsersQuery(DepartmentId: Guid.Empty))
               .ShouldHaveValidationErrorFor(x => x.DepartmentId);

    [Fact]
    public void AllFiltersValid_Passes()
        => _sut.TestValidate(new GetUsersQuery(2, "john", "john.doe", "a@b.com", true, Role.Manager, Guid.NewGuid()))
               .ShouldNotHaveAnyValidationErrors();

    private void Check(GetUsersQuery query, System.Linq.Expressions.Expression<Func<GetUsersQuery, string?>> property, bool shouldFail)
    {
        var result = _sut.TestValidate(query);
        if (shouldFail) result.ShouldHaveValidationErrorFor(property);
        else result.ShouldNotHaveValidationErrorFor(property);
    }
}
