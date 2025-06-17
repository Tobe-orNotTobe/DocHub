using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DochubSystem.Data.Entities
{
	public class ConsultationUsage
	{
		[Key]
		public int UsageId { get; set; }

		[ForeignKey("UserSubscription")]
		public int SubscriptionId { get; set; }

		[ForeignKey("Appointment")]
		public int AppointmentId { get; set; }

		[ForeignKey("User")]
		public string UserId { get; set; }

		public DateTime UsageDate { get; set; }
		public string ConsultationType { get; set; } // Video, Chat, Audio

		public UserSubscription UserSubscription { get; set; }
		public Appointment Appointment { get; set; }
		public User User { get; set; }
	}
}
