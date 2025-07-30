using Seek.ZATCA.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Requests.Invoice
{
    public class TaxRequest
    {
        /// <summary>
        /// Tax type (VAT, Withholding, etc.)
        /// </summary>
        [Required(ErrorMessage = "Tax type is required")]
        public TaxType Type { get; set; } = TaxType.VAT;

        /// <summary>
        /// Tax rate percentage (e.g., 15 for 15%)
        /// </summary>
        [Required(ErrorMessage = "Tax rate is required")]
        [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100")]
        public decimal Rate { get; set; } = 15m;
    }
}
