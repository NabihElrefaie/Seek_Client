using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Responses.Compliance
{
    /// <summary>
    /// Technical metadata about the signing process
    /// </summary>
    public class SigningTechnicalDetails
    {
        /// <summary>
        /// Key identifier used for signing
        /// </summary>
        public string? KeyId { get; set; }

        /// <summary>
        /// Signing algorithm used
        /// </summary>
        public string Algorithm { get; set; } = "SHA256withRSA";

        /// <summary>
        /// Certificate thumbprint (if applicable)
        /// </summary>
        public string? CertificateThumbprint { get; set; }

        /// <summary>
        /// Key expiration date
        /// </summary>
        public DateTime? KeyExpiryDate { get; set; }

        /// <summary>
        /// HSM/Security module details
        /// </summary>
        public SecurityModuleInfo? SecurityModule { get; set; }
    }

}
