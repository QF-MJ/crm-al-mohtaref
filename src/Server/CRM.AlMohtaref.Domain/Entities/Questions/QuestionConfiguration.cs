namespace CRM.AlMohtaref.Domain.Entities.Questions;

// How a question is handled: its answer type, who it applies to and the field it maps to.
public class QuestionConfiguration
{
    public Guid Id { get; private set; }

    // optional: a question may only produce tags and not write into any field
    public Guid? FieldId { get; private set; }

    public QuestionType Type { get; private set; }

    public QuestionTarget Target { get; private set; }

    public Field? Field { get; private set; }

    public ICollection<Question> Questions { get; private set; }

    private QuestionConfiguration()
    {
        Questions = new List<Question>();
    }

    public QuestionConfiguration(QuestionType type, QuestionTarget target, Guid? fieldId = null) : this()
    {
        Type = type;
        Target = target;
        FieldId = fieldId;
    }
}
