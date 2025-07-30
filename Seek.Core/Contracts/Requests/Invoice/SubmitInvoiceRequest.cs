using Seek.ZATCA.Core.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Requests.Invoice
{
    /// <summary>
    /// Request to submit invoice for clearance (Phase 2)
    /// </summary>
    public class SubmitInvoiceRequest
    {
        /// <summary>
        /// Unique identifier of the pre-created invoice (UUID format)
        /// </summary>
        [Required(ErrorMessage = "Invoice ID is required")]
        [RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
            ErrorMessage = "Invoice ID must be a valid GUID")]
        public string InvoiceId { get; set; }

        /// <summary>
        /// Cryptographic hash of the previous invoice in the chain (BR-33)
        /// Required for credit/debit notes
        /// Format: Base64-encoded SHA-256
        /// </summary>
        [StringLength(44, MinimumLength = 44,
            ErrorMessage = "Hash must be 44 characters (Base64 SHA-256)")]
        public string? PreviousInvoiceHash { get; set; }

        /// <summary>
        /// Invoice counter reference (BR-1 sequencing requirement)
        /// Format: Sequential number prefixed by seller VAT
        /// Example: "310123456789003-1001"
        /// </summary>
        [Required(ErrorMessage = "Invoice counter reference is required (BR-1)")]
        [StringLength(50, MinimumLength = 10,
            ErrorMessage = "Counter reference must be 10-50 characters")]
        public string InvoiceCounterReference { get; set; }

        /// <summary>
        /// Bypass validation warnings (use with caution)
        /// When true, submits invoice even with non-critical warnings
        /// </summary>
        public bool ForceSubmit { get; set; } = false;

        /// <summary>
        /// Invoice submission type (Default/CreditNote/DebitNote)
        /// </summary>
        public SubmitType SubmitType { get; set; } = SubmitType.Default;

        /// <summary>
        /// Digital signature parameters (BR-32)
        /// </summary>
        public SignatureParams? SignatureParams { get; set; }
    }
}
