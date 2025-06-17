using DochubSystem.Data.DTOs;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface ISubscriptionService
	{
		// Subscription Management
		Task<UserSubscriptionDTO> CreateSubscriptionAsync(CreateSubscriptionDTO createSubscriptionDTO);
		Task<UserSubscriptionDTO> GetUserSubscriptionAsync(string userId);
		Task<UserSubscriptionDTO> ChangePlanAsync(string userId, ChangePlanDTO changePlanDTO);

		// Plan Management
		Task<IEnumerable<SubscriptionPlanDTO>> GetAllPlansAsync();
		Task<SubscriptionPlanDTO> GetPlanByIdAsync(int planId);
		Task<SubscriptionPlanDTO> CreatePlanAsync(CreateSubscriptionPlanDTO createPlanDTO);
		Task<SubscriptionPlanDTO> UpdatePlanAsync(int planId, CreateSubscriptionPlanDTO updatePlanDTO);
		Task<bool> DeletePlanAsync(int planId);

		// Usage Tracking
		Task<bool> CanUserConsultAsync(string userId);
		Task<bool> RecordConsultationUsageAsync(string userId, int appointmentId, string consultationType);
		Task<SubscriptionUsageDTO> GetUserUsageAsync(string userId);

		// Subscription Privileges
		Task<bool> HasPrivilegeAsync(string userId, string privilege);
		Task<bool> CanAccessDoctorAsync(string userId, int doctorId);
		Task<decimal> GetDiscountPercentageAsync(string userId);

		// Admin Functions
		Task<IEnumerable<UserSubscriptionDTO>> GetAllSubscriptionsAsync();
		Task ProcessExpiredSubscriptionsAsync();
	}
}
