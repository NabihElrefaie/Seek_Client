using Seek.ZATCA.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Extensions
{
    public static class InvoiceExtensions
    {
        public static bool IsZatcaCompliant(this Invoice invoice)
        {
            return !string.IsNullOrEmpty(invoice.SellerVATNumber) &&
                   invoice.LineItems.Count > 0 &&
                   invoice.TotalWithoutVAT > 0 &&
                   invoice.TotalVAT >= 0;
        }

        public static string ToXml(this Invoice invoice)
        {
            return null;
            // Implementation for ZATCA XML generation
        }
    }
}
