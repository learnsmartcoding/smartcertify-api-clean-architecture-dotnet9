using LSC.SmartCertify.Application.Interfaces.Common;
using LSC.SmartCertify.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LSC.SmartCertify.Infrastructure.Security
{
    public class CurrentUserService : ICurrentUserService
    {
        private const string DefaultRoleName = "Customer";
        private const string DefaultSmartAppName = "SmartCertify";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SmartCertifyContext _context;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, SmartCertifyContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

        public string? ExternalUserId =>
            GetClaimValue(
                "oid",
                "http://schemas.microsoft.com/identity/claims/objectidentifier");

        public string? Email =>
            GetClaimValue(
                "email",
                "emails",
                "preferred_username",
                ClaimTypes.Email,
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");

        public string? DisplayName =>
            GetClaimValue(
                "name",
                ClaimTypes.Name,
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");

        public string? GivenName =>
            GetClaimValue(
                "given_name",
                ClaimTypes.GivenName,
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");

        public string? FamilyName =>
            GetClaimValue(
                "family_name",
                ClaimTypes.Surname,
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname");

        public async Task<int> GetRequiredUserIdAsync()
        {
            var userProfile = await GetCurrentUserProfileAsync();

            if (userProfile is null)
            {
                throw new UnauthorizedAccessException("Current user was not found in UserProfile.");
            }

            return userProfile.UserId;
        }

        public async Task<UserProfile?> GetCurrentUserProfileAsync()
        {
            if (!IsAuthenticated || string.IsNullOrWhiteSpace(Email))
            {
                return null;
            }

            var normalizedEmail = Email.Trim();
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(user => user.Email == normalizedEmail);

            if (userProfile is not null)
            {
                await EnsureUserRoleAsync(userProfile.UserId);
                return userProfile;
            }

            userProfile = new UserProfile
            {
                DisplayName = GetRequiredValue(DisplayName, normalizedEmail),
                FirstName = GetRequiredValue(GivenName, normalizedEmail),
                LastName = GetRequiredValue(FamilyName, normalizedEmail),
                Email = normalizedEmail,
                AdObjId = ExternalUserId ?? string.Empty,
                ProfileImageUrl = string.Empty,
                CreatedOn = DateTime.UtcNow
            };

            await _context.UserProfiles.AddAsync(userProfile);
            await _context.SaveChangesAsync();
            await EnsureUserRoleAsync(userProfile.UserId);

            return userProfile;
        }

        public async Task<List<UserRole>> GetCurrentUserRolesAsync()
        {
            var userProfile = await GetCurrentUserProfileAsync();

            if (userProfile is null)
            {
                return new List<UserRole>();
            }

            return await _context.UserRoles
                .AsNoTracking()
                .Include(userRole => userRole.Role)
                .Where(userRole => userRole.UserId == userProfile.UserId)
                .ToListAsync();
        }

        private async Task EnsureUserRoleAsync(int userId)
        {
            var hasRole = await _context.UserRoles.AnyAsync(userRole => userRole.UserId == userId);
            if (hasRole)
            {
                return;
            }

            var customerRole = await _context.Roles
                .FirstOrDefaultAsync(role => role.RoleName == DefaultRoleName);

            if (customerRole is null)
            {
                throw new InvalidOperationException($"Default role '{DefaultRoleName}' was not found.");
            }

            var smartApp = await _context.SmartApps
                .FirstOrDefaultAsync(app => app.AppName == DefaultSmartAppName);

            if (smartApp is null)
            {
                throw new InvalidOperationException($"Default smart app '{DefaultSmartAppName}' was not found.");
            }

            await _context.UserRoles.AddAsync(new UserRole
            {
                RoleId = customerRole.RoleId,
                UserId = userId,
                SmartAppId = smartApp.SmartAppId
            });

            await _context.SaveChangesAsync();
        }

        private string? GetClaimValue(params string[] claimTypes)
        {
            if (User is null)
            {
                return null;
            }

            foreach (var claimType in claimTypes)
            {
                var value = User.FindFirst(claimType)?.Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        private static string GetRequiredValue(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }
    }
}
