using DochubSystem.Data.Entities;

namespace DochubSystem.RepositoryContract.Interfaces
{
	public interface IUserSubscriptionRepository : IRepository<UserSubscription>
	{
		Task<UserSubscription> GetActiveSubscriptionByUserIdAsync(string userId);
		Task<IEnumerable<UserSubscription>> GetExpiredSubscriptionsAsync();
		Task<IEnumerable<UserSubscription>> GetSubscriptionsForRenewalAsync();
		Task<IEnumerable<UserSubscription>> GetPendingPlanChangesAsync();
		Task<UserSubscription> GetSubscriptionWithDetailsAsync(int subscriptionId);
	}
}
