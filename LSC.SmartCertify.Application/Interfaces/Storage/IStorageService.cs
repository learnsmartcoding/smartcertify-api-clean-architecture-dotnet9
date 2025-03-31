using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.Interfaces.Storage
{
    public interface IStorageService
    {
        Task<string> GenerateSasTokenAsync(string fileName);
        Task<string> UploadAsync(byte[] fileData, string fileName, string containerName = "");
    }
}
