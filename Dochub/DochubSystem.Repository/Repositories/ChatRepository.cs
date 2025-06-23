using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DochubSystem.Repository.Repositories
{
    public class ChatRepository : Repository<Chat>, IChatRepository
    {
        private readonly DochubDbContext _context;

        public ChatRepository(DochubDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Chat>> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _context.Chats
                .Where(c => c.AppointmentId == appointmentId)
                .OrderBy(c => c.Timestamp)
                .ToListAsync();
        }
    }
}
