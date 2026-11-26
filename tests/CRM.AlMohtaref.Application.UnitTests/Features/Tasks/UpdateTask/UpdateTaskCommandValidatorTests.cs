using CRM.AlMohtaref.Application.Features.Tasks.UpdateTask;
using FluentValidation.TestHelper;
using TaskStatus = CRM.AlMohtaref.Domain.Entities.Tasks.TaskStatus;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Tasks.UpdateTask;

public class UpdateTaskCommandValidatorTests
{
    private readonly UpdateTaskCommandValidator _validator = new();

    private static readonly DateTime Start = new(2026, 11, 24);

    private static UpdateTaskCommand Valid() => new(
        TaskId: Guid.NewGuid(),
        Title: "title",
        Description: null,
        Status: TaskStatus.Pending,
        StartTime: Start,
        EndTime: Start.AddDays(1),
        AssignedToUserId: Guid.NewGuid());

    // ---------- Happy path ----------

    [Fact]
    public void ValidCommand_HasNoErrors()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void FullyPopulatedCommand_HasNoErrors()
    {
        var command = Valid() with
        {
            Description = "description",
            Status = TaskStatus.InProgress,
            EndTime = Start.AddDays(30)
        };

        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    // ---------- TaskId ----------

    [Fact]
    public void TaskId_Empty_Fails()
    {
        _validator.TestValidate(Valid() with { TaskId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.TaskId);
    }

    [Fact]
    public void TaskId_Empty_ReportsOnlyOneError()
    {
        var result = _validator.TestValidate(Valid() with { TaskId = Guid.Empty });

        Assert.Single(result.Errors);
    }

    [Fact]
    public void TaskId_AnyNonEmptyGuid_Passes()
    {
        var id = new Guid("00000000-0000-0000-0000-000000000001");

        _validator.TestValidate(Valid() with { TaskId = id })
            .ShouldNotHaveValidationErrorFor(x => x.TaskId);
    }

    // ---------- Title (required, max 200) ----------

    [Fact]
    public void Title_Null_Fails()
    {
        _validator.TestValidate(Valid() with { Title = null! })
            .ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    [InlineData("\t\n")]
    public void Title_EmptyOrWhitespace_Fails(string title)
    {
        _validator.TestValidate(Valid() with { Title = title })
            .ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Title_SingleChar_Passes()
    {
        _validator.TestValidate(Valid() with { Title = "a" })
            .ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Title_Exactly200Chars_Passes()
    {
        _validator.TestValidate(Valid() with { Title = new string('\u0623', 200) })
            .ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Title_201Chars_Fails()
    {
        _validator.TestValidate(Valid() with { Title = new string('\u0623', 201) })
            .ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Title_Empty_ReportsOnlyOneError()
    {
        var result = _validator.TestValidate(Valid() with { Title = "" });

        Assert.Single(result.Errors);
    }

    // ---------- Description (nullable, max 2000) ----------

    [Fact]
    public void Description_Null_Passes()
    {
        _validator.TestValidate(Valid() with { Description = null })
            .ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_Empty_Passes()
    {
        // The column is nullable and has no minimum length.
        _validator.TestValidate(Valid() with { Description = "" })
            .ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_Exactly2000Chars_Passes()
    {
        _validator.TestValidate(Valid() with { Description = new string('x', 2000) })
            .ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_2001Chars_Fails()
    {
        _validator.TestValidate(Valid() with { Description = new string('x', 2001) })
            .ShouldHaveValidationErrorFor(x => x.Description);
    }

    // ---------- Status (stored as int, must be a defined value) ----------

    public static IEnumerable<object[]> AllStatuses() =>
        Enum.GetValues<TaskStatus>().Select(s => new object[] { s });

    [Theory]
    [MemberData(nameof(AllStatuses))]
    public void Status_EveryDefinedValue_Passes(TaskStatus status)
    {
        _validator.TestValidate(Valid() with { Status = status })
            .ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Theory]
    [InlineData(0)]    // enum starts at 1
    [InlineData(6)]
    [InlineData(-1)]
    [InlineData(999)]
    public void Status_UndefinedValue_Fails(int raw)
    {
        _validator.TestValidate(Valid() with { Status = (TaskStatus)raw })
            .ShouldHaveValidationErrorFor(x => x.Status);
    }

    // ---------- StartTime (required, "date" column) ----------

    [Fact]
    public void StartTime_Default_Fails()
    {
        _validator.TestValidate(Valid() with { StartTime = default })
            .ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void StartTime_WithTimeOfDay_Passes()
    {
        var command = Valid() with
        {
            StartTime = new DateTime(2026, 11, 24, 10, 30, 0),
            EndTime = new DateTime(2026, 11, 25)
        };

        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.StartTime);
    }

    // ---------- EndTime (required, >= StartTime, CK_Tasks_EndTime_GreaterThanOrEqual_StartTime) ----------

    [Fact]
    public void EndTime_Default_Fails()
    {
        _validator.TestValidate(Valid() with { EndTime = default })
            .ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void EndTime_DayBeforeStartTime_Fails()
    {
        _validator.TestValidate(Valid() with { StartTime = Start, EndTime = Start.AddDays(-1) })
            .ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void EndTime_LastSecondOfPreviousDay_Fails()
    {
        var command = Valid() with
        {
            StartTime = Start,
            EndTime = Start.AddSeconds(-1)
        };

        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void EndTime_EqualToStartTime_Passes()
    {
        _validator.TestValidate(Valid() with { StartTime = Start, EndTime = Start })
            .ShouldNotHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void EndTime_AfterStartTime_Passes()
    {
        _validator.TestValidate(Valid() with { StartTime = Start, EndTime = Start.AddDays(1) })
            .ShouldNotHaveValidationErrorFor(x => x.EndTime);
    }

    // The DB columns are "date" (no time part), so the same calendar day is always a valid range
    // regardless of the time-of-day the client sends.
    [Fact]
    public void SameDay_EndTimeEarlierTimeOfDay_Passes()
    {
        var command = Valid() with
        {
            StartTime = new DateTime(2026, 11, 24, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 24, 9, 0, 0)
        };

        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void SameDay_StartLateInDay_EndAtMidnight_Passes()
    {
        var command = Valid() with
        {
            StartTime = new DateTime(2026, 11, 24, 23, 59, 0),
            EndTime = new DateTime(2026, 11, 24, 0, 0, 0)
        };

        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void PreviousDay_EvenIfTimeOfDayIsLater_Fails()
    {
        var command = Valid() with
        {
            StartTime = new DateTime(2026, 11, 24, 0, 0, 0),
            EndTime = new DateTime(2026, 11, 23, 23, 59, 0)
        };

        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    // ---------- AssignedToUserId (non-nullable FK) ----------

    [Fact]
    public void AssignedToUserId_Empty_Fails()
    {
        _validator.TestValidate(Valid() with { AssignedToUserId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.AssignedToUserId);
    }

    [Fact]
    public void AssignedToUserId_NonEmpty_Passes()
    {
        _validator.TestValidate(Valid() with { AssignedToUserId = Guid.NewGuid() })
            .ShouldNotHaveValidationErrorFor(x => x.AssignedToUserId);
    }

    // ---------- Misc ----------

    [Fact]
    public void MultipleInvalidFields_ReportsEachOne()
    {
        var command = new UpdateTaskCommand(
            TaskId: Guid.Empty,
            Title: " ",
            Description: new string('x', 2001),
            Status: (TaskStatus)99,
            StartTime: Start,
            EndTime: Start.AddDays(-5),
            AssignedToUserId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TaskId);
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Description);
        result.ShouldHaveValidationErrorFor(x => x.Status);
        result.ShouldHaveValidationErrorFor(x => x.EndTime);
        result.ShouldHaveValidationErrorFor(x => x.AssignedToUserId);
    }


    [Fact]
    public void ErrorMessages_AreInArabic_LikeTheOtherTaskValidators()
    {
        var command = new UpdateTaskCommand(
            Guid.Empty, "", new string('x', 2001), (TaskStatus)99, default, default, Guid.Empty);

        var result = _validator.TestValidate(command);

        Assert.NotEmpty(result.Errors);
        Assert.All(result.Errors, e => Assert.Matches(@"[\u0600-\u06FF]", e.ErrorMessage));
    }
}
