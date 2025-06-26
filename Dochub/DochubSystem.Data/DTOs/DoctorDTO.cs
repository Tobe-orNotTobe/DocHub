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
		public int? YearsOfExperience { get; set; }
		public string? Bio { get; set; }
        public string? ImageDoctor { get; set; }  // ✅ Thêm dòng này
        public string? HospitalName { get; set; }
		public decimal? Rating { get; set; }
		public bool IsActive { get; set; }
	}

	public class CreateDoctorDTO
	{
		[Required]
		public string UserId { get; set; }

		public string? Specialization { get; set; }
		public int? YearsOfExperience { get; set; }
		public string? Bio { get; set; }
		public string? HospitalName { get; set; }
		public bool IsActive { get; set; } = true;
        public string? ImageDoctor { get; set; }  // ✅ Thêm dòng này

    }

    public class UpdateDoctorDTO
	{
		[StringLength(100)]
		public string? Specialization { get; set; }

		[Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
		public int? YearsOfExperience { get; set; }

		[StringLength(1000)]
		public string? Bio { get; set; }

		[StringLength(200)]
		public string? HospitalName { get; set; }

		public bool? IsActive { get; set; }
        public string? ImageDoctor { get; set; }  // ✅ Thêm dòng này

    }

    public class DoctorSummaryDTO
	{
		public int DoctorId { get; set; }
		public string UserName { get; set; }
		public string Specialization { get; set; }
		public int? YearsOfExperience { get; set; }
		public string? HospitalName { get; set; }
		public decimal? Rating { get; set; }
		public bool IsActive { get; set; }
		public string UserImageUrl { get; set; }
        public string? ImageDoctor { get; set; }  // ✅ Thêm dòng này

    }
}
