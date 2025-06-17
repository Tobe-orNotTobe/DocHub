using DochubSystem.Data.Entities;
using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DochubSystem.Repository.Repositories
{
	public class NotificationTemplateRepository : Repository<NotificationTemplate>, INotificationTemplateRepository
	{
		private readonly DochubDbContext _context;

		public NotificationTemplateRepository(DochubDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<NotificationTemplate> GetByTypeAsync(string type)
		{
			return await _context.NotificationTemplates
				.FirstOrDefaultAsync(nt => nt.Type == type && nt.IsActive);
		}

		public async Task<IEnumerable<NotificationTemplate>> GetByTargetRoleAsync(string targetRole)
		{
			return await _context.NotificationTemplates
				.Where(nt => (nt.TargetRole == targetRole || nt.TargetRole == "all") && nt.IsActive)
				.ToListAsync();
		}

		public async Task<IEnumerable<NotificationTemplate>> GetActiveTemplatesAsync()
		{
			return await _context.NotificationTemplates
				.Where(nt => nt.IsActive)
				.OrderBy(nt => nt.Name)
				.ToListAsync();
		}

		public async Task<bool> ExistsByTypeAsync(string type)
		{
			return await _context.NotificationTemplates
				.AnyAsync(nt => nt.Type == type);
		}
	}

	public class NotificationQueueRepository : Repository<NotificationQueue>, INotificationQueueRepository
	{
		private readonly DochubDbContext _context;

		public NotificationQueueRepository(DochubDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<IEnumerable<NotificationQueue>> GetPendingNotificationsAsync(DateTime currentTime, int batchSize = 50)
		{
			return await _context.NotificationQueues
				.Where(nq => nq.Status == "pending" && nq.ScheduledAt <= currentTime)
				.Include(nq => nq.User)
				.Include(nq => nq.NotificationTemplate)
				.OrderBy(nq => nq.Priority == "urgent" ? 1 : nq.Priority == "high" ? 2 : 3)
				.ThenBy(nq => nq.ScheduledAt)
				.Take(batchSize)
				.ToListAsync();
		}

		public async Task<IEnumerable<NotificationQueue>> GetFailedNotificationsForRetryAsync(int maxRetryCount = 3)
		{
			return await _context.NotificationQueues
				.Where(nq => nq.Status == "failed" && nq.RetryCount < maxRetryCount)
				.Include(nq => nq.User)
				.Include(nq => nq.NotificationTemplate)
				.OrderBy(nq => nq.CreatedAt)
				.ToListAsync();
		}

		public async Task<bool> UpdateStatusAsync(int queueId, string status, string? errorMessage = null)
		{
			var queueItem = await _context.NotificationQueues
				.FirstOrDefaultAsync(nq => nq.QueueId == queueId);

			if (queueItem == null)
				return false;

			queueItem.Status = status;
			queueItem.ErrorMessage = errorMessage;

			if (status == "sent")
			{
				queueItem.SentAt = DateTime.UtcNow;
			}

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> IncrementRetryCountAsync(int queueId)
		{
			var queueItem = await _context.NotificationQueues
				.FirstOrDefaultAsync(nq => nq.QueueId == queueId);

			if (queueItem == null)
				return false;

			queueItem.RetryCount++;
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<int> GetQueueCountByStatusAsync(string status)
		{
			return await _context.NotificationQueues
				.CountAsync(nq => nq.Status == status);
		}

		public async Task<bool> ClearOldProcessedNotificationsAsync(DateTime cutoffDate)
		{
			var oldProcessed = await _context.NotificationQueues
				.Where(nq => (nq.Status == "sent" || nq.Status == "cancelled") && nq.CreatedAt < cutoffDate)
				.ToListAsync();

			if (!oldProcessed.Any())
				return false;

			_context.NotificationQueues.RemoveRange(oldProcessed);
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
