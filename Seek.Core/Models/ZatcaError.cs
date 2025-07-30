using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models
{
    public class ZatcaError
    {
        public string Category { get; set; } // "VALIDATION", "SECURITY"
        public string Code { get; set; } // "ZATCA-1001"
        public string Message { get; set; }
        public string Severity { get; set; } // "ERROR", "WARNING"
        public string? Target { get; set; } // Field path
    }
}
