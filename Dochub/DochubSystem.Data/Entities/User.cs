using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace DochubSystem.Data.Entities
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; } = true;
        public string? RefreshToken { get; set; }
        public string? ImageUrl { get; set; }
        public string? CertificateImageUrl { get; set; }

        public ICollection<Session> Sessions { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<Chat> Chats { get; set; }
    }
}
