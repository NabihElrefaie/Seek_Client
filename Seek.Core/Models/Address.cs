using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models
{
    public class Address
    {
        // Required by ZATCA
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; } = "SA";

        // Optional
        public string? AdditionalNumber { get; set; }
        public string? BuildingNumber { get; set; }
    }
}
