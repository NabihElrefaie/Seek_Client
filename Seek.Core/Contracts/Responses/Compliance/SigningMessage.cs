using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Responses.Compliance
{
    /// <summary>
    /// Signing process message
    /// </summary>
    public class SigningMessage
    {
        public SigningMessageLevel Level { get; set; }
        public string Code { get; set; } // e.g., "SIGN-1001"
        public string Message { get; set; }
        public string? TechnicalDetails { get; set; }
    }
}
