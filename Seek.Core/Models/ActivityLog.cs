using Seek.ZATCA.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models
{
    public class ActivityLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Action { get; set; } // SUBMIT, VALIDATE, CANCEL, etc.
        public string EntityType { get; set; } // INVOICE, CREDENTIALS, etc.
        public string EntityId { get; set; }
        public LogLevel Level { get; set; }
        public string Status { get; set; } // SUCCESS, FAILED, PENDING
        public string Message { get; set; }
        public string? Details { get; set; } // JSON payload if needed
        public string? ErrorCode { get; set; } // ZATCA error code
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
