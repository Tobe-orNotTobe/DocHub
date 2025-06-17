using DochubSystem.Data.DTOs;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface IAppointmentService
	{
		Task<AppointmentDTO> CreateAppointmentAsync(string userId, CreateAppointmentDTO createAppointmentDTO);
		Task<AppointmentDTO> GetAppointmentByIdAsync(int appointmentId);
		Task<IEnumerable<AppointmentDTO>> GetAppointmentsByUserIdAsync(string userId);
		Task<IEnumerable<AppointmentDTO>> GetAppointmentsByDoctorIdAsync(int doctorId);
		Task<IEnumerable<AppointmentSummaryDTO>> GetUpcomingAppointmentsAsync(string userId);
		Task<AppointmentDTO> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentDTO updateAppointmentDTO);
		Task<bool> CancelAppointmentAsync(int appointmentId, CancelAppointmentDTO cancelAppointmentDTO);
		Task<IEnumerable<AppointmentDTO>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
		Task<IEnumerable<AppointmentDTO>> GetPendingAppointmentsAsync();
		Task<IEnumerable<AppointmentDTO>> GetTodaysAppointmentsAsync();
		Task<bool> ConfirmAppointmentAsync(int appointmentId);
		Task<bool> CompleteAppointmentAsync(int appointmentId);
		Task<IEnumerable<AppointmentDTO>> GetAppointmentsByStatusAsync(string status);
	}
}
