using Seek.ZATCA.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models.Tax
{
    public class LineItemTax
    {
        public TaxType Type { get; set; }
        public decimal Rate { get; set; } // 0.15 for 15%
        public decimal Amount { get; set; }
    }
}
