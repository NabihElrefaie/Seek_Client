using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models
{
    public class ZatcaResponse
    {
        public bool Success { get; set; }
        public string? ProcessingResults { get; set; } // Base64
        public List<ZatcaError> Errors { get; set; } = new();
        public string? SignedInvoice { get; set; } // XML
        public string? UUID { get; set; } // From ZATCA
        public DateTime? TimeStamp { get; set; }
    }
    
}
