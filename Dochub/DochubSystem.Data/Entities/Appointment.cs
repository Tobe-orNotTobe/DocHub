using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DochubSystem.Data.Entities
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }

        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } // 'pending', 'completed', 'cancelled'
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; }
        public Doctor Doctor { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public ICollection<Chat> Chats { get; set; }
        public ICollection<MedicalRecord> MedicalRecords { get; set; }
    }
}
