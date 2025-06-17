using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DochubSystem.Repository.Repositories
{
	public class NotificationHistoryRepository : Repository<NotificationHistory>, INotificationHistoryRepository
	{
		private readonly DochubDbContext _context;

		public NotificationHistoryRepository(DochubDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<IEnumerable<NotificationHistory>> GetUserHistoryAsync(string userId, int page = 1, int pageSize = 20)
		{
			return await _context.NotificationHistories
				.Where(nh => nh.UserId == userId)
				.Include(nh => nh.NotificationTemplate)
				.OrderByDescending(nh => nh.SentAt)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();
		}

		public async Task<NotificationHistory> GetByNotificationIdAsync(int notificationId)
		{
			return await _context.NotificationHistories
				.Include(nh => nh.NotificationTemplate)
				.FirstOrDefaultAsync(nh => nh.RelatedEntityType == "notification" && nh.RelatedEntityId == notificationId.ToString());
		}

		public async Task<IEnumerable<NotificationHistory>> GetHistoryByTypeAsync(string notificationType, DateTime? fromDate = null, DateTime? toDate = null)
		{
			var query = _context.NotificationHistories
				.Where(nh => nh.NotificationType == notificationType)
				.AsQueryable();

			if (fromDate.HasValue)
				query = query.Where(nh => nh.SentAt >= fromDate.Value);

			if (toDate.HasValue)
				query = query.Where(nh => nh.SentAt <= toDate.Value);

			return await query
				.OrderByDescending(nh => nh.SentAt)
				.ToListAsync();
		}

		public async Task<Dictionary<string, int>> GetNotificationStatsByTypeAsync(DateTime fromDate, DateTime toDate)
		{
			return await _context.NotificationHistories
				.Where(nh => nh.SentAt >= fromDate && nh.SentAt <= toDate)
				.GroupBy(nh => nh.NotificationType)
				.ToDictionaryAsync(g => g.Key, g => g.Count());
		}

		public async Task<bool> CleanupOldHistoryAsync(DateTime cutoffDate)
		{
			var oldHistory = await _context.NotificationHistories
				.Where(nh => nh.SentAt < cutoffDate)
				.ToListAsync();

			if (!oldHistory.Any())
				return false;

			_context.NotificationHistories.RemoveRange(oldHistory);
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
