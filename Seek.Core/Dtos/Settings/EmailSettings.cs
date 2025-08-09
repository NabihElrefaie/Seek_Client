using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.Dtos.Settings
{
    /// <summary>
    /// Configuration settings for email service
    /// </summary>
    public class EmailSettings
    {
        public required string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public required string Username { get; set; }
        public required string Password { get; set; }
        public bool UseSsl { get; set; }
        public string DisplayName { get; set; } = "Seek";
        public required string FromEmail { get; set; }
        public required string AdminEmail { get; set; }
    }
}
