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
        public int Experience { get; set; }
        public string LicenseNumber { get; set; }
        public bool IsActive { get; set; } = true;

        public User User { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}
