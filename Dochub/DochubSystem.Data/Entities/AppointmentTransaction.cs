using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DochubSystem.Data.Entities
{
	public class AppointmentTransaction
	{
		[Key]
		public int AppointmentTransactionId { get; set; }

		[ForeignKey("Appointment")]
		public int AppointmentId { get; set; }
		public Appointment Appointment { get; set; }

		[ForeignKey("User")]
		public string UserId { get; set; }
		public User User { get; set; }

		[Required]
		public string PaymentMethod { get; set; }

		[Required]
		public string Status { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Amount { get; set; }
	}
}
