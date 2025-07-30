using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models
{
    public class EncryptionConfig
    {
        public string Key { get; set; } // Base64 encoded encryption key
        public string IV { get; set; }  // Base64-encoded
        public string Algorithm { get; set; } = "AES-256-CBC";
        public int KeySize { get; set; } = 256;
        public int BlockSize { get; set; } = 128;
    }
}
