using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.AlMohtaref.Application.Features.Campaigns.Dtos
{
    public class PlatFormDto
    {
        public string? Name { get; private set; }

        public PlatFormDto(string? name)
        {
            Name = name;
        }
    }
}
