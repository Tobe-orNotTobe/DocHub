using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Data.DTOs
{
    public class FeedbackDTO
    {
        public int FeedbackId { get; set; }
        public int DoctorId { get; set; }
        public string UserId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Initials { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int Rating { get; set; }
        public DateTime Date { get; set; }
    }

}
