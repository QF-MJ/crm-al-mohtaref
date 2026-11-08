namespace CRM.AlMohtaref.Application.Features.Contacts;

public class ContactItemDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public List<StudentDto> Students { get; set; }
}

