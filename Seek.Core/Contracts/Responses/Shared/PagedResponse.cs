using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Responses.Shared
{
    /// <summary>
    /// Generic paged response
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// Current page items
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Total item count
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total page count
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
