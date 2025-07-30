using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Enums
{
    [Flags]
    public enum ValidationCheck
    {
        /// <summary>
        /// Validate UBL XML structure (BR-4)
        /// </summary>
        InvoiceStructure = 1,

        /// <summary>
        /// Verify tax calculations (BR-5, BR-6)
        /// </summary>
        TaxCalculations = 2,

        /// <summary>
        /// Validate QR code contents (BR-31)
        /// </summary>
        QRCode = 4,

        /// <summary>
        /// Verify hash chain integrity (BR-33)
        /// </summary>
        HashChain = 8,

        /// <summary>
        /// Validate digital signature (BR-32)
        /// </summary>
        DigitalSignature = 16,

        /// <summary>
        /// All available checks
        /// </summary>
        All = InvoiceStructure | TaxCalculations | QRCode | HashChain | DigitalSignature
    }
}
