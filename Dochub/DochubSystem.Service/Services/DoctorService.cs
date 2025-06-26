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

			var doctor = new Doctor
			{
				UserId = createDoctorDTO.UserId,
				Specialization = createDoctorDTO.Specialization ?? "Need Update",
				YearsOfExperience = createDoctorDTO.YearsOfExperience,
				Bio = createDoctorDTO.Bio ?? "Need Update",
				HospitalName = createDoctorDTO.HospitalName ?? "Need Update",
				Rating = null, 
				IsActive = createDoctorDTO.IsActive,
                ImageDoctor = createDoctorDTO.ImageDoctor // ✅ mới thêm

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

			// Update properties
			if (!string.IsNullOrEmpty(updateDoctorDTO.Specialization))
				doctor.Specialization = updateDoctorDTO.Specialization;

			if (updateDoctorDTO.YearsOfExperience.HasValue)
				doctor.YearsOfExperience = updateDoctorDTO.YearsOfExperience.Value;

			if (!string.IsNullOrEmpty(updateDoctorDTO.Bio))
				doctor.Bio = updateDoctorDTO.Bio;

			if (!string.IsNullOrEmpty(updateDoctorDTO.HospitalName))
				doctor.HospitalName = updateDoctorDTO.HospitalName;

			if (updateDoctorDTO.IsActive.HasValue)
				doctor.IsActive = updateDoctorDTO.IsActive.Value;

            if (!string.IsNullOrEmpty(updateDoctorDTO.ImageDoctor)) // ✅ mới thêm
                doctor.ImageDoctor = updateDoctorDTO.ImageDoctor;

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
	}
}