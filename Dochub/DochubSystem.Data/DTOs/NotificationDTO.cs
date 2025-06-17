using System.ComponentModel.DataAnnotations;

namespace DochubSystem.Data.DTOs
{
	// NotificationTemplate DTOs
	public class NotificationTemplateDTO
	{
		public int TemplateId { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public string Subject { get; set; }
		public string EmailBody { get; set; }
		public string NotificationBody { get; set; }
		public string Priority { get; set; }
		public string TargetRole { get; set; }
		public bool IsActive { get; set; }
		public bool RequiresEmail { get; set; }
		public bool RequiresInApp { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}

	public class CreateNotificationTemplateDTO
	{
		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[Required]
		[StringLength(50)]
		public string Type { get; set; }

		[Required]
		[StringLength(200)]
		public string Subject { get; set; }

		[Required]
		public string EmailBody { get; set; }

		[Required]
		public string NotificationBody { get; set; }

		[StringLength(50)]
		public string Priority { get; set; } = "normal";

		[StringLength(50)]
		public string TargetRole { get; set; }

		public bool IsActive { get; set; } = true;
		public bool RequiresEmail { get; set; } = true;
		public bool RequiresInApp { get; set; } = true;
	}

	public class UpdateNotificationTemplateDTO
	{
		[StringLength(100)]
		public string? Name { get; set; }

		[StringLength(200)]
		public string? Subject { get; set; }

		public string? EmailBody { get; set; }
		public string? NotificationBody { get; set; }

		[StringLength(50)]
		public string? Priority { get; set; }

		[StringLength(50)]
		public string? TargetRole { get; set; }

		public bool? IsActive { get; set; }
		public bool? RequiresEmail { get; set; }
		public bool? RequiresInApp { get; set; }
	}

	// Notification DTOs
	public class NotificationDTO
	{
		public int NotificationId { get; set; }
		public string UserId { get; set; }
		public string Title { get; set; }
		public string Message { get; set; }
		public string Type { get; set; }
		public string Priority { get; set; }
		public string Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? ReadAt { get; set; }
		public int? AppointmentId { get; set; }
		public int? DoctorId { get; set; }
		public string? RelatedEntityType { get; set; }
		public string? RelatedEntityId { get; set; }
		public string? ActionUrl { get; set; }

		// Additional info for display
		public string? DoctorName { get; set; }
		public string? AppointmentDate { get; set; }
	}

	public class CreateNotificationDTO
	{
		[Required]
		public string UserId { get; set; }

		[Required]
		[StringLength(200)]
		public string Title { get; set; }

		[Required]
		public string Message { get; set; }

		[Required]
		[StringLength(50)]
		public string Type { get; set; }

		[StringLength(50)]
		public string Priority { get; set; } = "normal";

		public int? AppointmentId { get; set; }
		public int? DoctorId { get; set; }
		public string? RelatedEntityType { get; set; }
		public string? RelatedEntityId { get; set; }
		public string? ActionUrl { get; set; }
	}

	public class NotificationSummaryDTO
	{
		public int NotificationId { get; set; }
		public string Title { get; set; }
		public string Message { get; set; }
		public string Type { get; set; }
		public string Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public string? ActionUrl { get; set; }
	}

	public class MarkNotificationReadDTO
	{
		[Required]
		public int NotificationId { get; set; }
	}

	public class BulkMarkReadDTO
	{
		[Required]
		public List<int> NotificationIds { get; set; }
	}

	// NotificationQueue DTOs
	public class NotificationQueueDTO
	{
		public int QueueId { get; set; }
		public int TemplateId { get; set; }
		public string UserId { get; set; }
		public string Subject { get; set; }
		public string NotificationBody { get; set; }
		public string? EmailBody { get; set; }
		public string Status { get; set; }
		public string Priority { get; set; }
		public string NotificationType { get; set; }
		public DateTime ScheduledAt { get; set; }
		public DateTime? SentAt { get; set; }
		public DateTime CreatedAt { get; set; }
		public int RetryCount { get; set; }
		public string? ErrorMessage { get; set; }
		public string? MetaData { get; set; }
	}

	public class CreateNotificationQueueDTO
	{
		[Required]
		public int TemplateId { get; set; }

		[Required]
		public string UserId { get; set; }

		[Required]
		public string Subject { get; set; }

		[Required]
		public string NotificationBody { get; set; }

		public string? EmailBody { get; set; }

		[Required]
		public string NotificationType { get; set; }

		public string Priority { get; set; } = "normal";
		public DateTime? ScheduledAt { get; set; }
		public string? MetaData { get; set; }
	}

	// Request/Response DTOs
	public class SendNotificationRequestDTO
	{
		[Required]
		public string UserId { get; set; }

		[Required]
		public string NotificationType { get; set; }

		public Dictionary<string, object>? Parameters { get; set; }
		public DateTime? ScheduledAt { get; set; }
		public string? Priority { get; set; }
	}

	public class GetNotificationsRequestDTO
	{
		public string? Status { get; set; } // unread, read, all
		public string? Type { get; set; }
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 20;
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
	}

	public class NotificationStatisticsDTO
	{
		public int TotalNotifications { get; set; }
		public int UnreadNotifications { get; set; }
		public int ReadNotifications { get; set; }
		public int HighPriorityNotifications { get; set; }
		public int TodayNotifications { get; set; }
		public Dictionary<string, int> NotificationsByType { get; set; } = new();
	}

	public class NotificationResponseDTO
	{
		public List<NotificationDTO> Notifications { get; set; } = new();
		public int TotalCount { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
		public int TotalPages { get; set; }
		public bool HasNextPage { get; set; }
		public bool HasPreviousPage { get; set; }
	}

	// Notification History DTOs
	public class NotificationHistoryDTO
	{
		public int HistoryId { get; set; }
		public string UserId { get; set; }
		public int? TemplateId { get; set; }
		public string Subject { get; set; }
		public string NotificationBody { get; set; }
		public string? EmailBody { get; set; }
		public string NotificationType { get; set; }
		public string DeliveryMethod { get; set; }
		public string Status { get; set; }
		public DateTime SentAt { get; set; }
		public DateTime? ReadAt { get; set; }
		public string? ErrorMessage { get; set; }
		public int? AppointmentId { get; set; }
		public int? DoctorId { get; set; }
		public string? RelatedEntityType { get; set; }
		public string? RelatedEntityId { get; set; }
		public string? TemplateName { get; set; }
	}

	// Scheduled Notification DTOs
	public class ScheduleReminderDTO
	{
		[Required]
		public int AppointmentId { get; set; }

		[Required]
		public string ReminderType { get; set; } // patient_reminder, doctor_reminder

		[Required]
		public DateTime ReminderTime { get; set; }

		public string? CustomMessage { get; set; }
	}

	public class BulkNotificationDTO
	{
		[Required]
		public List<string> UserIds { get; set; }

		[Required]
		public string NotificationType { get; set; }

		public Dictionary<string, object>? Parameters { get; set; }
		public DateTime? ScheduledAt { get; set; }
		public string? Priority { get; set; } = "normal";
	}

	// Notification Settings DTOs
	public class NotificationSettingsDTO
	{
		public bool EmailNotifications { get; set; } = true;
		public bool InAppNotifications { get; set; } = true;
		public bool AppointmentReminders { get; set; } = true;
		public bool MembershipNotifications { get; set; } = true;
		public bool MarketingNotifications { get; set; } = false;
		public int ReminderMinutesBefore { get; set; } = 30;
	}

	public class UpdateNotificationSettingsDTO
	{
		public bool? EmailNotifications { get; set; }
		public bool? InAppNotifications { get; set; }
		public bool? AppointmentReminders { get; set; }
		public bool? MembershipNotifications { get; set; }
		public bool? MarketingNotifications { get; set; }
		public int? ReminderMinutesBefore { get; set; }
	}
}