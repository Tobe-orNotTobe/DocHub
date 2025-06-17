using DochubSystem.Data.Entities;

namespace DochubSystem.RepositoryContract.Interfaces
{
	public interface IConsultationUsageRepository : IRepository<ConsultationUsage>
	{
		Task<IEnumerable<ConsultationUsage>> GetUsageByUserIdAsync(string userId);
		Task<IEnumerable<ConsultationUsage>> GetUsageBySubscriptionIdAsync(int subscriptionId);
		Task<int> GetUsageCountForCurrentPeriodAsync(string userId);
	}
}
