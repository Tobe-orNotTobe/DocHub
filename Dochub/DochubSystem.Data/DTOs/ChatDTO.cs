using System;

namespace DochubSystem.Data.DTOs
{
    public class ChatDTO
    {
        public int ChatId { get; set; }
        public int? AppointmentId { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
