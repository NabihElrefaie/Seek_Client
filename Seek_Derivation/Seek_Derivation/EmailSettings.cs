using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Seek_Derivation
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public string FromEmail { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; } = "Seek"; // Add this property
        public string Password { get; set; }
        public string AdminEmail { get; set; }
    }

}
