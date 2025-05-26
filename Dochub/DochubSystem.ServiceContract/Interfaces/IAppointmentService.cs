using DochubSystem.Data.DTOs;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface IAppointmentService
	{
		Task<AppointmentDTO> CreateAppointmentAsync(CreateAppointmentDTO createAppointmentDTO);
		Task<AppointmentDTO> GetAppointmentByIdAsync(int appointmentId);
	}
}
