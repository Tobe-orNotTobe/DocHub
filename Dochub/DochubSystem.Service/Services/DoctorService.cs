using AutoMapper;
using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using DochubSystem.RepositoryContract.Interfaces;
using DochubSystem.ServiceContract.Interfaces;

namespace DochubSystem.Service.Services
{
	public class DoctorService : IDoctorService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public DoctorService(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<DoctorDTO> CreateDoctorAsync(CreateDoctorDTO createDoctorDTO)
		{
			// Validate user exists
			var userExists = await _unitOfWork.Users.UserExistsAsync(createDoctorDTO.UserId);
			if (!userExists)
				throw new ArgumentException("User not found");

			// Check if user is already a doctor
			var existingDoctor = await _unitOfWork.Doctors.GetAsync(d => d.UserId == createDoctorDTO.UserId);
			if (existingDoctor != null)
				throw new InvalidOperationException("User is already registered as a doctor");

			// Check if license number is unique
			var licenseExists = await _unitOfWork.Doctors.AnyAsync(d => d.LicenseNumber == createDoctorDTO.LicenseNumber);
			if (licenseExists)
				throw new InvalidOperationException("License number already exists");

			var doctor = new Doctor
			{
				UserId = createDoctorDTO.UserId,
				Specialization = createDoctorDTO.Specialization,
				Experience = createDoctorDTO.Experience,
				LicenseNumber = createDoctorDTO.LicenseNumber,
				IsActive = createDoctorDTO.IsActive
			};

			var createdDoctor = await _unitOfWork.Doctors.AddAsync(doctor);
			await _unitOfWork.CompleteAsync();

			// Get the full doctor with user information
			var fullDoctor = await _unitOfWork.Doctors.GetAsync(
				d => d.DoctorId == createdDoctor.DoctorId,
				includeProperties: "User"
			);

			return _mapper.Map<DoctorDTO>(fullDoctor);
		}

		public async Task<DoctorDTO> GetDoctorByIdAsync(int doctorId)
		{
			var doctor = await _unitOfWork.Doctors.GetAsync(
				d => d.DoctorId == doctorId,
				includeProperties: "User"
			);

			if (doctor == null)
				return null;

			return _mapper.Map<DoctorDTO>(doctor);
		}

		public async Task<IEnumerable<DoctorDTO>> GetAllDoctorsAsync()
		{
			var doctors = await _unitOfWork.Doctors.GetAllAsync(
				includeProperties: "User"
			);

			return _mapper.Map<IEnumerable<DoctorDTO>>(doctors);
		}

		public async Task<IEnumerable<DoctorSummaryDTO>> GetAllActiveDoctorsAsync()
		{
			var doctors = await _unitOfWork.Doctors.GetAllAsync(
				filter: d => d.IsActive,
				includeProperties: "User"
			);

			return _mapper.Map<IEnumerable<DoctorSummaryDTO>>(doctors);
		}

		public async Task<DoctorDTO> UpdateDoctorAsync(int doctorId, UpdateDoctorDTO updateDoctorDTO)
		{
			var doctor = await _unitOfWork.Doctors.GetAsync(
				d => d.DoctorId == doctorId,
				includeProperties: "User"
			);

			if (doctor == null)
				throw new ArgumentException("Doctor not found");

			// Check license number uniqueness if it's being updated
			if (!string.IsNullOrEmpty(updateDoctorDTO.LicenseNumber) &&
				updateDoctorDTO.LicenseNumber != doctor.LicenseNumber)
			{
				var licenseExists = await _unitOfWork.Doctors.AnyAsync(
					d => d.LicenseNumber == updateDoctorDTO.LicenseNumber && d.DoctorId != doctorId);
				if (licenseExists)
					throw new InvalidOperationException("License number already exists");
			}

			// Update properties
			if (!string.IsNullOrEmpty(updateDoctorDTO.Specialization))
				doctor.Specialization = updateDoctorDTO.Specialization;

			if (updateDoctorDTO.Experience.HasValue)
				doctor.Experience = updateDoctorDTO.Experience.Value;

			if (!string.IsNullOrEmpty(updateDoctorDTO.LicenseNumber))
				doctor.LicenseNumber = updateDoctorDTO.LicenseNumber;

			if (updateDoctorDTO.IsActive.HasValue)
				doctor.IsActive = updateDoctorDTO.IsActive.Value;

			await _unitOfWork.Doctors.UpdateAsync(doctor);
			await _unitOfWork.CompleteAsync();

			return _mapper.Map<DoctorDTO>(doctor);
		}

		public async Task<bool> DeleteDoctorAsync(int doctorId)
		{
			var doctor = await _unitOfWork.Doctors.GetAsync(d => d.DoctorId == doctorId);
			if (doctor == null)
				return false;

			// Check if doctor has any appointments
			var hasAppointments = await _unitOfWork.Appointments.AnyAsync(a => a.DoctorId == doctorId);
			if (hasAppointments)
			{
				// Soft delete - just deactivate the doctor
				doctor.IsActive = false;
				await _unitOfWork.Doctors.UpdateAsync(doctor);
			}
			else
			{
				// Hard delete if no appointments
				await _unitOfWork.Doctors.DeleteAsync(doctor);
			}

			await _unitOfWork.CompleteAsync();
			return true;
		}

		public async Task<DoctorDTO> GetDoctorByUserIdAsync(string userId)
		{
			var doctor = await _unitOfWork.Doctors.GetAsync(
				d => d.UserId == userId,
				includeProperties: "User"
			);

			if (doctor == null)
				return null;

			return _mapper.Map<DoctorDTO>(doctor);
		}

		public async Task<IEnumerable<DoctorSummaryDTO>> GetDoctorsBySpecializationAsync(string specialization)
		{
			var doctors = await _unitOfWork.Doctors.GetAllAsync(
				filter: d => d.IsActive && d.Specialization.ToLower().Contains(specialization.ToLower()),
				includeProperties: "User"
			);

			return _mapper.Map<IEnumerable<DoctorSummaryDTO>>(doctors);
		}

		public async Task<bool> DoctorExistsAsync(int doctorId)
		{
			return await _unitOfWork.Doctors.AnyAsync(d => d.DoctorId == doctorId);
		}

		public async Task<bool> IsLicenseNumberUniqueAsync(string licenseNumber, int? excludeDoctorId = null)
		{
			if (excludeDoctorId.HasValue)
			{
				return !await _unitOfWork.Doctors.AnyAsync(
					d => d.LicenseNumber == licenseNumber && d.DoctorId != excludeDoctorId.Value);
			}

			return !await _unitOfWork.Doctors.AnyAsync(d => d.LicenseNumber == licenseNumber);
		}
	}
}