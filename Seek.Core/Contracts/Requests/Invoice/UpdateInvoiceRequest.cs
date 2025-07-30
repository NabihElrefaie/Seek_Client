using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Requests.Invoice
{
    /// <summary>
    /// Request to update existing invoice
    /// </summary>
    public class UpdateInvoiceRequest
    {
        /// <summary>
        /// Invoice ID to update
        /// </summary>
        [Required]
        public string InvoiceId { get; set; }

        /// <summary>
        /// Updated line items
        /// </summary>
        public List<InvoiceLineItemRequest>? LineItems { get; set; }

        /// <summary>
        /// Updated notes
        /// </summary>
        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Update reason (required for Phase 2)
        /// </summary>
        [MaxLength(500)]
        public string? UpdateReason { get; set; }
    }
}
