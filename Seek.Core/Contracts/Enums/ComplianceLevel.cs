using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Enums
{
    /// <summary>
    /// Invoice compliance level
    /// </summary>
    public enum ComplianceLevel
    {
        /// <summary>
        /// Phase 1 basic requirements
        /// </summary>
        Basic,

        /// <summary>
        /// Phase 2 clearance requirements
        /// </summary>
        Clearance,

        /// <summary>
        /// Phase 2 reporting requirements
        /// </summary>
        Reporting
    }
}
