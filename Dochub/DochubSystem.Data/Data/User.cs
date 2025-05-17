using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Data.Data
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; } // 'patient', 'doctor'
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Chat> Chats { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<Session> Sessions { get; set; }
    }
}
