using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Data.Entities
{
	public class TransactionRecord
	{
		[Key]
		public int TransactionId { get; set; }

		[Required]
		[StringLength(450)] // Match AspNetUsers.Id length
		public string UserId { get; set; }

		[Required]
		public int PaymentRequestId { get; set; }

		[Required]
		public int PlanId { get; set; }

		[Required]
		public int? SubscriptionId { get; set; }

		[Required]
		[StringLength(50)]
		public string TransferCode { get; set; }

		[Required]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Amount { get; set; }

		[Required]
		[StringLength(20)]
		public string BillingCycle { get; set; }

		[Required]
		[StringLength(20)]
		public string Status { get; set; } // Completed, Failed, Refunded

		[Required]
		public DateTime TransactionDate { get; set; }

		[Required]
		[StringLength(100)] // Just store admin name/email as string
		public string ProcessedByAdmin { get; set; }

		[StringLength(500)]
		public string? Notes { get; set; }

		// Bank transfer details
		[StringLength(20)]
		public string BankCode { get; set; }

		[StringLength(50)]
		public string AccountNo { get; set; }

		[StringLength(100)]
		public string AccountName { get; set; }

		// Navigation properties (simplified - no admin references)
		public User User { get; set; }
		public PaymentRequest PaymentRequest { get; set; }
		public SubscriptionPlan Plan { get; set; }
		public UserSubscription Subscription { get; set; }
	}
}
