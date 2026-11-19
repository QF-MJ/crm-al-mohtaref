using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.AlMohtaref.Application.Features.Campaigns.Dtos
{
    public class TrackDto
    {
        public string TrackName { get; private set; }
        public TrackDto(string trackName) {
            this.TrackName = trackName;

        }
    }
}
