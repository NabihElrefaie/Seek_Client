using Seek.ZATCA.Core.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Responses.Compliance
{
    /// <summary>
    /// Response DTO for invoice reporting operations (Phase 2)
    /// Reference: ZATCA E-Invoicing Implementation Standard vF, Section 8.4
    /// </summary>
    public class ReportingResponse
    {
        /// <summary>
        /// Reporting request status
        /// </summary>
        public ReportingStatus Status { get; set; }

        /// <summary>
        /// ZATCA-assigned reporting ID
        /// </summary>
        public string? ReportingId { get; set; }

        /// <summary>
        /// Reporting timestamp from ZATCA
        /// </summary>
        public DateTime? ReportingTimestamp { get; set; }

        /// <summary>
        /// Reporting period reference (YYYYMM)
        /// </summary>
        public string? PeriodReference { get; set; }

        /// <summary>
        /// Number of invoices successfully reported
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Number of invoices that failed reporting
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// Detailed results per invoice
        /// </summary>
        public List<InvoiceReportingResult> InvoiceResults { get; set; } = new();

        /// <summary>
        /// Raw response from ZATCA API
        /// </summary>
        public string? ZatcaResponse { get; set; }

        /// <summary>
        /// Timestamp when response was received
        /// </summary>
        public DateTime ResponseTimestamp { get; set; } = DateTime.UtcNow;
    }
}
