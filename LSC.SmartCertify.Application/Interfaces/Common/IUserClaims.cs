using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.Interfaces.Common
{
    public interface IUserClaims
    {
        string GetCurrentUserEmail();
        string GetCurrentUserId();
        List<string> GetUserRoles();
        int GetUserId();
    }
}
