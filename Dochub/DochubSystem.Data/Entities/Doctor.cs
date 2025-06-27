using DocHubSystem.Data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DochubSystem.Data.Entities
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public string Specialization { get; set; }
        public int? YearsOfExperience { get; set; }
		public string? Bio { get; set; }
		public string? HospitalName { get; set; }
		[Range(0, 5)]
		[Column(TypeName = "decimal(2,1)")]
		public decimal? Rating { get; set; }
		public bool IsActive { get; set; } = true;
        public string? ImageDoctor { get; set; }   // ✅ ảnh bác sĩ

        public User User { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
    }
}
