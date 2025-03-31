using LSC.SmartCertify.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.Interfaces.Graph
{
    public interface IGraphService
    {
        public Task<List<AdB2CUserModel>> GetADB2CUsersAsync();
    }
}
