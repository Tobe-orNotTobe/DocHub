using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Data.Data
{
    public class MedicalRecord
    {
        public int RecordId { get; set; }
        public int AppointmentId { get; set; }
        public string Description { get; set; }
        public string Files { get; set; } // JSON for file URLs
        public DateTime CreatedAt { get; set; }

        public Appointment Appointment { get; set; }
    }
}
