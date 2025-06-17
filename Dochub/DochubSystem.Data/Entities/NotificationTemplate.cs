using System.ComponentModel.DataAnnotations;

namespace DochubSystem.Data.Entities
{
	public class NotificationTemplate
	{
		[Key]
		public int TemplateId { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[Required]
		[StringLength(50)]
		public string Type { get; set; } // NEW_APPOINTMENT, APPOINTMENT_CANCELLED, etc.

		[Required]
		[StringLength(200)]
		public string Subject { get; set; }

		[Required]
		public string EmailBody { get; set; }

		[Required]
		public string NotificationBody { get; set; }

		[StringLength(50)]
		public string Priority { get; set; } = "normal"; // low, normal, high, urgent

		[StringLength(50)]
		public string TargetRole { get; set; } // doctor, customer, all

		public bool IsActive { get; set; } = true;
		public bool RequiresEmail { get; set; } = true;
		public bool RequiresInApp { get; set; } = true;

		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		// Navigation properties
		public ICollection<NotificationQueue> NotificationQueues { get; set; }
	}
}
