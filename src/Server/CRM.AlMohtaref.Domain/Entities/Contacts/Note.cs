using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Domain.Entities.Contacts;

public class Note
{
    public Guid Id { get; private set; }

    public string Text { get; private set; }


    public Guid ContactId { get; private set; }

    public Contact Contact { get; private set; }


    public Guid CreatedByUserId { get; private set; }

    public User CreatedByUser { get; private set; }


    private Note() { }

    public Note(Guid id, string text, Guid contactId, Guid? createdByUserId)
    {
        Id = id;
        Text = text;
        ContactId = contactId;
        CreatedByUserId = createdByUserId ?? throw new ArgumentNullException(nameof(createdByUserId), "معرف المستخدم مطلوب.");
    }

}
