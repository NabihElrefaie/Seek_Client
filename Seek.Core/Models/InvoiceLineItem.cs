using Seek.ZATCA.Core.Models.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models
{
    public class InvoiceLineItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int LineNumber { get; set; } // Sequential (BR-10)

        // ZATCA Required Fields (BR-9 to BR-12)
        public string ItemCode { get; set; } // GS1 or custom (KSA-10)
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "EA"; // UN/ECE rec20

        // Pricing (BR-13)
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetAmount { get; set; }

        // Tax Breakdown (BR-14)
        public List<LineItemTax> Taxes { get; set; } = new();

        // Cross-border (KSA-11)
        public string? CommodityCode { get; set; } // HS Code
    }
}
