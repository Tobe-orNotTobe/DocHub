using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DochubSystem.Data.Entities
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } // 'unread', 'read'

        public User User { get; set; }
    }
}
