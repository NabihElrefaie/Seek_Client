using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Requests.Compliance
{
    /// <summary>
    /// Request to digitally sign invoice (BR-32)
    /// </summary>
    public class SigningRequest
    {
        /// <summary>
        /// Invoice ID to sign
        /// </summary>
        [Required]
        public string InvoiceId { get; set; }

        /// <summary>
        /// HSM-stored key reference (KSA-15)
        /// </summary>
        [Required]
        public string KeyId { get; set; }
    }
}
