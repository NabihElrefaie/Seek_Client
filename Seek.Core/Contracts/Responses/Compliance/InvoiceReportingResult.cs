using Seek.ZATCA.Core.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Responses.Compliance
{
    /// <summary>
    /// Individual invoice reporting result
    /// </summary>
    public class InvoiceReportingResult
    {
        /// <summary>
        /// Invoice ID reference
        /// </summary>
        public string InvoiceId { get; set; }

        /// <summary>
        /// Invoice UUID from clearance phase
        /// </summary>
        public string ClearanceUUID { get; set; }

        /// <summary>
        /// Reporting status for this invoice
        /// </summary>
        public InvoiceReportingStatus Status { get; set; }

        /// <summary>
        /// Error details (if failed)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// ZATCA error code (if failed)
        /// </summary>
        public string? ErrorCode { get; set; }
    }
}
