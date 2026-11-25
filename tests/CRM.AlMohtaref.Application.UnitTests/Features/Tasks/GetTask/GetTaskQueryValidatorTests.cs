using CRM.AlMohtaref.Application.Features.Tasks.GetTask;
using FluentValidation.TestHelper;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Tasks.GetTask;

// Validator = configuration rules only (shape of the input), no business rules here.
public class GetTaskQueryValidatorTests
{
    private readonly GetTaskQueryValidator _validator = new();

    // ---------- Happy path ----------

    [Fact]
    public void ValidQuery_HasNoErrors()
    {
        _validator.TestValidate(new GetTaskQuery(Guid.NewGuid()))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AnyNonEmptyGuid_Passes()
    {
        var id = new Guid("00000000-0000-0000-0000-000000000001");

        _validator.TestValidate(new GetTaskQuery(id))
            .ShouldNotHaveValidationErrorFor(x => x.TaskId);
    }

    // ---------- TaskId ----------

    [Fact]
    public void TaskId_Empty_Fails()
    {
        _validator.TestValidate(new GetTaskQuery(Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.TaskId)
            .WithErrorMessage("معرف المهمة مطلوب");
    }

    [Fact]
    public void TaskId_Default_Fails()
    {
        _validator.TestValidate(new GetTaskQuery(default))
            .ShouldHaveValidationErrorFor(x => x.TaskId);
    }

    [Fact]
    public void TaskId_Empty_ReportsOnlyOneError()
    {
        var result = _validator.TestValidate(new GetTaskQuery(Guid.Empty));

        Assert.Single(result.Errors);
    }
}
