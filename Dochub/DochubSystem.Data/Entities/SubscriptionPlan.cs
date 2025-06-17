using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Data.Entities
{
	public class SubscriptionPlan
	{
		[Key]
		public int PlanId { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; } // Basic, Standard, Premium

		public string Description { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal MonthlyPrice { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal YearlyPrice { get; set; }

		// Features as JSON or separate properties
		public int ConsultationsPerMonth { get; set; } // -1 for unlimited
		public int MaxDoctorsAccess { get; set; } // -1 for all doctors
		public bool HasVideoCallSupport { get; set; }
		public bool HasPriorityBooking { get; set; }
		public bool Has24x7Support { get; set; }
		public decimal DiscountPercentage { get; set; }
		public bool HasMedicationReminders { get; set; }
		public bool HasHealthReports { get; set; }
		public bool HasBasicMedicalInfo { get; set; }
		public bool HasMedicalRecordStorage { get; set; }
		public bool HasExamReminders { get; set; }

		public bool IsActive { get; set; } = true;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		public ICollection<UserSubscription> UserSubscriptions { get; set; }
	}
}
