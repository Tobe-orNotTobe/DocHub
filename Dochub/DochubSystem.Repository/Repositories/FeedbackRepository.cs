using DochubSystem.Data.Models;
using DocHubSystem.Data;
using DocHubSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocHubSystem.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly DochubDbContext _context;

        public FeedbackRepository(DochubDbContext context)
        {
            _context = context;
        }

        public async Task<Feedback> AddAsync(Feedback feedback)
        {
            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<List<Feedback>> GetByDoctorIdAsync(int doctorId)
        {
            return await _context.Feedbacks
                .Where(x => x.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<Feedback?> GetByIdAsync(int id)
        {
            return await _context.Feedbacks.FindAsync(id);
        }

        public async Task<Feedback?> UpdateAsync(Feedback feedback)
        {
            var existing = await _context.Feedbacks.FindAsync(feedback.FeedbackId);
            if (existing == null) return null;

            existing.Content = feedback.Content;
            existing.Rating = feedback.Rating;
            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Feedbacks.FindAsync(id);
            if (entity == null) return false;

            _context.Feedbacks.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
