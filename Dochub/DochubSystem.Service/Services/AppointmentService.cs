using AutoMapper;
using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using DochubSystem.RepositoryContract.Interfaces;
using DochubSystem.ServiceContract.Interfaces;

namespace DochubSystem.Service.Services
{
	public class AppointmentService : IAppointmentService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<AppointmentDTO> CreateAppointmentAsync(string userId, CreateAppointmentDTO createAppointmentDTO)
		{
			// Validate user exists
			var userExists = await _unitOfWork.Users.UserExistsAsync(userId);
			if (!userExists)
				throw new ArgumentException("User not found");

			// Validate doctor exists
			var doctor = await _unitOfWork.Doctors.GetAsync(d => d.DoctorId == createAppointmentDTO.DoctorId && d.IsActive);
			if (doctor == null)
				throw new ArgumentException("Doctor not found or inactive");

			// Check if appointment date is in the future
			if (createAppointmentDTO.AppointmentDate <= DateTime.UtcNow)
				throw new ArgumentException("Appointment date must be in the future");

			var appointment = new Appointment
			{
				UserId = userId,
				DoctorId = createAppointmentDTO.DoctorId,
				AppointmentDate = createAppointmentDTO.AppointmentDate,
				Price = createAppointmentDTO.Price,
				Status = "pending",
				Symptoms = createAppointmentDTO.Symptoms,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			var createdAppointment = await _unitOfWork.Appointments.AddAsync(appointment);
			await _unitOfWork.CompleteAsync();

			// Get the full appointment with navigation properties
			var fullAppointment = await _unitOfWork.Appointments.GetAsync(
				a => a.AppointmentId == createdAppointment.AppointmentId,
				includeProperties: "User,Doctor.User"
			);

			return _mapper.Map<AppointmentDTO>(fullAppointment);
		}

		public async Task<AppointmentDTO> GetAppointmentByIdAsync(int appointmentId)
		{
			var appointment = await _unitOfWork.Appointments.GetAsync(
				a => a.AppointmentId == appointmentId,
				includeProperties: "User,Doctor.User"
			);

			if (appointment == null)
				return null;

			return _mapper.Map<AppointmentDTO>(appointment);
		}
	}
}