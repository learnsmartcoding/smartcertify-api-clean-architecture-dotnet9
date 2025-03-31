using LSC.SmartCertify.Application.Interfaces.Graph;
using LSC.SmartCertify.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LSC.SmartCertify.Infrastructure.BackgroundServices
{
    public class OnboardUserBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public OnboardUserBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<NotificationBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running notification background service...");

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<SmartCertifyContext>();
                        var graphService = scope.ServiceProvider.GetRequiredService<IGraphService>();
                        
                        var adb2cUsers= await graphService.GetADB2CUsersAsync();
                        if (adb2cUsers.Any())
                        {
                            foreach (var user in adb2cUsers)
                            {
                                if (user.OtherMails.Any())
                                {
                                    var userExists = dbContext.UserProfiles
                                        .Any(u => u.Email == user.OtherMails.FirstOrDefault());
                                   
                                    if (!userExists)
                                    {
                                        var userProfile = new UserProfile
                                        {
                                            Email = user.OtherMails.FirstOrDefault(),
                                            FirstName = Convert.ToString(user.GivenName),
                                            LastName = user.Surname ?? "",
                                            DisplayName = user.DisplayName,
                                            AdObjId = "",
                                            ProfileImageUrl = ""
                                        };

                                        dbContext.UserProfiles.Add(userProfile);
                                    }
                                }
                            }
                            await dbContext.SaveChangesAsync();
                            _logger.LogInformation($"Processed {adb2cUsers.Count} new users.");
                        }

                      
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in onboardUser background service");
                }

                // Wait for an hour before next execution
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
