using Seek.ZATCA.Core.Models.Enums;
using Seek.ZATCA.Core.Models.Tax;
using System.ComponentModel.DataAnnotations;


namespace Seek.ZATCA.Core.Models
{
    public class Invoice
    {
        // Core Identifiers (BR-1, BR-24)
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; }
        public int InvoiceCounter { get; set; }
        public string DocumentType { get; set; } = "0100000"; // Standard invoice code

        // Timestamps (BR-2, BR-34-35)
        public DateTime IssueDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ZatcaClearanceTimestamp { get; set; }
        public DateTime? ReportingTimestamp { get; set; }

        // Seller Information (BR-3, KSA-2)
        public string SellerName { get; set; }
        public string SellerVATNumber { get; set; } // 15 digits
        public Address SellerAddress { get; set; } = new();

        // Buyer Information
        public string BuyerName { get; set; }
        public string? BuyerVATNumber { get; set; } // Optional for B2C
        public Address? BuyerAddress { get; set; }

        // Financials (BR-4 to BR-8)
        public InvoiceFinancials Financials { get; set; } = new();

        // ZATCA Phase 2 Compliance Fields
        public string? QRCode { get; set; } // Base64 encoded QR (BR-31)
        public string? CryptographicStamp { get; set; } // RSA signature (BR-32)
        public string? InvoiceHash { get; set; } // SHA-256 of UBL (BR-32)
        public string? PreviousInvoiceHash { get; set; } // For corrections (BR-33)
        public string? InvoiceCounterReference { get; set; } // Chain ID (BR-1)
        public string? ReportingId { get; set; } // BR-35


        // Collections
        public List<InvoiceLineItem> LineItems { get; set; } = new();
        public List<InvoiceTax> Taxes { get; set; } = new();
        public List<ComplianceResult> ComplianceResults { get; set; } = new();

        // Status
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
