using DocHubSystem.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocHubSystem.Repositories
{
    public interface IFeedbackRepository
    {
        Task<Feedback> AddAsync(Feedback feedback);
        Task<List<Feedback>> GetByDoctorIdAsync(int doctorId);
        Task<Feedback?> GetByIdAsync(int id);
        Task<Feedback?> UpdateAsync(Feedback feedback);
        Task<bool> DeleteAsync(int id);
    }
}
