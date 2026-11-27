using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.AlMohtaref.Application.Features.Campaigns.Dtos
{
    public class QuestionDto
    {
        public string? Text { get; private set; }

        public QuestionDto(string? text)
        {
            Text = text;
        }
    }
}
