using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Data.DTOs
{
    public class CreateFeedbackDTO
    {
        public int DoctorId { get; set; }
        public string Content { get; set; } = null!;
        public int Rating { get; set; }
    }
}
