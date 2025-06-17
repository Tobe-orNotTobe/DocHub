using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DochubSystem.Data.Entities
{
	public class PaymentTransaction
	{
		[Key]
		public int PaymentTransactionId { get; set; }

		[Required]
		public string TransactionRef { get; set; } // Unique transaction reference

		[ForeignKey("User")]
		public string UserId { get; set; }

		[ForeignKey("UserSubscription")]
		public int? SubscriptionId { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Amount { get; set; }

		[Required]
		public string PaymentMethod { get; set; } // VNPay, MoMo, Banking

		[Required]
		public string Status { get; set; } // Pending, Completed, Failed, Expired

		[Required]
		public string TransactionType { get; set; } // Subscription, Renewal, Upgrade

		public string? PaymentGatewayTransactionId { get; set; }
		public string? PaymentGatewayResponse { get; set; } // Raw response from gateway

		public DateTime CreatedAt { get; set; }
		public DateTime? ProcessedAt { get; set; }
		public DateTime? ExpiredAt { get; set; }

		public string? IpAddress { get; set; }
		public string? UserAgent { get; set; }

		// Additional info
		public string OrderInfo { get; set; }
		public string BillingCycle { get; set; }
		public string Currency { get; set; } = "VND";

		// Navigation properties
		public User User { get; set; }
		public UserSubscription UserSubscription { get; set; }
	}
}
