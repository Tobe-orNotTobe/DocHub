using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Data.Data
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int UserId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } // 'pending', 'completed', 'cancelled'
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; }
        public Doctor Doctor { get; set; }
        public Payment Payment { get; set; }
        public ICollection<Chat> Chats { get; set; }
        public ICollection<MedicalRecord> MedicalRecords { get; set; }
    }
}
