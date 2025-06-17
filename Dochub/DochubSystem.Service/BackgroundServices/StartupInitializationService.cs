using DochubSystem.ServiceContract.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Service.BackgroundServices
{
	public class StartupInitializationService : IHostedService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly ILogger<StartupInitializationService> _logger;

		public StartupInitializationService(
			IServiceScopeFactory serviceScopeFactory,
			ILogger<StartupInitializationService> logger)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_logger = logger;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting application initialization...");

			try
			{
				using (var scope = _serviceScopeFactory.CreateScope())
				{
					// Seed default notification templates
					var templateService = scope.ServiceProvider.GetRequiredService<INotificationTemplateService>();
					await templateService.SeedDefaultTemplatesAsync();
					_logger.LogInformation("Default notification templates seeded successfully");

				}

				_logger.LogInformation("Application initialization completed successfully");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during application initialization");
				throw; // This will prevent the application from starting if initialization fails
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Application shutdown - initialization service stopped");
			return Task.CompletedTask;
		}
	}
}
