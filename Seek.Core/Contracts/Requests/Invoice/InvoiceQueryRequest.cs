using Seek.ZATCA.Core.Contracts.Requests.Shared;
using Seek.ZATCA.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Requests.Invoice
{
    /// <summary>
    /// Invoice query/filter request
    /// </summary>
    public class InvoiceQueryRequest : PagingRequest
    {
        /// <summary>
        /// Date range filter
        /// </summary>
        public DateRangeRequest? DateRange { get; set; }

        /// <summary>
        /// Invoice status filter
        /// </summary>
        public List<InvoiceStatus>? Statuses { get; set; }

        /// <summary>
        /// Invoice type filter
        /// </summary>
        public List<InvoiceType>? Types { get; set; }

        /// <summary>
        /// Buyer VAT number filter
        /// </summary>
        [StringLength(15)]
        public string? BuyerVATNumber { get; set; }
    }
}
