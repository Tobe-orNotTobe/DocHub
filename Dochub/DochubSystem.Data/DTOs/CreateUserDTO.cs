using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Data.DTOs
{
    public class CreateUserDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Password { get; set; }
        public string ImageUrl { get; set; }
    }
}
