namespace CRM.AlMohtaref.Domain.Entities.Contacts
{
    public class StudentTag
    {
        public Guid Id { get; private set; }

        public Guid TagId { get; private set; }

        public Guid? StudentId { get; private set; }
        
        public Tag Tag { get; private set; }

        public Student? Student { get; private set; }
    }
}
