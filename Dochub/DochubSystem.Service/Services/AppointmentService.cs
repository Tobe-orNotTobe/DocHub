using AutoMapper;
using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using DochubSystem.RepositoryContract.Interfaces;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.Extensions.Logging;

namespace DochubSystem.Service.Services
{
	public class AppointmentService : IAppointmentService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly INotificationService _notificationService;
		private readonly ILogger<AppointmentService> _logger;

		public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper,
			INotificationService notificationService,
			ILogger<AppointmentService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_notificationService = notificationService;
			_logger = logger;
		}

		public async Task<AppointmentDTO> CreateAppointmentAsync(string userId, CreateAppointmentDTO createAppointmentDTO)
		{
			using var transaction = await _unitOfWork.BeginTransactionAsync();
			try
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
				var appointmentDTO = _mapper.Map<AppointmentDTO>(fullAppointment);

				// Commit transaction before sending notifications
				await transaction.CommitAsync();

				// Send notifications asynchronously (don't wait for them to complete)
				_ = Task.Run(async () =>
				{
					try
					{
						await _notificationService.SendAppointmentCreatedNotificationAsync(createdAppointment.AppointmentId);
						_logger.LogInformation($"Sent appointment created notifications for appointment {createdAppointment.AppointmentId}");
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, $"Failed to send appointment created notifications for appointment {createdAppointment.AppointmentId}");
					}
				});

				return appointmentDTO;
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
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

		public async Task<IEnumerable<AppointmentDTO>> GetAppointmentsByUserIdAsync(string userId)
		{
			var appointments = await _unitOfWork.Appointments.GetAllAsync(
				filter: a => a.UserId == userId,
				includeProperties: "Doctor.User"
			);

			return _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
		}

		public async Task<IEnumerable<AppointmentDTO>> GetAppointmentsByDoctorIdAsync(int doctorId)
		{
			var appointments = await _unitOfWork.Appointments.GetAllAsync(
				filter: a => a.DoctorId == doctorId,
				includeProperties: "Doctor,Doctor.User,User");

			return _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
		}

		public async Task<IEnumerable<AppointmentSummaryDTO>> GetUpcomingAppointmentsAsync(string userId)
		{
			var now = DateTime.UtcNow;
			var appointments = await _unitOfWork.Appointments.GetAllAsync(
				filter: a => a.UserId == userId && a.AppointmentDate > now && a.Status != "cancelled",
				includeProperties: "Doctor.User"
			);

			return _mapper.Map<IEnumerable<AppointmentSummaryDTO>>(appointments);
		}

		public async Task<AppointmentDTO> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentDTO updateAppointmentDTO)
		{
			var appointment = await _unitOfWork.Appointments.GetAsync(
				a => a.AppointmentId == appointmentId,
				includeProperties: "User,Doctor.User"
			);

			if (appointment == null)
				throw new ArgumentException("Appointment not found");

			// Don't allow updates to completed or cancelled appointments
			if (appointment.Status == "completed" || appointment.Status == "cancelled")
				throw new InvalidOperationException("Cannot update completed or cancelled appointments");

			// Update appointment date if provided
			if (updateAppointmentDTO.AppointmentDate.HasValue)
			{
				if (updateAppointmentDTO.AppointmentDate.Value <= DateTime.UtcNow)
					throw new ArgumentException("Appointment date must be in the future");

				appointment.AppointmentDate = updateAppointmentDTO.AppointmentDate.Value;
			}

			if (!string.IsNullOrEmpty(updateAppointmentDTO.Status))
				appointment.Status = updateAppointmentDTO.Status;

			appointment.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.Appointments.UpdateAsync(appointment);
			await _unitOfWork.CompleteAsync();

			return _mapper.Map<AppointmentDTO>(appointment);
		}

		public async Task<bool> CancelAppointmentAsync(int appointmentId, CancelAppointmentDTO cancelAppointmentDTO)
		{
			var appointment = await _unitOfWork.Appointments.GetAsync(a => a.AppointmentId == appointmentId);
			if (appointment == null) return false;

			appointment.Status = "cancelled";
			appointment.CancellationReason = cancelAppointmentDTO.CancellationReason;
			appointment.CancelledAt = DateTime.UtcNow;
			appointment.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.Appointments.UpdateAsync(appointment);
			await _unitOfWork.CompleteAsync();

			return true;
		}

		public async Task<IEnumerable<AppointmentDTO>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
		{
			var appointments = await _unitOfWork.Appointments.GetAllAsync(
				filter: a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate,
				includeProperties: "User,Doctor.User"
			);

			return _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
		}

		public async Task<IEnumerable<AppointmentDTO>> GetPendingAppointmentsAsync()
		{
			var appointments = await _unitOfWork.Appointments.GetAllAsync(
				filter: a => a.Status == "pending",
				includeProperties: "User,Doctor.User"
			);

			return _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
		}

		public async Task<IEnumerable<AppointmentDTO>> GetTodaysAppointmentsAsync()
		{
			var today = DateTime.Today;
			var tomorrow = today.AddDays(1);

			var appointments = await _unitOfWork.Appointments.GetAllAsync(
				filter: a => a.AppointmentDate >= today &&
						   a.AppointmentDate < tomorrow &&
						   a.Status != "cancelled",
				includeProperties: "User,Doctor.User"
			);

			return _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
		}

		public async Task<bool> ConfirmAppointmentAsync(int appointmentId)
		{
			var appointment = await _unitOfWork.Appointments.GetAsync(a => a.AppointmentId == appointmentId);
			if (appointment == null || appointment.Status != "pending")
				return false;

			appointment.Status = "confirmed";
			appointment.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.Appointments.UpdateAsync(appointment);
			await _unitOfWork.CompleteAsync();
			return true;
		}

		public async Task<bool> CompleteAppointmentAsync(int appointmentId)
		{
			var appointment = await _unitOfWork.Appointments.GetAsync(a => a.AppointmentId == appointmentId);
			if (appointment == null || appointment.Status == "cancelled")
				return false;

			appointment.Status = "completed";
			appointment.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.Appointments.UpdateAsync(appointment);
			await _unitOfWork.CompleteAsync();
			return true;
		}

		public async Task<IEnumerable<AppointmentDTO>> GetAppointmentsByStatusAsync(string status)
		{
			var appointments = await _unitOfWork.Appointments.GetAllAsync(
				filter: a => a.Status == status,
				includeProperties: "User,Doctor.User"
			);

			return _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
		}
	}
}