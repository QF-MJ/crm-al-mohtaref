using CRM.AlMohtaref.Domain.Entities.Contacts;

namespace CRM.AlMohtaref.Domain.Entities.Tasks;

public class StudentTask
{
    public Guid Id { get; set; }

    
    public Guid TaskId { get; set; }
    
    public Task Task { get; set; } = null!;

    
    public Guid StudentId { get; set; }

    public Student Student { get; set; } = null!;
}
