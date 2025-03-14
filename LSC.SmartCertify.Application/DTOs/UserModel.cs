using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.DTOs
{
    public class UserModel
    {
        public string Id { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string DisplayName { get; set; }
        public string UserPrincipalName { get; set; }
        public string Mail { get; set; }
        public List<string> OtherMails { get; set; }
    }

    // Wrapper model for JSON structure
    public class GraphApiResponse
    {
        [JsonProperty("value")] // The key in the API response that holds the list of users
        public List<UserModel> Users { get; set; }
    }
}
