using Seek.ZATCA.Core.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Requests.Shared
{
    /// <summary>
    /// Standard paging request parameters
    /// </summary>
    public class PagingRequest
    {
        /// <summary>
        /// Page number (1-based)
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Items per page
        /// </summary>
        [Range(1, 1000)]
        public int PageSize { get; set; } = 50;

        /// <summary>
        /// Search filter string
        /// </summary>
        [MaxLength(100)]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Sorting field
        /// </summary>
        [MaxLength(50)]
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction
        /// </summary>
        public SortDirection SortDirection { get; set; } = SortDirection.Asc;
    }
}
