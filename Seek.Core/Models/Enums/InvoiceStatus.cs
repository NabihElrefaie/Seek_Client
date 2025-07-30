using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models.Enums
{
    public enum InvoiceStatus
    {
        Draft = 0,
        Generated = 1,
        Signed = 2,
        SubmittedToZATCA = 3,
        Approved = 4,
        Rejected = 5,
        Cancelled = 6,
        Reported = 7,
        Archived = 8
    }
}
