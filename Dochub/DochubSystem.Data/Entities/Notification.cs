using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DochubSystem.Data.Entities
{
    public class Notification
    {
		[Key]
		public int NotificationId { get; set; }

		[ForeignKey("User")]
		public string UserId { get; set; }

		[Required]
		[StringLength(200)]
		public string Title { get; set; }

		[Required]
		public string Message { get; set; }

		[StringLength(50)]
		public string Type { get; set; } // NEW_APPOINTMENT, APPOINTMENT_CANCELLED, etc.

		[StringLength(50)]
		public string Priority { get; set; } = "normal"; // low, normal, high, urgent

		[StringLength(50)]
		public string Status { get; set; } = "unread"; // unread, read

		public DateTime CreatedAt { get; set; }
		public DateTime? ReadAt { get; set; }

		// Reference IDs for related entities
		public int? AppointmentId { get; set; }
		public int? DoctorId { get; set; }
		public string? RelatedEntityType { get; set; } // appointment, membership, etc.
		public string? RelatedEntityId { get; set; }

		// Action URL cho deep linking
		public string? ActionUrl { get; set; }

		// Navigation properties
		public User User { get; set; }

		// Optional navigation properties
		[ForeignKey("AppointmentId")]
		public Appointment? Appointment { get; set; }

		[ForeignKey("DoctorId")]
		public Doctor? Doctor { get; set; }
	}
}
