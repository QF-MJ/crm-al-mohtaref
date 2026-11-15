using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.AlMohtaref.Infrastructure.ExternalServices
{
    public class GoogleSheetsSettings
    {
        public const string SectionName = "GoogleSheets";

        public string CredentialsPath { get; set; } = default!;
        public string ApplicationName { get; set; } = default!;
    }
}
