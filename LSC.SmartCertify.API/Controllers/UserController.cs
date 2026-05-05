using LSC.SmartCertify.Application.DTOs;
using LSC.SmartCertify.Application.Helper;
using LSC.SmartCertify.Application.Interfaces.Common;
using LSC.SmartCertify.Application.Interfaces.EmailNotification;
using LSC.SmartCertify.Application.Interfaces.Graph;
using LSC.SmartCertify.Application.Interfaces.ManageUser;
using LSC.SmartCertify.Application.Interfaces.Storage;
using LSC.SmartCertify.Application.Services.Graph;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace LSC.SmartCertify.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly GraphAuthService _graphAuthService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IEmailNotification emailNotification;
        private readonly IGraphService graphService;
        private readonly IStorageService storageService;
        private readonly ICurrentUserService currentUserService;
        private readonly IUserProfileService userProfileService;

        public UserController(GraphAuthService graphAuthService,
            IConfiguration configuration,
            HttpClient httpClient,
            IEmailNotification emailNotification,
            IGraphService graphService,
            IStorageService storageService,
            ICurrentUserService currentUserService,
            IUserProfileService userProfileService)
        {
            _graphAuthService = graphAuthService;
            _configuration = configuration;
            _httpClient = httpClient;
            this.emailNotification = emailNotification;
            this.graphService = graphService;
            this.storageService = storageService;
            this.currentUserService = currentUserService;
            this.userProfileService = userProfileService;
        }

        /*
         For below enpoint you will get error and here is the explanation.
        The error "/me request is only valid with delegated authentication flow" occurs because the /me endpoint in Microsoft Graph only works with delegated authentication (i.e., a user token obtained via interactive sign-in). However, your API is likely using application authentication (client credentials flow), which does not have a signed-in user context.

        Why This Error Happens?
        🔹 Delegated authentication flow (user-based access) → /me is valid.
        🔹 Application authentication flow (app-only access) → /me is not valid.

        Since your API is likely calling Graph using client credentials, Microsoft Graph does not associate any user with the request, so /me fails.
         */

        [HttpGet("me")]
        public async Task<IActionResult> GetUserInfo()
        {
            var accessToken = await _graphAuthService.GetAccessTokenAsync();
            var graphEndpoint = _configuration["AzureAdGraph:GraphEndpoint"] + "me";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync(graphEndpoint);
            var content = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode ? Ok(content) : StatusCode((int)response.StatusCode, content);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("User ID is required.");
            }


            var accessToken = await _graphAuthService.GetAccessTokenAsync();
            var graphEndpoint = $"{_configuration["AzureAdGraph:GraphEndpoint"]}users/{id}?$select=id,displayName,givenName,surname,userPrincipalName,mail,otherMails";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync(graphEndpoint);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorContent);
            }

            var content = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<AdB2CUserModel>(content);

            return Ok(user);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var accessToken = await _graphAuthService.GetAccessTokenAsync();
            //var graphEndpoint = _configuration["AzureAdGraph:GraphEndpoint"] + "users";
            var graphEndpoint = _configuration["AzureAdGraph:GraphEndpoint"] + "users?$select=id,givenName,surname,displayName,userPrincipalName,mail,otherMails";


            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync(graphEndpoint);
            var content = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode ? Ok(content) : StatusCode((int)response.StatusCode, content);
        }

        [HttpGet("users-with-email")]
        public async Task<IActionResult> GetUsersWithEmail()
        {
            var adb2cUserModel = await graphService.GetADB2CUsersAsync();
            return Ok(adb2cUserModel);
        }

        [HttpPatch("users/{id}/reset-password")]
        public async Task<IActionResult> ResetUserPassword(string id)
        {
            AdB2CUserModel userModel;

            // Step 1: Fetch user details (get the user model from OkObjectResult)
            var actionResult = await GetUserById(id); // This returns IActionResult
            if (actionResult is OkObjectResult okResult)
            {
                userModel = okResult.Value as AdB2CUserModel;
                if (userModel == null)
                {
                    return NotFound("User details not found.");
                }
                // Generate random strong password
                var newPassword = PasswordHelper.GenerateRandomPassword(16); // Default length is 16 characters

                var accessToken = await _graphAuthService.GetAccessTokenAsync();
                var graphEndpoint = $"{_configuration["AzureAdGraph:GraphEndpoint"]}users/{id}";

                var requestBody = new
                {
                    passwordProfile = new
                    {
                        forceChangePasswordNextSignIn = true,
                        password = newPassword // Set generated password
                    }
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await _httpClient.PatchAsync(graphEndpoint, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, errorContent);
                }


                // Send email with temporary password (you will handle this)
                await emailNotification.SendPasswordResetEmailForUser(userModel.OtherMails.FirstOrDefault(), newPassword);
            }

            return Ok("Password reset successfully. A temporary password has been sent to the user.");
        }

        [HttpPost("updateProfile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromForm] UpdateUserProfileModel model)
        {
            string? pictureUrl = null;
            var currentUser = await currentUserService.GetCurrentUserProfileAsync();
            if (currentUser is null)
            {
                return Unauthorized("Current user was not found in UserProfile.");
            }

            if (model.Picture != null)
            {
                using (var stream = new MemoryStream())
                {
                    await model.Picture.CopyToAsync(stream);

                    // Upload the byte array or stream to Azure Blob Storage
                    pictureUrl = await storageService.UploadAsync(stream.ToArray(),
                        $"{currentUser.UserId}_profile_picture.{model.Picture.FileName.Split('.').LastOrDefault()}");
                }

                // Update the profile picture URL in the database
                await userProfileService.UpdateUserProfilePicture(currentUser.UserId, pictureUrl);
            }


            return Ok(model);
        }


        [HttpGet("generate-sas")]
        [Authorize]
        public async Task<IActionResult> GenerateSasToken()
        {
            try
            {
                var userinfo = await currentUserService.GetCurrentUserProfileAsync();
                if (userinfo is null)
                {
                    return Unauthorized("Current user was not found in UserProfile.");
                }

                string sasToken = await storageService.GenerateSasTokenAsync(userinfo?.ProfileImageUrl ?? "");
                if (string.IsNullOrEmpty(sasToken))
                {
                    return StatusCode(500, "Failed to generate SAS token.");
                }

                return Ok(new
                {
                    FileUrl = $"{userinfo?.ProfileImageUrl}?{sasToken}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var userInfo = await currentUserService.GetCurrentUserProfileAsync();

            if (userInfo == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(userInfo.ProfileImageUrl))
            {
                string sasToken =
                await storageService.GenerateSasTokenAsync(userInfo.ProfileImageUrl ?? "");

                if (string.IsNullOrEmpty(sasToken))
                {
                    return StatusCode(500, "Failed to generate SAS token.");
                }

                userInfo.ProfileImageUrl = $"{userInfo.ProfileImageUrl}{sasToken}";
            }

            return Ok(userInfo);
        }

        [HttpGet("bootstrap")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CurrentUserBootstrapModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUserBootstrap()
        {
            var userInfo = await currentUserService.GetCurrentUserProfileAsync();

            if (userInfo == null)
            {
                return NotFound("Current user was not found in UserProfile.");
            }

            var roles = await currentUserService.GetCurrentUserRolesAsync();
            var profileImageUrl = userInfo.ProfileImageUrl;

            if (!string.IsNullOrEmpty(profileImageUrl))
            {
                string sasToken =
                await storageService.GenerateSasTokenAsync(profileImageUrl);

                if (string.IsNullOrEmpty(sasToken))
                {
                    return StatusCode(500, "Failed to generate SAS token.");
                }

                profileImageUrl = $"{profileImageUrl}{sasToken}";
            }

            return Ok(new CurrentUserBootstrapModel
            {
                UserId = userInfo.UserId,
                DisplayName = userInfo.DisplayName,
                FirstName = userInfo.FirstName,
                LastName = userInfo.LastName,
                Email = userInfo.Email,
                ProfileImageUrl = profileImageUrl,
                Roles = roles.Select(role => new UserRoleModel
                {
                    UserRoleId = role.UserRoleId,
                    RoleId = role.RoleId,
                    RoleName = role.Role.RoleName,
                    UserId = role.UserId
                }).ToList()
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserProfile([FromRoute] int id)
        {
            var userid = await currentUserService.GetRequiredUserIdAsync();
            id = userid;
            var userInfo = await userProfileService.GetUserInfoAsync(id);

            if (userInfo == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(userInfo?.ProfileImageUrl))
            {
                string sasToken =
                await storageService.GenerateSasTokenAsync(userInfo?.ProfileImageUrl ?? "");

                if (string.IsNullOrEmpty(sasToken))
                {
                    return StatusCode(500, "Failed to generate SAS token.");
                }

                userInfo.ProfileImageUrl = $"{userInfo?.ProfileImageUrl}{sasToken}";
            }

            return Ok(userInfo);
        }
    }

}
