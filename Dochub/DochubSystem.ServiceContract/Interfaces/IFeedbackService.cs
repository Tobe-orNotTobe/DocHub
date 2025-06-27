using DochubSystem.Data.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DochubSystem.Services
{
    public interface IFeedbackService
    {
        Task<FeedbackDTO> AddFeedback(CreateFeedbackDTO dto, string userId, string userName, string initials);
        Task<List<FeedbackDTO>> GetFeedbacksByDoctorId(int doctorId);
        Task<FeedbackDTO?> GetFeedbackById(int id);
        Task<FeedbackDTO?> UpdateFeedback(int id, FeedbackDTO dto);
        Task<bool> DeleteFeedback(int id);
    }
}
