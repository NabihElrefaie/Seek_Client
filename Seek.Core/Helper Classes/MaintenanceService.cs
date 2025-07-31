using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.Helper_Classes
{
    public class MaintenanceService
    {
        private bool _isInMaintenance = false;

        // Flag to determine if the app is in maintenance mode
        public bool IsInMaintenance => _isInMaintenance;

        // Set the system to maintenance mode (used when starting encryption/decryption)
        public void EnableMaintenance() => _isInMaintenance = true;

        // Exit maintenance mode (used when encryption/decryption is finished)
        public void DisableMaintenance() => _isInMaintenance = false;
    }
}
