using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Requests.Shared
{
    /// <summary>
    /// Date range filter
    /// </summary>
    public class DateRangeRequest
    {
        /// <summary>
        /// Start date (inclusive)
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date (inclusive)
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Timezone identifier (IANA)
        /// </summary>
        [MaxLength(50)]
        public string Timezone { get; set; } = "Asia/Riyadh";
    }
}
