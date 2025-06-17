using DochubSystem.Data.Entities;

namespace DochubSystem.RepositoryContract.Interfaces
{
	public interface INotificationHistoryRepository : IRepository<NotificationHistory>
	{
		Task<IEnumerable<NotificationHistory>> GetUserHistoryAsync(string userId, int page = 1, int pageSize = 20);
		Task<NotificationHistory> GetByNotificationIdAsync(int notificationId);
		Task<IEnumerable<NotificationHistory>> GetHistoryByTypeAsync(string notificationType, DateTime? fromDate = null, DateTime? toDate = null);
		Task<Dictionary<string, int>> GetNotificationStatsByTypeAsync(DateTime fromDate, DateTime toDate);
		Task<bool> CleanupOldHistoryAsync(DateTime cutoffDate);
	}
}
