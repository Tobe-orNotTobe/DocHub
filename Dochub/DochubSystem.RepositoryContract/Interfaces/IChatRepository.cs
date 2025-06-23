using DochubSystem.Data.Entities;

namespace DochubSystem.RepositoryContract.Interfaces
{
    public interface IChatRepository : IRepository<Chat>
    {
        Task<IEnumerable<Chat>> GetByAppointmentIdAsync(int appointmentId);
    }
}
