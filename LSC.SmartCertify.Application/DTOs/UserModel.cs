using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSC.SmartCertify.Application.DTOs
{
    public class AdB2CUserModel
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
        public List<AdB2CUserModel> Users { get; set; }
    }

    public class UpdateUserProfileModel
    {
        public required int UserId { get; set; }
        public IFormFile? Picture { get; set; }
    }

    public class UserModel
    {
        public int UserId { get; set; }

        public string DisplayName { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string AdObjId { get; set; } = null!;
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
        public required List<UserRoleModel> UserRoleModel { get; set; }
    }
    public class UserRoleModel
    {
        public int UserRoleId { get; set; }

        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public int UserId { get; set; }
    }


}
