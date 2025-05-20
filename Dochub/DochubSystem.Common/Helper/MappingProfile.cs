using AutoMapper;
using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DochubSystem.Common.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User Mapping
            CreateMap<User, UserDTO>();
            CreateMap<CreateUserDTO, User>();
            CreateMap<UpdateUserDTO, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
