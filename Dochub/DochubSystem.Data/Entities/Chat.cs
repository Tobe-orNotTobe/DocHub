using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DochubSystem.Data.Entities
{
    public class Chat
    {
        [Key]
        public int ChatId { get; set; }

        [ForeignKey("Appointment")]
        public int? AppointmentId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        public Appointment Appointment { get; set; }
        public User User { get; set; }
    }
}
