using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.AlMohtaref.Domain.Entities.Campaigns
{
    public class CampaginStatus
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public ICollection<Campaign>? Campaigns { get; set; }




    }
}
