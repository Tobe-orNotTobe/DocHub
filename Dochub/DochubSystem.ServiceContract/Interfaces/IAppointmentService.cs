using DochubSystem.Data.DTOs;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface IAppointmentService
	{
		Task<AppointmentDTO> CreateAppointmentAsync(string userId, CreateAppointmentDTO createAppointmentDTO);
		Task<AppointmentDTO> GetAppointmentByIdAsync(int appointmentId);
	}
}
