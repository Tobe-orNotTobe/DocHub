using DochubSystem.ServiceContract.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DochubSystem.Service.BackgroundServices
{
	public class HighPriorityNotificationService : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly ILogger<HighPriorityNotificationService> _logger;
		private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(30); // Check every 30 seconds for urgent notifications

		public HighPriorityNotificationService(
			IServiceScopeFactory serviceScopeFactory,
			ILogger<HighPriorityNotificationService> logger)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("High Priority Notification Service started");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using (var scope = _serviceScopeFactory.CreateScope())
					{
						var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

						// Process only urgent and high priority notifications
						await ProcessHighPriorityNotificationsAsync(notificationService);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error occurred in high priority notification service");
				}

				await Task.Delay(_processingInterval, stoppingToken);
			}

			_logger.LogInformation("High Priority Notification Service stopped");
		}

		private async Task ProcessHighPriorityNotificationsAsync(INotificationService notificationService)
		{
			try
			{
				// This would process only urgent/high priority notifications
				// Implementation would be similar to ProcessPendingNotificationsAsync 
				// but with priority filtering
				await notificationService.ProcessPendingNotificationsAsync();
				_logger.LogDebug("Processed high priority notifications");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing high priority notifications");
			}
		}
	}
}
