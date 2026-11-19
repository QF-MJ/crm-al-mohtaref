namespace CRM.AlMohtaref.Domain.Entities.Questions;

// A system field that an answer can be written into (e.g. a column of the lead data).
public class Field
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string DisplayName { get; private set; }

    public QuestionConfiguration Configuration { get; private set; }

    private Field()
    {
        Name = string.Empty;
        DisplayName = string.Empty;
    }

    public Field(string name, string displayName) : this()
    {
        Name = name;
        DisplayName = displayName;
    }
}
