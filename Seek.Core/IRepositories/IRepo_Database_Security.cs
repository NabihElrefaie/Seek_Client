using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.IRepositories
{
    public interface IRepo_Database_Security
    {
        Task<bool> EncryptDatabaseAsync(string plainDbPath, string encryptedDbPath, string encryptionKey);
        Task<bool> DecryptDatabaseAsync(string encryptedDbPath, string plainDbPath, string encryptionKey);
    }
}
