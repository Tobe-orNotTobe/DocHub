using DochubSystem.Data.Entities;

namespace DochubSystem.RepositoryContract.Interfaces
{
	public interface INotificationTemplateRepository : IRepository<NotificationTemplate>
	{
		Task<NotificationTemplate> GetByTypeAsync(string type);
		Task<IEnumerable<NotificationTemplate>> GetByTargetRoleAsync(string targetRole);
		Task<IEnumerable<NotificationTemplate>> GetActiveTemplatesAsync();
		Task<bool> ExistsByTypeAsync(string type);
	}
}
