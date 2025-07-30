using Seek.ZATCA.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models
{
    public class ComplianceResult
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string InvoiceId { get; set; } // FK to Invoice
        public ComplianceLevel Level { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

        // Validation Details
        public List<ComplianceRuleResult> RuleResults { get; set; } = new();
        public string? SignedInvoiceXml { get; set; }
        public string? ReportingId { get; set; } // For Phase 2 reporting

        // Status Flags
        public bool IsCompliant => RuleResults.All(r => r.IsPassed);
        public bool HasWarnings => RuleResults.Any(r => r.Severity == ComplianceSeverity.Warning);
    }
}
