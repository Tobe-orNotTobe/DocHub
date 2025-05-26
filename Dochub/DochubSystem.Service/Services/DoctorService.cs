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
	}
}