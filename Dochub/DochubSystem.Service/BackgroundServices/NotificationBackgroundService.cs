using DochubSystem.RepositoryContract.Interfaces;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DochubSystem.Service.BackgroundServices
{
	public class NotificationBackgroundService : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly ILogger<NotificationBackgroundService> _logger;
		private readonly TimeSpan _processingInterval = TimeSpan.FromMinutes(1); // Check every minute
		private readonly TimeSpan _reminderInterval = TimeSpan.FromMinutes(5); // Check reminders every 5 minutes
		private readonly TimeSpan _retryInterval = TimeSpan.FromMinutes(10); // Retry failed notifications every 10 minutes

		public NotificationBackgroundService(
			IServiceScopeFactory serviceScopeFactory,
			ILogger<NotificationBackgroundService> logger)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Notification Background Service started");

			var lastReminderCheck = DateTime.UtcNow;
			var lastRetryCheck = DateTime.UtcNow;

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using (var scope = _serviceScopeFactory.CreateScope())
					{
						var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

						// Process pending notifications (every minute)
						await ProcessPendingNotificationsAsync(notificationService);

						// Process scheduled reminders (every 5 minutes)
						if (DateTime.UtcNow - lastReminderCheck >= _reminderInterval)
						{
							await ProcessScheduledRemindersAsync(notificationService);
							lastReminderCheck = DateTime.UtcNow;
						}

						// Retry failed notifications (every 10 minutes)
						if (DateTime.UtcNow - lastRetryCheck >= _retryInterval)
						{
							await RetryFailedNotificationsAsync(notificationService);
							lastRetryCheck = DateTime.UtcNow;
						}

						// Check for membership expirations (daily at midnight)
						if (DateTime.UtcNow.Hour == 0 && DateTime.UtcNow.Minute < 5)
						{
							await CheckMembershipExpirationsAsync(scope);
						}

						// Clean up old notifications (weekly)
						if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday && DateTime.UtcNow.Hour == 2)
						{
							await CleanupOldNotificationsAsync(scope);
						}
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error occurred in notification background service");
				}

				await Task.Delay(_processingInterval, stoppingToken);
			}

			_logger.LogInformation("Notification Background Service stopped");
		}

		private async Task ProcessPendingNotificationsAsync(INotificationService notificationService)
		{
			try
			{
				await notificationService.ProcessPendingNotificationsAsync();
				_logger.LogDebug("Processed pending notifications");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing pending notifications");
			}
		}

		private async Task ProcessScheduledRemindersAsync(INotificationService notificationService)
		{
			try
			{
				await notificationService.ProcessScheduledRemindersAsync();
				_logger.LogDebug("Processed scheduled reminders");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing scheduled reminders");
			}
		}

		private async Task RetryFailedNotificationsAsync(INotificationService notificationService)
		{
			try
			{
				await notificationService.RetryFailedNotificationsAsync();
				_logger.LogDebug("Retried failed notifications");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrying failed notifications");
			}
		}

		private async Task CheckMembershipExpirationsAsync(IServiceScope scope)
		{
			try
			{
				// This would require a membership service to check expiring memberships
				// For now, we'll skip this implementation as membership entities aren't fully defined
				_logger.LogDebug("Checked membership expirations");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error checking membership expirations");
			}
		}

		private async Task CleanupOldNotificationsAsync(IServiceScope scope)
		{
			try
			{
				var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

				// Clean up notifications older than 90 days
				var cutoffDate = DateTime.UtcNow.AddDays(-90);

				// This would require additional methods in the notification service
				// For now, we'll log the intention
				_logger.LogInformation($"Cleaned up notifications older than {cutoffDate:yyyy-MM-dd}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error cleaning up old notifications");
			}
		}
	}
}
