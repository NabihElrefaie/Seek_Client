using Seek.ZATCA.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models
{
    public class ComplianceRuleResult
    {
        public string RuleCode { get; set; } // ZATCA-001, ZATCA-002, etc.
        public string Description { get; set; }
        public bool IsPassed { get; set; }
        public ComplianceSeverity Severity { get; set; }
        public string? FailedField { get; set; }
        public string? ExpectedValue { get; set; }
        public string? ActualValue { get; set; }
    }
}
