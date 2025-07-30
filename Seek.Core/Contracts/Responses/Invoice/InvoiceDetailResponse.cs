using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Responses.Invoice
{
    /// <summary>
    /// Detailed invoice response
    /// </summary>
    public class InvoiceDetailResponse : InvoiceResponse
    {
        /// <summary>
        /// Complete UBL XML
        /// </summary>
        public string? InvoiceXml { get; set; }

        /// <summary>
        /// Full compliance history
        /// </summary>
        public List<ComplianceCheck> ComplianceHistory { get; set; } = new();

        /// <summary>
        /// Audit logs
        /// </summary>
        public List<ActivityLogEntry> ActivityLogs { get; set; } = new();
    }
}
