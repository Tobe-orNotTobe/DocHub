using DochubSystem.Data.DTOs;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface IDoctorService
	{
		Task<DoctorDTO> CreateDoctorAsync(CreateDoctorDTO createDoctorDTO);
		Task<DoctorDTO> GetDoctorByIdAsync(int doctorId);
	}
}
