using LSC.SmartCertify.Application.DTOs;
using LSC.SmartCertify.Application.Interfaces.Common;
using LSC.SmartCertify.Application.Interfaces.Graph;
using LSC.SmartCertify.Application.Interfaces.ManageUser;
using LSC.SmartCertify.Application.Interfaces.Storage;
using Microsoft.AspNetCore.Mvc;

namespace LSC.SmartCertify.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IStorageService storageService;
        private readonly IUserClaims userClaims;
        private readonly IUserProfileService userProfileService;
        private readonly IGraphService graphService;

        public UserController(
            IStorageService storageService,
            IUserClaims userClaims,
            IUserProfileService userProfileService,
            IGraphService graphService)
        {
            this.storageService = storageService;
            this.userClaims = userClaims;
            this.userProfileService = userProfileService;
            this.graphService = graphService;
        }

        [HttpPost("updateProfile")]

        public async Task<IActionResult> UpdateUserProfile([FromForm] UpdateUserProfileModel model)
        {
            string pictureUrl = null;

            if (model.Picture != null)
            {
                using (var stream = new MemoryStream())
                {
                    await model.Picture.CopyToAsync(stream);

                    // Upload the byte array or stream to Azure Blob Storage
                    pictureUrl = await storageService.UploadAsync(stream.ToArray(),
                        $"{model.UserId}_profile_picture.{model.Picture.FileName.Split('.').LastOrDefault()}");
                }

                // Update the profile picture URL in the database
                await userProfileService.UpdateUserProfilePicture(model.UserId, pictureUrl);
            }


            return Ok(model);
        }


        [HttpGet("generate-sas")]
        public async Task<IActionResult> GenerateSasToken()
        {
            try
            {
                var userid = userClaims.GetUserId();
                var userinfo = await userProfileService.GetUserInfoAsync(userid);

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

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserModel))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserProfile([FromRoute] int id)
        {
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

        [HttpGet("users-with-email")]
        public async Task<IActionResult> GetUsersWithEmail()
        {
            var adb2cUserModel = await graphService.GetADB2CUsersAsync();
            return Ok(adb2cUserModel);
        }


    }

}
