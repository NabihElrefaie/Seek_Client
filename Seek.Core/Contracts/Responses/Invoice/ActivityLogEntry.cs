using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Responses.Invoice
{
    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string? Details { get; set; }
    }
}
