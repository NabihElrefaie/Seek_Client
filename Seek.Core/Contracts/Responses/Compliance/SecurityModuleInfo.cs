using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Responses.Compliance
{
    /// <summary>
    /// Security module information (KSA-15)
    /// </summary>
    public class SecurityModuleInfo
    {
        public string? Name { get; set; }
        public string? SerialNumber { get; set; }
        public string? Manufacturer { get; set; }
    }
}
