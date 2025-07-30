using Seek.ZATCA.Core.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Responses.Compliance
{
    public class ValidationResponse
    {
        /// <summary>
        /// Overall validation status
        /// </summary>
        public ValidationStatus Status { get; set; }

        /// <summary>
        /// Detailed validation results
        /// </summary>
        public List<ValidationResult> Results { get; set; } = new();

        /// <summary>
        /// Validated invoice in UBL 2.1 format (if successful)
        /// </summary>
        public string? ValidatedInvoiceXml { get; set; }

        /// <summary>
        /// ZATCA-generated UUID for successful clearance (Phase 2)
        /// </summary>
        public string? ClearanceUUID { get; set; }

        /// <summary>
        /// Cryptographic stamp (Phase 2 successful validation)
        /// </summary>
        public string? CryptographicStamp { get; set; }

        /// <summary>
        /// Base64-encoded QR code (when applicable)
        /// </summary>
        public string? QRCode { get; set; }

        /// <summary>
        /// Timestamp of validation
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
