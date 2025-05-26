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
			CreateMap<UserRegisterDTO, User>()
			 .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
			 .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
			 .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
			 .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
			 .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
			 .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
			 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true)) // Default to true
			 .ForMember(dest => dest.Id, opt => opt.Ignore());
			CreateMap<CreateUserDTO, User>();
            CreateMap<UpdateUserDTO, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

			// Appointment Mappings
			CreateMap<CreateAppointmentDTO, Appointment>()
				.ForMember(dest => dest.AppointmentId, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.Status, opt => opt.Ignore())
				.ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
				.ForMember(dest => dest.CancelledAt, opt => opt.Ignore())
				.ForMember(dest => dest.User, opt => opt.Ignore())
				.ForMember(dest => dest.Doctor, opt => opt.Ignore())
				.ForMember(dest => dest.Chats, opt => opt.Ignore())
				.ForMember(dest => dest.MedicalRecords, opt => opt.Ignore());

			CreateMap<Appointment, AppointmentDTO>()
				.ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
				.ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
				.ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor != null && src.Doctor.User != null ? src.Doctor.User.FullName : string.Empty));

			CreateMap<Appointment, AppointmentSummaryDTO>()
				.ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
				.ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor != null && src.Doctor.User != null ? src.Doctor.User.FullName : string.Empty))
				.ForMember(dest => dest.DoctorSpecialization, opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.Specialization : string.Empty));

			CreateMap<UpdateAppointmentDTO, Appointment>()
				.ForMember(dest => dest.AppointmentId, opt => opt.Ignore())
				.ForMember(dest => dest.UserId, opt => opt.Ignore())
				.ForMember(dest => dest.DoctorId, opt => opt.Ignore())
				.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
				.ForMember(dest => dest.User, opt => opt.Ignore())
				.ForMember(dest => dest.Doctor, opt => opt.Ignore())
				.ForMember(dest => dest.Chats, opt => opt.Ignore())
				.ForMember(dest => dest.MedicalRecords, opt => opt.Ignore())
				.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

			// Doctor Mappings
			CreateMap<CreateDoctorDTO, Doctor>()
				.ForMember(dest => dest.DoctorId, opt => opt.Ignore())
				.ForMember(dest => dest.User, opt => opt.Ignore())
				.ForMember(dest => dest.Appointments, opt => opt.Ignore());

			CreateMap<Doctor, DoctorDTO>()
				.ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
				.ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
				.ForMember(dest => dest.UserPhone, opt => opt.MapFrom(src => src.User != null ? src.User.PhoneNumber : string.Empty))
				.ForMember(dest => dest.UserImageUrl, opt => opt.MapFrom(src => src.User != null ? src.User.ImageUrl : string.Empty));

			CreateMap<Doctor, DoctorSummaryDTO>()
				.ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
				.ForMember(dest => dest.UserImageUrl, opt => opt.MapFrom(src => src.User != null ? src.User.ImageUrl : string.Empty));

			CreateMap<UpdateDoctorDTO, Doctor>()
				.ForMember(dest => dest.DoctorId, opt => opt.Ignore())
				.ForMember(dest => dest.UserId, opt => opt.Ignore())
				.ForMember(dest => dest.User, opt => opt.Ignore())
				.ForMember(dest => dest.Appointments, opt => opt.Ignore())
				.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


		}
	}
}
