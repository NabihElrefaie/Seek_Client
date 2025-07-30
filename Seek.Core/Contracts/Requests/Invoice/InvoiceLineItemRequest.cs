using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Requests.Invoice
{
    public class InvoiceLineItemRequest
    {
        /// <summary>
        /// Item identifier (GS1 or internal code) (KSA-10)
        /// </summary>
        [Required(ErrorMessage = "Item code is required (KSA-10)")]
        [MaxLength(50, ErrorMessage = "Item code cannot exceed 50 characters")]
        public string ItemCode { get; set; }

        /// <summary>
        /// Item description (BR-10)
        /// </summary>
        [Required(ErrorMessage = "Description is required (BR-10)")]
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        /// <summary>
        /// Item quantity (BR-11)
        /// </summary>
        [Required(ErrorMessage = "Quantity is required (BR-11)")]
        [Range(0.0001, double.MaxValue,
        ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unit price before tax (BR-13)
        /// </summary>
        [Required(ErrorMessage = "Unit price is required (BR-13)")]
        [Range(0.01, double.MaxValue,
        ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Unit of measurement (UN/ECE rec20) (BR-12)
        /// Default: "EA" (each)
        /// </summary>
        [Required(ErrorMessage = "Unit is required (BR-12)")]
        [MaxLength(10, ErrorMessage = "Unit cannot exceed 10 characters")]
        public string Unit { get; set; } = "EA";
    }
}
