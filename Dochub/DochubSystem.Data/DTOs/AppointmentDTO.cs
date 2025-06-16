using System.ComponentModel.DataAnnotations;

namespace DochubSystem.Data.DTOs
{
	public class AppointmentDTO
	{
		public int AppointmentId { get; set; }
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string UserEmail { get; set; }
		public int DoctorId { get; set; }
		public string DoctorName { get; set; }
		public DateTime AppointmentDate { get; set; }
		public string Status { get; set; }
		public string? Symptoms { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public string? CancellationReason { get; set; }
		public DateTime? CancelledAt { get; set; }
	}

	public class CreateAppointmentDTO
	{
		[Required]
		public int DoctorId { get; set; }

		[Required]
		public DateTime AppointmentDate { get; set; }

		[StringLength(1000, ErrorMessage = "Symptoms cannot exceed 1000 characters")]
		public string? Symptoms { get; set; }

	}
	public class AppointmentSummaryDTO
	{
		public int AppointmentId { get; set; }
		public string PatientName { get; set; }
		public string DoctorName { get; set; }
		public string DoctorSpecialization { get; set; }
		public DateTime AppointmentDate { get; set; }
		public string Status { get; set; }
		public string? Symptoms { get; set; }

	}

	public class UpdateAppointmentDTO
	{
		public DateTime? AppointmentDate { get; set; }
		public string? Status { get; set; }
		public string? CancellationReason { get; set; }
		[StringLength(1000, ErrorMessage = "Symptoms cannot exceed 1000 characters")]
		public string? Symptoms { get; set; }
	}

	public class CancelAppointmentDTO
	{
		[Required]
		public string CancellationReason { get; set; }
	}
}
