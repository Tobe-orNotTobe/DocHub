using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Data.Data
{
    public class Doctor
    {
        public int DoctorId { get; set; }
        public int UserId { get; set; }
        public string Specialization { get; set; }
        public int Experience { get; set; }
        public string LicenseNumber { get; set; }

        public User User { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}
