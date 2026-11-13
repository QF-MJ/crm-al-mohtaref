using CRM.AlMohtaref.Domain.Entities.Contacts;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Domain.Entities
{
    public class Person
    {
        

        public Guid Id { get; set; }

        public string? FullName { get; set; }

        public Guid? CreatedByUserId { get; set; }

       
        public virtual User User { get; set; }

        public virtual User? CreatedByUser { get; set; }

        public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();

        public virtual Student? Student { get; set; }


        public Person(Guid id, string? fullName, Guid? createdByUserId)
        {
            Id = id;
            FullName = fullName;
            CreatedByUserId = createdByUserId;
        }
    }

}
