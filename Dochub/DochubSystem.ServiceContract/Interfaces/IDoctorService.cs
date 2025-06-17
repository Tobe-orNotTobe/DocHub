using DochubSystem.Data.DTOs;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface IDoctorService
	{
		Task<DoctorDTO> CreateDoctorAsync(CreateDoctorDTO createDoctorDTO);
		Task<DoctorDTO> GetDoctorByIdAsync(int doctorId);
		Task<IEnumerable<DoctorDTO>> GetAllDoctorsAsync();
		Task<IEnumerable<DoctorSummaryDTO>> GetAllActiveDoctorsAsync();
		Task<DoctorDTO> UpdateDoctorAsync(int doctorId, UpdateDoctorDTO updateDoctorDTO);
		Task<bool> DeleteDoctorAsync(int doctorId);
		Task<DoctorDTO> GetDoctorByUserIdAsync(string userId);
		Task<IEnumerable<DoctorSummaryDTO>> GetDoctorsBySpecializationAsync(string specialization);
		Task<bool> DoctorExistsAsync(int doctorId);
	}
}
