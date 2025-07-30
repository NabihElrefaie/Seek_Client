using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Requests.Invoice
{
    public class SignatureParams
    {
        /// <summary>
        /// Key identifier from HSM/KeyVault
        /// </summary>
        [Required(ErrorMessage = "Key ID is required for signing")]
        public string KeyId { get; set; }

        /// <summary>
        /// Signature algorithm (Default: SHA256withRSA)
        /// </summary>
        public string Algorithm { get; set; } = "SHA256withRSA";

        /// <summary>
        /// Certificate PEM (if not using HSM)
        /// </summary>
        public string? CertificatePEM { get; set; }
    }
}
