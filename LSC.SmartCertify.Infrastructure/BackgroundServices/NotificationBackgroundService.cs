using LSC.SmartCertify.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LSC.SmartCertify.Infrastructure.BackgroundServices
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<NotificationBackgroundService> _logger;

        public NotificationBackgroundService(IServiceScopeFactory serviceScopeFactory, 
            ILogger<NotificationBackgroundService> logger)
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


                        // Retrieve new notifications that are not processed
                        var newNotifications = dbContext.Notifications
                            .Where(n => n.IsActive && n.ScheduledSendTime >= DateTime.Now.AddHours(-1)
                            && n.ScheduledSendTime < DateTime.Now.AddHours(1))
                            .ToList();

                        if (newNotifications.Any())
                        {
                            var users = dbContext.UserProfiles.ToList();

                            foreach (var notification in newNotifications)
                            {
                                var userNotificationExists = dbContext.UserNotifications
                                    .Any(un => un.NotificationId == notification.NotificationId);
                                if (!userNotificationExists)
                                {
                                    foreach (var user in users)
                                    {
                                        dbContext.UserNotifications.Add(new UserNotification
                                        {
                                            CreatedOn = DateTime.Now,
                                            NotificationId = notification.NotificationId,
                                            UserId = user.UserId,
                                            EmailContent = notification.Content,
                                            EmailSubject = notification.Subject,
                                            NotificationSent = false,
                                            SentOn = null
                                        });
                                    }
                                }

                                notification.IsActive = false;

                            }

                            await dbContext.SaveChangesAsync();
                            _logger.LogInformation($"Processed {newNotifications.Count} new notifications.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in notification background service");
                }

                // Wait for an hour before next execution
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
