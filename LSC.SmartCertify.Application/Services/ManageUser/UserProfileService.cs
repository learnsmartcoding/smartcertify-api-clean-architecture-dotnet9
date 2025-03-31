using LSC.SmartCertify.Application.Interfaces.ManageUser;
using LSC.SmartCertify.Domain.Entities;

namespace LSC.SmartCertify.Application.Services.ManageUser
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository userProfileRepository;

        public UserProfileService(IUserProfileRepository userProfileRepository)
        {
            this.userProfileRepository = userProfileRepository;
        }

        public async Task UpdateUserProfilePicture(int userId, string pictureUrl)
        {
            await userProfileRepository.UpdateUserProfilePicture(userId, pictureUrl);
        }      

        public Task<UserProfile?> GetUserInfoAsync(int userId)
        {
            return userProfileRepository.GetUserInfoAsync(userId);
        }
    }
}
