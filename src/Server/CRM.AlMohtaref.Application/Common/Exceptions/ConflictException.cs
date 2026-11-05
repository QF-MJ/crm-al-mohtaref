using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.AlMohtaref.Application.Common.Exceptions
{
    public class ConflictException : AppException
    {
        public ConflictException(string message) : base(409, message)
        {
        }
    }
}
