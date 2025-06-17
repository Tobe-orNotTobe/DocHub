using DochubSystem.RepositoryContract.Interfaces;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DochubSystem.Service.BackgroundServices
{
	public class AppointmentReminderService : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly ILogger<AppointmentReminderService> _logger;
		private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(15); // Check every 15 minutes

		public AppointmentReminderService(
			IServiceScopeFactory serviceScopeFactory,
			ILogger<AppointmentReminderService> logger)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Appointment Reminder Service started");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using (var scope = _serviceScopeFactory.CreateScope())
					{
						await ProcessAppointmentRemindersAsync(scope);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error occurred in appointment reminder service");
				}

				await Task.Delay(_checkInterval, stoppingToken);
			}

			_logger.LogInformation("Appointment Reminder Service stopped");
		}

		private async Task ProcessAppointmentRemindersAsync(IServiceScope scope)
		{
			try
			{
				var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
				var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

				// Find appointments that need reminders
				var now = DateTime.UtcNow;
				var oneHourLater = now.AddHours(1);
				var thirtyMinutesLater = now.AddMinutes(30);

				// Get appointments that need 1-hour reminders
				var appointmentsFor1HourReminder = await unitOfWork.Appointments.GetAllAsync(
					a => a.AppointmentDate >= oneHourLater &&
						 a.AppointmentDate <= oneHourLater.AddMinutes(15) &&
						 a.Status == "pending");

				foreach (var appointment in appointmentsFor1HourReminder)
				{
					await notificationService.SendAppointmentReminderAsync(appointment.AppointmentId, "patient");
					await notificationService.SendAppointmentReminderAsync(appointment.AppointmentId, "doctor");
				}

				// Get appointments that need 30-minute reminders
				var appointmentsFor30MinReminder = await unitOfWork.Appointments.GetAllAsync(
					a => a.AppointmentDate >= thirtyMinutesLater &&
						 a.AppointmentDate <= thirtyMinutesLater.AddMinutes(15) &&
						 a.Status == "pending");

				foreach (var appointment in appointmentsFor30MinReminder)
				{
					await notificationService.SendAppointmentReminderAsync(appointment.AppointmentId, "patient");
					await notificationService.SendAppointmentReminderAsync(appointment.AppointmentId, "doctor");
				}

				_logger.LogDebug($"Processed {appointmentsFor1HourReminder.Count()} 1-hour reminders and {appointmentsFor30MinReminder.Count()} 30-minute reminders");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing appointment reminders");
			}
		}
	}
}
