using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DochubSystem.Data.Entities
{
	public class NotificationHistory
	{
		[Key]
		public int HistoryId { get; set; }

		[ForeignKey("User")]
		public string UserId { get; set; }

		[ForeignKey("NotificationTemplate")]
		public int? TemplateId { get; set; }

		[Required]
		[StringLength(200)]
		public string Subject { get; set; }

		[Required]
		public string NotificationBody { get; set; }

		public string? EmailBody { get; set; }

		[StringLength(50)]
		public string NotificationType { get; set; }

		[StringLength(50)]
		public string DeliveryMethod { get; set; } // email, inapp, both

		[StringLength(50)]
		public string Status { get; set; } // sent, failed, read, unread

		public DateTime SentAt { get; set; }
		public DateTime? ReadAt { get; set; }

		public string? ErrorMessage { get; set; }

		// Reference to related entities
		public int? AppointmentId { get; set; }
		public int? DoctorId { get; set; }
		public string? RelatedEntityType { get; set; }
		public string? RelatedEntityId { get; set; }

		// Navigation properties
		public User User { get; set; }
		public NotificationTemplate? NotificationTemplate { get; set; }
	}
}