using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DochubSystem.Data.Entities
{
	public class UserSubscription
	{
		[Key]
		public int SubscriptionId { get; set; }

		[ForeignKey("User")]
		public string UserId { get; set; }

		[ForeignKey("SubscriptionPlan")]
		public int PlanId { get; set; }

		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public DateTime? CancelledAt { get; set; }
		public string? CancellationReason { get; set; }

		public string Status { get; set; } // Active, Cancelled, Expired, PendingCancellation
		public string BillingCycle { get; set; } // Monthly, Yearly

		[Column(TypeName = "decimal(18,2)")]
		public decimal PaidAmount { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		// For plan changes
		public int? PendingPlanId { get; set; }
		public DateTime? PlanChangeEffectiveDate { get; set; }

		// Usage tracking
		public int ConsultationsUsed { get; set; } = 0;
		public DateTime LastConsultationDate { get; set; }

		public User User { get; set; }
		public SubscriptionPlan SubscriptionPlan { get; set; }
		public SubscriptionPlan PendingPlan { get; set; }
	}
}
