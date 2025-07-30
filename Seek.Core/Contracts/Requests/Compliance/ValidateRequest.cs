using Seek.ZATCA.Core.Contracts.Enums;
using Seek.ZATCA.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Requests.Compliance
{
    public class ValidateRequest
    {
        /// <summary>
        /// Invoice ID or raw UBL XML
        /// </summary>
        [Required(ErrorMessage = "Invoice identifier or XML content is required")]
        public string InvoiceIdentifier { get; set; }

        /// <summary>
        /// Validation level (Basic/Clearance/Reporting)
        /// </summary>
        public ComplianceLevel Level { get; set; } = ComplianceLevel.Clearance;
        /// <summary>
        /// Force revalidation even if previously validated
        /// </summary>
        public bool ForceRevalidation { get; set; } = false;

        /// <summary>
        /// Validate cryptographic stamp (Phase 2 only)
        /// </summary>
        public bool ValidateSignature { get; set; } = true;

        /// <summary>
        /// Validate against ZATCA production sandbox
        /// </summary>
        public bool UseSandbox { get; set; } = true;
        /// <summary>
        /// Detailed validation checks to perform
        /// </summary>
        public List<ValidationCheck> Checks { get; set; } = new()
    {
        ValidationCheck.InvoiceStructure,
        ValidationCheck.TaxCalculations,
        ValidationCheck.QRCode,
        ValidationCheck.HashChain
    };

    }
}
