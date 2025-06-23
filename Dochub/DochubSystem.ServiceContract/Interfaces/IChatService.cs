using DochubSystem.Data.DTOs;

namespace DochubSystem.ServiceContract.Interfaces
{
    public interface IChatService
    {
        Task<ChatDTO> SaveMessageAsync(CreateChatWithUserDTO dto);
        Task<IEnumerable<ChatDTO>> GetMessagesByAppointmentAsync(int appointmentId);
    }
}
