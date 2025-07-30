using Seek.ZATCA.Core.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Responses.Compliance
{
    public class ValidationResult
    {
        /// <summary>
        /// Validation check type
        /// </summary>
        public ValidationCheckType CheckType { get; set; }

        /// <summary>
        /// ZATCA rule identifier (e.g., "BR-12")
        /// </summary>
        public string RuleCode { get; set; }

        /// <summary>
        /// Check status
        /// </summary>
        public CheckStatus Status { get; set; }

        /// <summary>
        /// Detailed message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Affected field path (e.g., "Invoice.LineItems[0].Price")
        /// </summary>
        public string? TargetField { get; set; }

        /// <summary>
        /// Expected value (for failed checks)
        /// </summary>
        public string? ExpectedValue { get; set; }

        /// <summary>
        /// Actual value (for failed checks)
        /// </summary>
        public string? ActualValue { get; set; }
    }
}
