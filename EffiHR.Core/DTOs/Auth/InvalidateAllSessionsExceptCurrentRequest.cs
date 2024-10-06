using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.Core.DTOs.Auth
{
    public class InvalidateAllSessionsExceptCurrentRequest
    {
        public string CurrentSessionId { get; set; }
    }

}
