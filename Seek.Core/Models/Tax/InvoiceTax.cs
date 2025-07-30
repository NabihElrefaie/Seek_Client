using Seek.ZATCA.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models.Tax
{
    public class InvoiceTax
    {
        public TaxType Type { get; set; } // VAT, WHT, etc.
        public decimal Rate { get; set; } // VAT 0.15 for 15%
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
