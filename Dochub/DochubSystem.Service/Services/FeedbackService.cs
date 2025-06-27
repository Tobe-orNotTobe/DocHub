using AutoMapper;
using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DocHubSystem.Data.Entities;
using DocHubSystem.Repositories;

namespace DochubSystem.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repo;
        private readonly IMapper _mapper;

        public FeedbackService(IFeedbackRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<FeedbackDTO> AddFeedback(CreateFeedbackDTO dto, string userId, string userName, string initials)
        {
            var entity = new Feedback
            {
                DoctorId = dto.DoctorId,
                UserId = userId,
                Name = userName,
                Initials = initials,
                Content = dto.Content,
                Rating = dto.Rating,
                Date = DateTime.UtcNow
            };

            var result = await _repo.AddAsync(entity);
            return _mapper.Map<FeedbackDTO>(result);
        }

        public async Task<List<FeedbackDTO>> GetFeedbacksByDoctorId(int doctorId)
        {
            var list = await _repo.GetByDoctorIdAsync(doctorId);
            return _mapper.Map<List<FeedbackDTO>>(list);
        }

        public async Task<FeedbackDTO?> GetFeedbackById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity != null ? _mapper.Map<FeedbackDTO>(entity) : null;
        }

        public async Task<FeedbackDTO?> UpdateFeedback(int id, FeedbackDTO dto)
        {
            var entity = new Feedback
            {
                FeedbackId = id,
                Content = dto.Content,
                Rating = dto.Rating
            };

            var updated = await _repo.UpdateAsync(entity);
            return updated != null ? _mapper.Map<FeedbackDTO>(updated) : null;
        }

        public async Task<bool> DeleteFeedback(int id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}
