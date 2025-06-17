using System.ComponentModel.DataAnnotations;

namespace DochubSystem.Data.DTOs
{
	public class SubscriptionPlanDTO
	{
		public int PlanId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal MonthlyPrice { get; set; }
		public decimal YearlyPrice { get; set; }
		public int ConsultationsPerMonth { get; set; }
		public int MaxDoctorsAccess { get; set; }
		public bool HasVideoCallSupport { get; set; }
		public bool HasPriorityBooking { get; set; }
		public bool Has24x7Support { get; set; }
		public decimal DiscountPercentage { get; set; }
		public bool HasMedicationReminders { get; set; }
		public bool HasHealthReports { get; set; }
		public bool HasBasicMedicalInfo { get; set; }
		public bool HasMedicalRecordStorage { get; set; }
		public bool HasExamReminders { get; set; }
		public bool IsActive { get; set; }
	}

	public class CreateSubscriptionPlanDTO
	{
		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[Required]
		public string Description { get; set; }

		[Required]
		[Range(0, double.MaxValue)]
		public decimal MonthlyPrice { get; set; }

		[Required]
		[Range(0, double.MaxValue)]
		public decimal YearlyPrice { get; set; }

		public int ConsultationsPerMonth { get; set; } = -1;
		public int MaxDoctorsAccess { get; set; } = -1;
		public bool HasVideoCallSupport { get; set; } = false;
		public bool HasPriorityBooking { get; set; } = false;
		public bool Has24x7Support { get; set; } = false;
		public decimal DiscountPercentage { get; set; } = 0;
		public bool HasMedicationReminders { get; set; } = false;
		public bool HasHealthReports { get; set; } = false;
		public bool HasBasicMedicalInfo { get; set; } = true;
		public bool HasMedicalRecordStorage { get; set; } = true;
		public bool HasExamReminders { get; set; } = true;
	}

	public class UserSubscriptionDTO
	{
		public int SubscriptionId { get; set; }
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string UserEmail { get; set; }
		public int PlanId { get; set; }
		public string PlanName { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public DateTime? CancelledAt { get; set; }
		public string? CancellationReason { get; set; }
		public string Status { get; set; }
		public string BillingCycle { get; set; }
		public decimal PaidAmount { get; set; }
		public int ConsultationsUsed { get; set; }
		public int ConsultationsRemaining { get; set; }
		public DateTime? LastConsultationDate { get; set; }
		public bool HasPendingPlanChange { get; set; }
		public string? PendingPlanName { get; set; }
		public DateTime? PlanChangeEffectiveDate { get; set; }
		public SubscriptionPlanDTO CurrentPlan { get; set; }
	}

	public class CreateSubscriptionDTO
	{
		[Required]
		public string UserId { get; set; }

		[Required]
		public int PlanId { get; set; }

		[Required]
		public string BillingCycle { get; set; } // Monthly, Yearly

		[Required]
		public string PaymentMethod { get; set; }

		public string? PaymentGatewayTransactionId { get; set; }
	}

	public class CancelSubscriptionDTO
	{
		[Required]
		public string CancellationReason { get; set; }

		public bool ImmediateCancel { get; set; } = false; // If true, cancel immediately, else at period end
	}

	public class ChangePlanDTO
	{
		[Required]
		public int NewPlanId { get; set; }

		public bool EffectiveImmediately { get; set; } = false; // If false, change at next billing period

		public string? PaymentMethod { get; set; }
	}

	public class SubscriptionUsageDTO
	{
		public int ConsultationsUsed { get; set; }
		public int ConsultationsRemaining { get; set; }
		public DateTime? LastConsultationDate { get; set; }
		public List<ConsultationUsageDetailDTO> RecentUsage { get; set; }
	}

	public class ConsultationUsageDetailDTO
	{
		public int UsageId { get; set; }
		public int AppointmentId { get; set; }
		public DateTime UsageDate { get; set; }
		public string ConsultationType { get; set; }
		public string DoctorName { get; set; }
	}

	public class SubscriptionStatsDTO
	{
		public int TotalActiveSubscriptions { get; set; }
		public int TotalCancelledSubscriptions { get; set; }
		public decimal MonthlyRevenue { get; set; }
		public decimal YearlyRevenue { get; set; }
		public Dictionary<string, int> PlanDistribution { get; set; }
		public List<SubscriptionTrendDTO> Revenuetrend { get; set; }
	}

	public class SubscriptionTrendDTO
	{
		public DateTime Date { get; set; }
		public decimal Revenue { get; set; }
		public int NewSubscriptions { get; set; }
		public int Cancellations { get; set; }
	}

	public class RenewSubscriptionDTO
	{
		[Required]
		public string PaymentMethod { get; set; }

		public string? PaymentGatewayTransactionId { get; set; }
	}
}
