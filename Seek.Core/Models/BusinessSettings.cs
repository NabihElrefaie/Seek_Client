using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models
{
    public class BusinessSettings
    {
        public string VATNumber { get; set; }
        public string BranchName { get; set; }
        public string BusinessName { get; set; }
        public Address BusinessAddress { get; set; }
        public string? TaxCertificateNumber { get; set; }
        public string? DeviceSerialNumber { get; set; } // For Phase 2
        public string? ProductionEnvironmentId { get; set; }
    }
}
