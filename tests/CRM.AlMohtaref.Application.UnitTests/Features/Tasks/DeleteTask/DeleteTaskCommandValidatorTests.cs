using CRM.AlMohtaref.Application.Features.Tasks.DeleteTask;
using FluentValidation.TestHelper;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Tasks.DeleteTask;

// Validator = configuration rules only (shape of the input), no business rules here.
public class DeleteTaskCommandValidatorTests
{
    private readonly DeleteTaskCommandValidator _validator = new();

    // ---------- Happy path ----------

    [Fact]
    public void ValidCommand_HasNoErrors()
    {
        _validator.TestValidate(new DeleteTaskCommand(Guid.NewGuid()))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AnyNonEmptyGuid_Passes()
    {
        var id = new Guid("00000000-0000-0000-0000-000000000001");

        _validator.TestValidate(new DeleteTaskCommand(id))
            .ShouldNotHaveValidationErrorFor(x => x.TaskId);
    }

    // ---------- TaskId ----------

    [Fact]
    public void TaskId_Empty_Fails()
    {
        _validator.TestValidate(new DeleteTaskCommand(Guid.Empty))
            .ShouldHaveValidationErrorFor(x => x.TaskId)
            .WithErrorMessage("معرف المهمة مطلوب");
    }

    [Fact]
    public void TaskId_Empty_ReportsOnlyOneError()
    {
        var result = _validator.TestValidate(new DeleteTaskCommand(Guid.Empty));

        Assert.Single(result.Errors);
    }

    [Fact]
    public void TaskId_Default_Fails()
    {
        _validator.TestValidate(new DeleteTaskCommand(default))
            .ShouldHaveValidationErrorFor(x => x.TaskId);
    }
}
