using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Data.Entities
{
	public class PaymentRequest
	{
		[Key]
		public int PaymentRequestId { get; set; }

		[Required]
		[StringLength(450)] // Match AspNetUsers.Id length
		public string UserId { get; set; }

		[Required]
		public int PlanId { get; set; }

		[Required]
		[StringLength(50)]
		public string TransferCode { get; set; } // TVIP-USER123-24JUN2025

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Amount { get; set; }

		[Required]
		[StringLength(20)]
		public string BillingCycle { get; set; } // Monthly, Yearly

		[Required]
		[StringLength(20)]
		public string Status { get; set; } // Pending, Confirmed, Expired, Cancelled

		[Required]
		public DateTime CreatedAt { get; set; }

		[Required]
		public DateTime ExpiresAt { get; set; }

		public DateTime? ConfirmedAt { get; set; }

		[StringLength(100)] // Just store admin name/email as string
		public string? ConfirmedByAdmin { get; set; }

		[StringLength(500)]
		public string? Notes { get; set; }

		// QR Code information
		[Required]
		[StringLength(20)]
		public string BankCode { get; set; }

		[Required]
		[StringLength(50)]
		public string AccountNo { get; set; }

		[Required]
		[StringLength(100)]
		public string AccountName { get; set; }

		public string QRCodeUrl { get; set; }

		// Navigation properties (simplified - no admin references)
		public User User { get; set; }
		public SubscriptionPlan Plan { get; set; }
	}
}
