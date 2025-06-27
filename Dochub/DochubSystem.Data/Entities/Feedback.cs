using DochubSystem.Data.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocHubSystem.Data.Entities
{
    public class Feedback
    {
        public int FeedbackId { get; set; }
        public int DoctorId { get; set; }

        public string UserId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Initials { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int Rating { get; set; }
        public DateTime Date { get; set; }

        public Doctor Doctor { get; set; } = null!;
    }
}
