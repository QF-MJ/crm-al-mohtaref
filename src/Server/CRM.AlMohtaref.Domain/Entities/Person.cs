using CRM.AlMohtaref.Domain.Entities.Contacts;
using CRM.AlMohtaref.Domain.Entities.Users;

namespace CRM.AlMohtaref.Domain.Entities
{
    public class Person
    {

        public string? FullName { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public virtual User? CreatedByUser { get; set; }


        public Person()
        {

        }

        public Person(string? fullName, Guid? createdByUserId)
        {
            FullName = fullName;
            CreatedByUserId = createdByUserId;
        }
    }

}
