using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Requests.Invoice
{
    public class CreateInvoiceRequest
    {
        /// <summary>
        /// 15-digit Saudi VAT registration number (BR-3)
        /// Example: "310123456789003"
        /// </summary>
        [Required(ErrorMessage = "Seller VAT number is required for compliance (BR-3)")]
        [StringLength(15, MinimumLength = 15,
        ErrorMessage = "VAT number must be exactly 15 digits (BR-3)")]
        [RegularExpression("^3[0-9]{14}$",
        ErrorMessage = "VAT number must start with 3 and contain 15 digits")]
        public string SellerVATNumber { get; set; }

        /// <summary>
        /// Legal name of the seller as registered with ZATCA
        /// </summary>
        [Required(ErrorMessage = "Seller name is required (KSA-2)")]
        [MaxLength(1000, ErrorMessage = "Seller name cannot exceed 1000 characters")]
        public string SellerName { get; set; }

        /// <summary>
        /// Buyer's name (required for all invoices)
        /// </summary>
        [Required(ErrorMessage = "Buyer name is required")]
        [MaxLength(1000, ErrorMessage = "Buyer name cannot exceed 1000 characters")]
        public string BuyerName { get; set; }

        /// <summary>
        /// 15-digit buyer VAT number (required for B2B invoices)
        /// </summary>
        [StringLength(15, MinimumLength = 15,
        ErrorMessage = "VAT number must be exactly 15 digits when provided")]
        [RegularExpression("^[0-9]*$",
        ErrorMessage = "VAT number must contain only digits")]
        public string? BuyerVATNumber { get; set; }

        /// <summary>
        /// Invoice line items (minimum 1 required) (BR-9 to BR-14)
        /// </summary>
        [Required(ErrorMessage = "At least one line item is required (BR-9)")]
        [MinLength(1, ErrorMessage = "Invoice must contain at least one line item")]
        public List<InvoiceLineItemRequest> LineItems { get; set; } = new();

        /// <summary>
        /// Tax breakdown information (defaults to 15% VAT if empty)
        /// </summary>
        public List<TaxRequest> Taxes { get; set; } = new();

        /// <summary>
        /// Additional notes (KSA-6)
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
        /// <summary>
        /// Currency code (ISO 4217)
        /// Default: SAR (Saudi Riyal)
        /// </summary>
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "SAR";
    }
}
