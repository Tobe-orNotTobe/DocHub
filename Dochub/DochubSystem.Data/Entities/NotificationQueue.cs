using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Data.Entities
{
	public class NotificationQueue
	{
		[Key]
		public int QueueId { get; set; }

		[ForeignKey("NotificationTemplate")]
		public int TemplateId { get; set; }

		[ForeignKey("User")]
		public string UserId { get; set; }

		[Required]
		[StringLength(200)]
		public string Subject { get; set; }

		[Required]
		public string NotificationBody { get; set; }

		public string? EmailBody { get; set; }

		[StringLength(50)]
		public string Status { get; set; } = "pending"; // pending, sent, failed, cancelled

		[StringLength(50)]
		public string Priority { get; set; } = "normal"; // low, normal, high, urgent

		[StringLength(50)]
		public string NotificationType { get; set; }

		public DateTime ScheduledAt { get; set; }
		public DateTime? SentAt { get; set; }
		public DateTime CreatedAt { get; set; }

		public int RetryCount { get; set; } = 0;
		public string? ErrorMessage { get; set; }

		// Metadata cho dynamic content
		public string? MetaData { get; set; } // JSON string chứa các thông tin bổ sung

		// Navigation properties
		public NotificationTemplate NotificationTemplate { get; set; }
		public User User { get; set; }
	}
}
