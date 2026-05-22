using LSC.SmartCertify.Domain.Entities;

namespace LSC.SmartCertify.Application.Interfaces.Common
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }
        string? ExternalUserId { get; }
        string? Email { get; }
        string? DisplayName { get; }
        string? GivenName { get; }
        string? FamilyName { get; }
        Task<int> GetRequiredUserIdAsync();
        Task<UserProfile?> GetCurrentUserProfileAsync();
        Task<List<UserRole>> GetCurrentUserRolesAsync();
    }
}
