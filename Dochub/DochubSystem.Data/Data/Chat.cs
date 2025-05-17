using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Data.Data
{
    public class Chat
    {
        public int ChatId { get; set; }
        public int AppointmentId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        public Appointment Appointment { get; set; }
        public User User { get; set; }
    }
}
