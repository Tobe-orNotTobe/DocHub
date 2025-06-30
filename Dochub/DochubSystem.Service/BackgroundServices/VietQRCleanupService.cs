using Azure;
using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;

namespace DochubSystem.Service.BackgroundServices
{
	public class VietQRCleanupService : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly ILogger<VietQRCleanupService> _logger;
		private readonly TimeSpan _period = TimeSpan.FromMinutes(5); // Run every 5 minutes

		public VietQRCleanupService(
			IServiceScopeFactory serviceScopeFactory,
			ILogger<VietQRCleanupService> logger)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			using var timer = new PeriodicTimer(_period);

			while (!stoppingToken.IsCancellationRequested &&
				   await timer.WaitForNextTickAsync(stoppingToken))
			{
				await ProcessExpiredPaymentRequestsAsync();
			}
		}

		private async Task ProcessExpiredPaymentRequestsAsync()
		{
			try
			{
				_logger.LogInformation("Starting VietQR payment requests cleanup at {Time}", DateTime.UtcNow);

				using var scope = _serviceScopeFactory.CreateScope();
				var vietQRPaymentService = scope.ServiceProvider.GetRequiredService<IVietQRPaymentService>();

				await vietQRPaymentService.ProcessExpiredPaymentRequestsAsync();

				_logger.LogInformation("Completed VietQR payment requests cleanup at {Time}", DateTime.UtcNow);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during VietQR payment requests cleanup");
			}
		}
	}
}