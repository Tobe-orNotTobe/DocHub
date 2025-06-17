using DochubSystem.Data.Entities;

namespace DochubSystem.RepositoryContract.Interfaces
{
	public interface INotificationQueueRepository : IRepository<NotificationQueue>
	{
		Task<IEnumerable<NotificationQueue>> GetPendingNotificationsAsync(DateTime currentTime, int batchSize = 50);
		Task<IEnumerable<NotificationQueue>> GetFailedNotificationsForRetryAsync(int maxRetryCount = 3);
		Task<bool> UpdateStatusAsync(int queueId, string status, string? errorMessage = null);
		Task<bool> IncrementRetryCountAsync(int queueId);
		Task<int> GetQueueCountByStatusAsync(string status);
		Task<bool> ClearOldProcessedNotificationsAsync(DateTime cutoffDate);
	}
}
