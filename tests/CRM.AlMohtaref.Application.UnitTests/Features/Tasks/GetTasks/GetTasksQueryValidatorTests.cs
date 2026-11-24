using CRM.AlMohtaref.Application.Features.Tasks.GetTasks;
using FluentValidation.TestHelper;
using TaskType = CRM.AlMohtaref.Domain.Entities.Tasks.TaskType;

namespace CRM.AlMohtaref.Application.UnitTests.Features.Tasks.GetTasks;

// Validator = configuration rules only (shape of the input), no business rules here.
public class GetTasksQueryValidatorTests
{
    private readonly GetTasksQueryValidator _validator = new();

    private static readonly DateTime Start = new(2026, 11, 24);

    private static GetTasksQuery Valid(int page = 1) => new(
        AssignedToUserId: Guid.NewGuid(),
        PageNumber: page,
        Title: null,
        Type: null,
        StartTime: Start,
        EndTime: Start.AddDays(7),
        GroupBySubTask: false);

    // ---------- Happy path ----------

    [Fact]
    public void ValidQuery_HasNoErrors()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void FullyPopulatedQuery_HasNoErrors()
    {
        var query = Valid() with { Title = "متابعة", Type = TaskType.FollowUp, GroupBySubTask = true };
        _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
    }

    // ---------- PageNumber (from PagedQueryValidator via Include) ----------

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void PageNumber_NotPositive_Fails(int page)
    {
        _validator.TestValidate(Valid(page)).ShouldHaveValidationErrorFor(x => x.PageNumber);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10_000)]
    public void PageNumber_Positive_Passes(int page)
    {
        _validator.TestValidate(Valid(page)).ShouldNotHaveValidationErrorFor(x => x.PageNumber);
    }

    // ---------- AssignedToUserId ----------

    [Fact]
    public void AssignedToUserId_Empty_Fails()
    {
        var query = Valid() with { AssignedToUserId = Guid.Empty };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor(x => x.AssignedToUserId)
            .WithErrorMessage("معرف المنفذ مطلوب");
    }

    // ---------- Title ----------

    [Fact]
    public void Title_Null_Passes()
    {
        _validator.TestValidate(Valid() with { Title = null })
            .ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("     ")]
    [InlineData("\t\n")]
    public void Title_EmptyOrWhitespace_Fails(string title)
    {
        _validator.TestValidate(Valid() with { Title = title })
            .ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("عنوان المهمة لا يمكن أن يكون فارغاً");
    }

    [Fact]
    public void Title_Exactly200Chars_Passes()
    {
        _validator.TestValidate(Valid() with { Title = new string('أ', 200) })
            .ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Title_201Chars_Fails()
    {
        _validator.TestValidate(Valid() with { Title = new string('أ', 201) })
            .ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("عنوان المهمة يجب ألا يتجاوز 200 حرف");
    }

    [Fact]
    public void Title_NormalText_DoesNotTriggerWrongMessage()
    {
        // "When" must scope both Title rules: a valid title must not produce any error
        _validator.TestValidate(Valid() with { Title = "اتصال بالعميل" })
            .ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    // ---------- Type ----------

    public static IEnumerable<object[]> AllTaskTypes() =>
        Enum.GetValues<TaskType>()
            .Where(t => t != TaskType.SubTask)
            .Select(t => new object[] { t });

    [Theory]
    [MemberData(nameof(AllTaskTypes))]
    public void Type_EveryDefinedValue_Passes(TaskType type)
    {
        _validator.TestValidate(Valid() with { Type = type })
            .ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void Type_Null_Passes()
    {
        _validator.TestValidate(Valid() with { Type = null })
            .ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Theory]
    [InlineData(0)]   // enum starts at 1
    [InlineData(9)]
    [InlineData(-1)]
    [InlineData(999)]
    public void Type_UndefinedValue_Passes(int raw)
    {
        _validator.TestValidate(Valid() with { Type = (TaskType)raw })
            .ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("نوع المهمة غير صالح");
    }

    // ---------- Date range ----------

    [Fact]
    public void EndTime_BeforeStartTime_Passes()
    {
        var query = Valid() with { StartTime = Start, EndTime = Start.AddDays(-1) };

        _validator.TestValidate(query)
            .ShouldHaveValidationErrorFor(x => x.EndTime)
            .WithErrorMessage("تاريخ النهاية يجب أن يكون أكبر من أو يساوي تاريخ البداية");
    }

    [Fact]
    public void EndTime_EqualToStartTime_Passes()
    {
        var query = Valid() with { StartTime = Start, EndTime = Start };
        _validator.TestValidate(query).ShouldNotHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void EndTime_AfterStartTime_Passes()
    {
        var query = Valid() with { StartTime = Start, EndTime = Start.AddDays(1) };
        _validator.TestValidate(query).ShouldNotHaveValidationErrorFor(x => x.EndTime);
    }

    // The DB columns are "date" (no time part), so the same day must be a valid range
    // regardless of the time-of-day the client sends. Expected to FAIL today:
    // the validator compares full DateTime values.
    [Fact]
    public void SameDay_EndTimeEarlierTimeOfDay_Passes()
    {
        var query = Valid() with
        {
            StartTime = new DateTime(2026, 11, 24, 10, 0, 0),
            EndTime = new DateTime(2026, 11, 24, 9, 0, 0)
        };

        _validator.TestValidate(query).ShouldNotHaveValidationErrorFor(x => x.EndTime);
    }

    // Per the API spec StartTime and EndTime are required.
    // Expected to FAIL today: both are nullable and the rule is skipped when null.
    [Fact]
    public void StartTime_Missing_Fails()
    {
        var query = Valid() with { StartTime = null, EndTime = Start };
        _validator.TestValidate(query).ShouldHaveValidationErrorFor(x => x.StartTime);
    }

    [Fact]
    public void EndTime_Missing_Fails()
    {
        var query = Valid() with { StartTime = Start, EndTime = null };
        _validator.TestValidate(query).ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void BothDates_Missing_Fails()
    {
        var result = _validator.TestValidate(Valid() with { StartTime = null, EndTime = null });

        result.ShouldHaveValidationErrorFor(x => x.StartTime);
        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    // ---------- Misc ----------

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GroupBySubTask_AnyValue_Passes(bool group)
    {
        _validator.TestValidate(Valid() with { GroupBySubTask = group })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MultipleInvalidFields_ReportsEachOne()
    {
        var query = Valid(page: 0) with
        {
            AssignedToUserId = Guid.Empty,
            Title = " ",
            Type = (TaskType)99,
            StartTime = Start,
            EndTime = Start.AddDays(-5)
        };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        result.ShouldHaveValidationErrorFor(x => x.AssignedToUserId);
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Type);
        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void Type_SubTask_Passes()
    {
        _validator.TestValidate(Valid() with { Type = TaskType.SubTask })
            .ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("لا يمكن الفلترة بنوع مهمة فرعية");
    }
}
