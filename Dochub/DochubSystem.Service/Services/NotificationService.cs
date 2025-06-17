using AutoMapper;
using DochubSystem.Data.Constants;
using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using DochubSystem.RepositoryContract.Interfaces;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DochubSystem.Service.Services
{
	public class NotificationService : INotificationService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IMapper _mapper;
		private readonly IEmailService _emailService;
		private readonly ILogger<NotificationService> _logger;

		public NotificationService(
			IServiceScopeFactory serviceScopeFactory,
			IMapper mapper,
			IEmailService emailService,
			ILogger<NotificationService> logger)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_mapper = mapper;
			_emailService = emailService;
			_logger = logger;
		}

		#region Core Notification Methods

		public async Task<bool> SendNotificationAsync(SendNotificationRequestDTO request)
		{
			try
			{
				using var scope = _serviceScopeFactory.CreateScope();
				var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

				var template = await unitOfWork.NotificationTemplates.GetByTypeAsync(request.NotificationType);
				if (template == null)
				{
					_logger.LogWarning($"Template not found for notification type: {request.NotificationType}");
					return false;
				}

				var user = await unitOfWork.Users.GetUserByIdAsync(request.UserId);
				if (user == null)
				{
					_logger.LogWarning($"User not found: {request.UserId}");
					return false;
				}

				// Render notification content
				var notificationBody = await RenderNotificationContentAsync(template.NotificationBody, request.Parameters ?? new Dictionary<string, object>());
				var emailBody = template.RequiresEmail ? await RenderEmailContentAsync(template.EmailBody, request.Parameters ?? new Dictionary<string, object>()) : null;
				var subject = await RenderNotificationContentAsync(template.Subject, request.Parameters ?? new Dictionary<string, object>());

				// Create in-app notification if required
				if (template.RequiresInApp)
				{
					var notification = new Notification
					{
						UserId = request.UserId,
						Title = subject,
						Message = notificationBody,
						Type = request.NotificationType,
						Priority = request.Priority ?? template.Priority,
						Status = NotificationStatus.UNREAD,
						CreatedAt = DateTime.UtcNow,
						AppointmentId = ExtractAppointmentId(request.Parameters),
						DoctorId = ExtractDoctorId(request.Parameters),
						RelatedEntityType = ExtractRelatedEntityType(request.Parameters),
						RelatedEntityId = ExtractRelatedEntityId(request.Parameters),
						ActionUrl = ExtractActionUrl(request.Parameters)
					};

					await unitOfWork.Notifications.AddAsync(notification);
				}

				// Queue email notification if required
				if (template.RequiresEmail && !string.IsNullOrEmpty(user.Email))
				{
					var queueItem = new NotificationQueue
					{
						TemplateId = template.TemplateId,
						UserId = request.UserId,
						Subject = subject,
						NotificationBody = notificationBody,
						EmailBody = emailBody,
						NotificationType = request.NotificationType,
						Priority = request.Priority ?? template.Priority,
						Status = NotificationStatus.PENDING,
						ScheduledAt = request.ScheduledAt ?? DateTime.UtcNow,
						CreatedAt = DateTime.UtcNow,
						MetaData = request.Parameters != null ? JsonSerializer.Serialize(request.Parameters) : null
					};

					await unitOfWork.NotificationQueues.AddAsync(queueItem);
				}

				await unitOfWork.CompleteAsync();

				// Save to history
				await SaveToHistoryAsync(request.UserId, template.TemplateId, subject, notificationBody, emailBody, request.NotificationType, request.Parameters);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error sending notification: {request.NotificationType} to user {request.UserId}");
				return false;
			}
		}

		public async Task<bool> SendBulkNotificationAsync(BulkNotificationDTO request)
		{
			try
			{
				var tasks = request.UserIds.Select(userId =>
					SendNotificationAsync(new SendNotificationRequestDTO
					{
						UserId = userId,
						NotificationType = request.NotificationType,
						Parameters = request.Parameters,
						ScheduledAt = request.ScheduledAt,
						Priority = request.Priority
					}));

				var results = await Task.WhenAll(tasks);
				return results.All(r => r);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error sending bulk notification: {request.NotificationType}");
				return false;
			}
		}

		public async Task<bool> ScheduleNotificationAsync(string userId, string notificationType, DateTime scheduledAt, Dictionary<string, object>? parameters = null)
		{
			return await SendNotificationAsync(new SendNotificationRequestDTO
			{
				UserId = userId,
				NotificationType = notificationType,
				ScheduledAt = scheduledAt,
				Parameters = parameters
			});
		}

		#endregion

		#region Notification Management

		public async Task<NotificationResponseDTO> GetUserNotificationsAsync(string userId, GetNotificationsRequestDTO request)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			var notifications = await unitOfWork.Notifications.GetUserNotificationsAsync(userId, request);
			var totalCount = await unitOfWork.Notifications.CountAsync(n => n.UserId == userId);

			var notificationDTOs = notifications.Select(n => new NotificationDTO
			{
				NotificationId = n.NotificationId,
				UserId = n.UserId,
				Title = n.Title,
				Message = n.Message,
				Type = n.Type,
				Priority = n.Priority,
				Status = n.Status,
				CreatedAt = n.CreatedAt,
				ReadAt = n.ReadAt,
				AppointmentId = n.AppointmentId,
				DoctorId = n.DoctorId,
				RelatedEntityType = n.RelatedEntityType,
				RelatedEntityId = n.RelatedEntityId,
				ActionUrl = n.ActionUrl,
				DoctorName = n.Doctor?.User?.FullName,
				AppointmentDate = n.Appointment?.AppointmentDate.ToString("dd/MM/yyyy HH:mm")
			}).ToList();

			var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

			return new NotificationResponseDTO
			{
				Notifications = notificationDTOs,
				TotalCount = totalCount,
				Page = request.Page,
				PageSize = request.PageSize,
				TotalPages = totalPages,
				HasNextPage = request.Page < totalPages,
				HasPreviousPage = request.Page > 1
			};
		}

		public async Task<NotificationDTO> GetNotificationByIdAsync(int notificationId)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			var notification = await unitOfWork.Notifications.GetAsync(
				n => n.NotificationId == notificationId,
				includeProperties: "Appointment,Doctor.User");

			return notification != null ? _mapper.Map<NotificationDTO>(notification) : null;
		}

		public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			return await unitOfWork.Notifications.MarkAsReadAsync(notificationId, userId);
		}

		public async Task<bool> MarkMultipleAsReadAsync(BulkMarkReadDTO request, string userId)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			return await unitOfWork.Notifications.MarkMultipleAsReadAsync(request.NotificationIds, userId);
		}

		public async Task<bool> MarkAllAsReadAsync(string userId)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			return await unitOfWork.Notifications.MarkAllAsReadAsync(userId);
		}

		public async Task<bool> DeleteNotificationAsync(int notificationId, string userId)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			var notification = await unitOfWork.Notifications.GetAsync(n => n.NotificationId == notificationId && n.UserId == userId);
			if (notification == null) return false;

			await unitOfWork.Notifications.DeleteAsync(notification);
			await unitOfWork.CompleteAsync();
			return true;
		}

		#endregion

		#region Statistics

		public async Task<NotificationStatisticsDTO> GetUserStatisticsAsync(string userId)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			return await unitOfWork.Notifications.GetUserNotificationStatisticsAsync(userId);
		}

		public async Task<int> GetUnreadCountAsync(string userId)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			return await unitOfWork.Notifications.GetUnreadCountAsync(userId);
		}

		#endregion

		#region Appointment Notifications

		public async Task<bool> SendAppointmentCreatedNotificationAsync(int appointmentId)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			var appointment = await unitOfWork.Appointments.GetAsync(
				a => a.AppointmentId == appointmentId,
				includeProperties: "User,Doctor.User");

			if (appointment == null) return false;

			// Notify patient
			var patientParams = new Dictionary<string, object>
			{
				["AppointmentId"] = appointmentId,
				["PatientName"] = appointment.User.FullName ?? "",
				["DoctorName"] = appointment.Doctor.User?.FullName ?? "",
				["AppointmentDate"] = appointment.AppointmentDate.ToString("dd/MM/yyyy HH:mm"),
				["ActionUrl"] = $"/appointments/{appointmentId}"
			};

			var patientResult = await SendNotificationAsync(new SendNotificationRequestDTO
			{
				UserId = appointment.UserId,
				NotificationType = NotificationTypes.APPOINTMENT_BOOKED,
				Parameters = patientParams
			});

			// Notify doctor
			var doctorParams = new Dictionary<string, object>
			{
				["AppointmentId"] = appointmentId,
				["PatientName"] = appointment.User.FullName ?? "",
				["DoctorName"] = appointment.Doctor.User?.FullName ?? "",
				["AppointmentDate"] = appointment.AppointmentDate.ToString("dd/MM/yyyy HH:mm"),
				["Symptoms"] = appointment.Symptoms ?? "Không có",
				["ActionUrl"] = $"/doctor/appointments/{appointmentId}"
			};

			var doctorResult = await SendNotificationAsync(new SendNotificationRequestDTO
			{
				UserId = appointment.Doctor.UserId,
				NotificationType = NotificationTypes.NEW_APPOINTMENT,
				Parameters = doctorParams
			});

			return patientResult && doctorResult;
		}

		public async Task<bool> SendAppointmentUpdatedNotificationAsync(int appointmentId, string updateType)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			var appointment = await unitOfWork.Appointments.GetAsync(
				a => a.AppointmentId == appointmentId,
				includeProperties: "User,Doctor.User");

			if (appointment == null) return false;

			var parameters = new Dictionary<string, object>
			{
				["AppointmentId"] = appointmentId,
				["PatientName"] = appointment.User.FullName ?? "",
				["DoctorName"] = appointment.Doctor.User?.FullName ?? "",
				["AppointmentDate"] = appointment.AppointmentDate.ToString("dd/MM/yyyy HH:mm"),
				["UpdateType"] = updateType,
				["ActionUrl"] = $"/appointments/{appointmentId}"
			};

			// Notify patient
			var patientResult = await SendNotificationAsync(new SendNotificationRequestDTO
			{
				UserId = appointment.UserId,
				NotificationType = NotificationTypes.APPOINTMENT_MODIFIED,
				Parameters = parameters
			});

			// Notify doctor
			var doctorResult = await SendNotificationAsync(new SendNotificationRequestDTO
			{
				UserId = appointment.Doctor.UserId,
				NotificationType = NotificationTypes.APPOINTMENT_UPDATED,
				Parameters = parameters
			});

			return patientResult && doctorResult;
		}

		public async Task<bool> SendAppointmentCancelledNotificationAsync(int appointmentId, string cancellationReason)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			var appointment = await unitOfWork.Appointments.GetAsync(
				a => a.AppointmentId == appointmentId,
				includeProperties: "User,Doctor.User");

			if (appointment == null) return false;

			var parameters = new Dictionary<string, object>
			{
				["AppointmentId"] = appointmentId,
				["PatientName"] = appointment.User.FullName ?? "",
				["DoctorName"] = appointment.Doctor.User?.FullName ?? "",
				["AppointmentDate"] = appointment.AppointmentDate.ToString("dd/MM/yyyy HH:mm"),
				["CancellationReason"] = cancellationReason,
				["ActionUrl"] = $"/appointments"
			};

			// Notify patient
			var patientResult = await SendNotificationAsync(new SendNotificationRequestDTO
			{
				UserId = appointment.UserId,
				NotificationType = NotificationTypes.APPOINTMENT_CANCELLED,
				Parameters = parameters
			});

			// Notify doctor
			var doctorResult = await SendNotificationAsync(new SendNotificationRequestDTO
			{
				UserId = appointment.Doctor.UserId,
				NotificationType = NotificationTypes.APPOINTMENT_CANCELLED,
				Parameters = parameters
			});

			return patientResult && doctorResult;
		}

		public async Task<bool> SendAppointmentReminderAsync(int appointmentId, string reminderType)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			var appointment = await unitOfWork.Appointments.GetAsync(
				a => a.AppointmentId == appointmentId,
				includeProperties: "User,Doctor.User");

			if (appointment == null) return false;

			var parameters = new Dictionary<string, object>
			{
				["AppointmentId"] = appointmentId,
				["PatientName"] = appointment.User.FullName ?? "",
				["DoctorName"] = appointment.Doctor.User?.FullName ?? "",
				["AppointmentDate"] = appointment.AppointmentDate.ToString("dd/MM/yyyy HH:mm"),
				["ReminderType"] = reminderType,
				["ActionUrl"] = $"/appointments/{appointmentId}"
			};

			var notificationType = reminderType == "patient"
				? NotificationTypes.APPOINTMENT_REMINDER_PATIENT
				: NotificationTypes.APPOINTMENT_REMINDER_DOCTOR;

			var userId = reminderType == "patient" ? appointment.UserId : appointment.Doctor.UserId;

			return await SendNotificationAsync(new SendNotificationRequestDTO
			{
				UserId = userId,
				NotificationType = notificationType,
				Parameters = parameters,
				Priority = NotificationPriority.HIGH
			});
		}

		#endregion

		#region Membership Notifications

		public async Task<bool> SendMembershipExpiringNotificationAsync(string userId, DateTime expirationDate)
		{
			var parameters = new Dictionary<string, object>
			{
				["ExpirationDate"] = expirationDate.ToString("dd/MM/yyyy"),
				["DaysLeft"] = (expirationDate - DateTime.UtcNow).Days,
				["ActionUrl"] = "/membership/renew"
			};

			return await SendNotificationAsync(new SendNotificationRequestDTO
			{
				UserId = userId,
				NotificationType = NotificationTypes.MEMBERSHIP_EXPIRING,
				Parameters = parameters,
				Priority = NotificationPriority.HIGH
			});
		}

		public async Task<bool> SendMembershipRenewedNotificationAsync(string userId)
		{
			var parameters = new Dictionary<string, object>
			{
				["ActionUrl"] = "/membership"
			};

			return await SendNotificationAsync(new SendNotificationRequestDTO
			{
				UserId = userId,
				NotificationType = NotificationTypes.MEMBERSHIP_RENEWED,
				Parameters = parameters
			});
		}

		#endregion

		#region Doctor Notifications

		public async Task<bool> SendDoctorNotificationAsync(int doctorId, string notificationType, Dictionary<string, object>? parameters = null)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			var doctor = await unitOfWork.Doctors.GetAsync(d => d.DoctorId == doctorId, includeProperties: "User");
			if (doctor == null) return false;

			return await SendNotificationAsync(new SendNotificationRequestDTO
			{
				UserId = doctor.UserId,
				NotificationType = notificationType,
				Parameters = parameters ?? new Dictionary<string, object>()
			});
		}

		#endregion

		#region Background Processing

		public async Task ProcessPendingNotificationsAsync()
		{
			try
			{
				using var scope = _serviceScopeFactory.CreateScope();
				var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

				var pendingNotifications = await unitOfWork.NotificationQueues.GetPendingNotificationsAsync(DateTime.UtcNow);

				foreach (var notification in pendingNotifications)
				{
					try
					{
						if (notification.NotificationTemplate.RequiresEmail && !string.IsNullOrEmpty(notification.User.Email))
						{
							var emailRequest = new EmailRequestDTO
							{
								toEmail = notification.User.Email,
								Subject = notification.Subject,
								Body = notification.EmailBody ?? notification.NotificationBody
							};

							_emailService.SendEmail(emailRequest);
						}

						await unitOfWork.NotificationQueues.UpdateStatusAsync(notification.QueueId, NotificationStatus.SENT);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, $"Failed to send notification {notification.QueueId}");
						await unitOfWork.NotificationQueues.UpdateStatusAsync(notification.QueueId, NotificationStatus.FAILED, ex.Message);
						await unitOfWork.NotificationQueues.IncrementRetryCountAsync(notification.QueueId);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing pending notifications");
			}
		}

		public async Task ProcessScheduledRemindersAsync()
		{
			try
			{
				using var scope = _serviceScopeFactory.CreateScope();
				var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

				var tomorrow = DateTime.UtcNow.AddDays(1);
				var oneHourLater = DateTime.UtcNow.AddHours(1);

				// Get appointments that need reminders
				var upcomingAppointments = await unitOfWork.Appointments.GetAllAsync(
					a => a.AppointmentDate >= oneHourLater && a.AppointmentDate <= tomorrow && a.Status == "pending",
					includeProperties: "User,Doctor.User");

				foreach (var appointment in upcomingAppointments)
				{
					var timeDiff = appointment.AppointmentDate - DateTime.UtcNow;

					// Send 1-hour reminder
					if (timeDiff.TotalMinutes <= 60 && timeDiff.TotalMinutes > 50)
					{
						await SendAppointmentReminderAsync(appointment.AppointmentId, "patient");
						await SendAppointmentReminderAsync(appointment.AppointmentId, "doctor");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing scheduled reminders");
			}
		}

		public async Task RetryFailedNotificationsAsync()
		{
			try
			{
				using var scope = _serviceScopeFactory.CreateScope();
				var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

				var failedNotifications = await unitOfWork.NotificationQueues.GetFailedNotificationsForRetryAsync();

				foreach (var notification in failedNotifications)
				{
					try
					{
						if (notification.NotificationTemplate.RequiresEmail && !string.IsNullOrEmpty(notification.User.Email))
						{
							var emailRequest = new EmailRequestDTO
							{
								toEmail = notification.User.Email,
								Subject = notification.Subject,
								Body = notification.EmailBody ?? notification.NotificationBody
							};

							_emailService.SendEmail(emailRequest);
						}

						await unitOfWork.NotificationQueues.UpdateStatusAsync(notification.QueueId, NotificationStatus.SENT);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, $"Retry failed for notification {notification.QueueId}");
						await unitOfWork.NotificationQueues.IncrementRetryCountAsync(notification.QueueId);

						if (notification.RetryCount >= 3)
						{
							await unitOfWork.NotificationQueues.UpdateStatusAsync(notification.QueueId, NotificationStatus.CANCELLED, "Max retry attempts reached");
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrying failed notifications");
			}
		}

		#endregion

		#region Template Processing

		public async Task<string> RenderNotificationContentAsync(string templateContent, Dictionary<string, object> parameters)
		{
			var result = templateContent;

			foreach (var param in parameters)
			{
				var placeholder = $"{{{param.Key}}}";
				result = result.Replace(placeholder, param.Value?.ToString() ?? "");
			}

			return result;
		}

		public async Task<NotificationTemplate> GetTemplateByTypeAsync(string type)
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

			return await unitOfWork.NotificationTemplates.GetByTypeAsync(type);
		}

		private async Task<string> RenderEmailContentAsync(string emailTemplate, Dictionary<string, object> parameters)
		{
			return await RenderNotificationContentAsync(emailTemplate, parameters);
		}

		#endregion

		#region Helper Methods

		private int? ExtractAppointmentId(Dictionary<string, object>? parameters)
		{
			if (parameters != null && parameters.TryGetValue("AppointmentId", out var value))
			{
				return Convert.ToInt32(value);
			}
			return null;
		}

		private int? ExtractDoctorId(Dictionary<string, object>? parameters)
		{
			if (parameters != null && parameters.TryGetValue("DoctorId", out var value))
			{
				return Convert.ToInt32(value);
			}
			return null;
		}

		private string? ExtractRelatedEntityType(Dictionary<string, object>? parameters)
		{
			if (parameters != null && parameters.TryGetValue("RelatedEntityType", out var value))
			{
				return value.ToString();
			}
			return null;
		}

		private string? ExtractRelatedEntityId(Dictionary<string, object>? parameters)
		{
			if (parameters != null && parameters.TryGetValue("RelatedEntityId", out var value))
			{
				return value.ToString();
			}
			return null;
		}

		private string? ExtractActionUrl(Dictionary<string, object>? parameters)
		{
			if (parameters != null && parameters.TryGetValue("ActionUrl", out var value))
			{
				return value.ToString();
			}
			return null;
		}

		private async Task SaveToHistoryAsync(string userId, int templateId, string subject, string notificationBody, string? emailBody, string notificationType, Dictionary<string, object>? parameters)
		{
			try
			{
				using var scope = _serviceScopeFactory.CreateScope();
				var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

				var history = new NotificationHistory
				{
					UserId = userId,
					TemplateId = templateId,
					Subject = subject,
					NotificationBody = notificationBody,
					EmailBody = emailBody,
					NotificationType = notificationType,
					DeliveryMethod = !string.IsNullOrEmpty(emailBody) ? DeliveryMethods.BOTH : DeliveryMethods.IN_APP,
					Status = NotificationStatus.SENT,
					SentAt = DateTime.UtcNow,
					AppointmentId = ExtractAppointmentId(parameters),
					DoctorId = ExtractDoctorId(parameters),
					RelatedEntityType = ExtractRelatedEntityType(parameters),
					RelatedEntityId = ExtractRelatedEntityId(parameters)
				};

				await unitOfWork.NotificationHistories.AddAsync(history);
				await unitOfWork.CompleteAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error saving notification to history");
			}
		}

		#endregion
	}
}