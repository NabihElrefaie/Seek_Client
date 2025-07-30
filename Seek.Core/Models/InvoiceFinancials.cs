using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models
{
    public class InvoiceFinancials
    {
        public decimal TotalWithoutVAT { get; set; }
        public decimal TotalVAT { get; set; } // Must be 15% of taxable amount
        public decimal TotalWithVAT { get; set; }
        public string Currency { get; set; } = "SAR";
    }
}
