using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface INotificationService
	{
		// Core notification methods
		Task<bool> SendNotificationAsync(SendNotificationRequestDTO request);
		Task<bool> SendBulkNotificationAsync(BulkNotificationDTO request);
		Task<bool> ScheduleNotificationAsync(string userId, string notificationType, DateTime scheduledAt, Dictionary<string, object>? parameters = null);

		// Notification management
		Task<NotificationResponseDTO> GetUserNotificationsAsync(string userId, GetNotificationsRequestDTO request);
		Task<NotificationDTO> GetNotificationByIdAsync(int notificationId);
		Task<bool> MarkAsReadAsync(int notificationId, string userId);
		Task<bool> MarkMultipleAsReadAsync(BulkMarkReadDTO request, string userId);
		Task<bool> MarkAllAsReadAsync(string userId);
		Task<bool> DeleteNotificationAsync(int notificationId, string userId);

		// Statistics and analytics
		Task<NotificationStatisticsDTO> GetUserStatisticsAsync(string userId);
		Task<int> GetUnreadCountAsync(string userId);

		// Appointment-specific notifications
		Task<bool> SendAppointmentCreatedNotificationAsync(int appointmentId);
		Task<bool> SendAppointmentUpdatedNotificationAsync(int appointmentId, string updateType);
		Task<bool> SendAppointmentCancelledNotificationAsync(int appointmentId, string cancellationReason);
		Task<bool> SendAppointmentReminderAsync(int appointmentId, string reminderType);

		// Membership notifications
		Task<bool> SendMembershipExpiringNotificationAsync(string userId, DateTime expirationDate);
		Task<bool> SendMembershipRenewedNotificationAsync(string userId);

		// Doctor-specific notifications
		Task<bool> SendDoctorNotificationAsync(int doctorId, string notificationType, Dictionary<string, object>? parameters = null);

		// Background processing
		Task ProcessPendingNotificationsAsync();
		Task ProcessScheduledRemindersAsync();
		Task RetryFailedNotificationsAsync();

		// Template management
		Task<string> RenderNotificationContentAsync(string templateType, Dictionary<string, object> parameters);
		Task<NotificationTemplate> GetTemplateByTypeAsync(string type);
	}
}