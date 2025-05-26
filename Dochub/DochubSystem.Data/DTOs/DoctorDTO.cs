using System.ComponentModel.DataAnnotations;

namespace DochubSystem.Data.DTOs
{
	public class DoctorDTO
	{
		public int DoctorId { get; set; }
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string UserEmail { get; set; }
		public string UserPhone { get; set; }
		public string UserImageUrl { get; set; }
		public string Specialization { get; set; }
		public int Experience { get; set; }
		public string LicenseNumber { get; set; }
		public bool IsActive { get; set; }
	}

	public class CreateDoctorDTO
	{
		[Required]
		public string UserId { get; set; }

		[Required]
		[StringLength(100)]
		public string Specialization { get; set; }

		[Required]
		[Range(0, 50, ErrorMessage = "Experience must be between 0 and 50 years")]
		public int Experience { get; set; }

		[Required]
		[StringLength(50)]
		public string LicenseNumber { get; set; }

		public bool IsActive { get; set; } = true;
	}

	public class UpdateDoctorDTO
	{
		[StringLength(100)]
		public string? Specialization { get; set; }

		[Range(0, 50, ErrorMessage = "Experience must be between 0 and 50 years")]
		public int? Experience { get; set; }

		[StringLength(50)]
		public string? LicenseNumber { get; set; }

		public bool? IsActive { get; set; }
	}

	public class DoctorSummaryDTO
	{
		public int DoctorId { get; set; }
		public string UserName { get; set; }
		public string Specialization { get; set; }
		public int Experience { get; set; }
		public bool IsActive { get; set; }
		public string UserImageUrl { get; set; }
	}
}
