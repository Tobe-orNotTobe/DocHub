using AutoMapper;
using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using DochubSystem.RepositoryContract.Interfaces;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.Extensions.Logging;

namespace DochubSystem.Service.Services
{
	public class SubscriptionService : ISubscriptionService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly ILogger<SubscriptionService> _logger;

		public SubscriptionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SubscriptionService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_logger = logger;
		}

		#region Subscription Management

		public async Task<UserSubscriptionDTO> CreateSubscriptionAsync(CreateSubscriptionDTO createSubscriptionDTO)
		{
			_logger.LogInformation("Creating subscription for user {UserId} with plan {PlanId}",
				createSubscriptionDTO.UserId, createSubscriptionDTO.PlanId);

			// Validate user exists
			var userExists = await _unitOfWork.Users.UserExistsAsync(createSubscriptionDTO.UserId);
			if (!userExists)
			{
				_logger.LogWarning("User not found: {UserId}", createSubscriptionDTO.UserId);
				throw new ArgumentException("User not found");
			}

			// Validate plan exists and is active
			var plan = await _unitOfWork.SubscriptionPlans.GetAsync(p => p.PlanId == createSubscriptionDTO.PlanId && p.IsActive);
			if (plan == null)
			{
				_logger.LogWarning("Subscription plan not found or inactive: {PlanId}", createSubscriptionDTO.PlanId);
				throw new ArgumentException("Subscription plan not found or inactive");
			}

			// Check if user already has an active subscription
			var existingSubscription = await _unitOfWork.UserSubscriptions.GetAsync(
				s => s.UserId == createSubscriptionDTO.UserId && s.Status == "Active");

			if (existingSubscription != null)
			{
				_logger.LogWarning("User {UserId} already has an active subscription", createSubscriptionDTO.UserId);
				throw new InvalidOperationException("User already has an active subscription");
			}

			using var transaction = await _unitOfWork.BeginTransactionAsync();
			try
			{
				var startDate = DateTime.UtcNow;
				var endDate = createSubscriptionDTO.BillingCycle == "Yearly"
					? startDate.AddYears(1)
					: startDate.AddMonths(1);

				var amount = createSubscriptionDTO.BillingCycle == "Yearly"
					? plan.YearlyPrice
					: plan.MonthlyPrice;

				// Create subscription
				var subscription = new UserSubscription
				{
					UserId = createSubscriptionDTO.UserId,
					PlanId = createSubscriptionDTO.PlanId,
					StartDate = startDate,
					EndDate = endDate,
					Status = "Active",
					BillingCycle = createSubscriptionDTO.BillingCycle,
					PaidAmount = amount,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow,
					ConsultationsUsed = 0
				};

				var createdSubscription = await _unitOfWork.UserSubscriptions.AddAsync(subscription);

				await _unitOfWork.CompleteAsync();
				await transaction.CommitAsync();

				_logger.LogInformation("Subscription created successfully for user {UserId}", createSubscriptionDTO.UserId);

				// Get full subscription with plan details
				var fullSubscription = await _unitOfWork.UserSubscriptions.GetAsync(
					s => s.SubscriptionId == createdSubscription.SubscriptionId,
					includeProperties: "User,SubscriptionPlan");

				var result = _mapper.Map<UserSubscriptionDTO>(fullSubscription);
				CalculateRemainingConsultations(result, fullSubscription);

				return result;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Error creating subscription for user {UserId}", createSubscriptionDTO.UserId);
				throw;
			}
		}

		public async Task<UserSubscriptionDTO> GetUserSubscriptionAsync(string userId)
		{
			_logger.LogDebug("Getting subscription for user {UserId}", userId);

			var subscription = await _unitOfWork.UserSubscriptions.GetAsync(
				s => s.UserId == userId && s.Status == "Active",
				includeProperties: "User,SubscriptionPlan,PendingPlan");

			if (subscription == null)
			{
				_logger.LogDebug("No active subscription found for user {UserId}", userId);
				return null;
			}

			var dto = _mapper.Map<UserSubscriptionDTO>(subscription);
			CalculateRemainingConsultations(dto, subscription);

			return dto;
		}

		public async Task<UserSubscriptionDTO> ChangePlanAsync(string userId, ChangePlanDTO changePlanDTO)
		{
			_logger.LogInformation("Changing plan for user {UserId} to plan {NewPlanId}. Immediate: {Immediate}",
				userId, changePlanDTO.NewPlanId, changePlanDTO.EffectiveImmediately);

			var subscription = await _unitOfWork.UserSubscriptions.GetAsync(
				s => s.UserId == userId && s.Status == "Active",
				includeProperties: "SubscriptionPlan");

			if (subscription == null)
			{
				_logger.LogWarning("No active subscription found for user {UserId}", userId);
				throw new ArgumentException("No active subscription found for user");
			}

			var newPlan = await _unitOfWork.SubscriptionPlans.GetAsync(
				p => p.PlanId == changePlanDTO.NewPlanId && p.IsActive);

			if (newPlan == null)
			{
				_logger.LogWarning("New subscription plan not found or inactive: {PlanId}", changePlanDTO.NewPlanId);
				throw new ArgumentException("New subscription plan not found or inactive");
			}

			if (subscription.PlanId == changePlanDTO.NewPlanId)
			{
				_logger.LogWarning("User {UserId} is already on plan {PlanId}", userId, changePlanDTO.NewPlanId);
				throw new ArgumentException("User is already on this plan");
			}

			using var transaction = await _unitOfWork.BeginTransactionAsync();
			try
			{
				if (changePlanDTO.EffectiveImmediately)
				{
					// Calculate prorated amount
					var currentPlan = subscription.SubscriptionPlan;
					var remainingDays = (subscription.EndDate - DateTime.UtcNow).Days;
					var totalDays = subscription.BillingCycle == "Yearly" ? 365 : 30;

					var newAmount = subscription.BillingCycle == "Yearly" ? newPlan.YearlyPrice : newPlan.MonthlyPrice;
					var currentAmount = subscription.BillingCycle == "Yearly" ? currentPlan.YearlyPrice : currentPlan.MonthlyPrice;

					var proratedDifference = (newAmount - currentAmount) * remainingDays / totalDays;

					// Update subscription
					subscription.PlanId = changePlanDTO.NewPlanId;
					subscription.UpdatedAt = DateTime.UtcNow;
					subscription.ConsultationsUsed = 0; // Reset usage for new plan

					_logger.LogInformation("Plan changed immediately for user {UserId} from plan {OldPlan} to {NewPlan}",
						userId, currentPlan.Name, newPlan.Name);
				}
				else
				{
					// Schedule plan change for next billing period
					subscription.PendingPlanId = changePlanDTO.NewPlanId;
					subscription.PlanChangeEffectiveDate = subscription.EndDate;
					subscription.UpdatedAt = DateTime.UtcNow;

					_logger.LogInformation("Plan change scheduled for user {UserId} to take effect on {EffectiveDate}",
						userId, subscription.EndDate);
				}

				await _unitOfWork.UserSubscriptions.UpdateAsync(subscription);
				await _unitOfWork.CompleteAsync();
				await transaction.CommitAsync();

				// Get updated subscription
				var updatedSubscription = await _unitOfWork.UserSubscriptions.GetAsync(
					s => s.SubscriptionId == subscription.SubscriptionId,
					includeProperties: "User,SubscriptionPlan,PendingPlan");

				var result = _mapper.Map<UserSubscriptionDTO>(updatedSubscription);
				CalculateRemainingConsultations(result, updatedSubscription);

				return result;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Error changing plan for user {UserId}", userId);
				throw;
			}
		}

		#endregion

		#region Plan Management

		public async Task<IEnumerable<SubscriptionPlanDTO>> GetAllPlansAsync()
		{
			_logger.LogDebug("Getting all active subscription plans");

			var plans = await _unitOfWork.SubscriptionPlans.GetAllAsync(p => p.IsActive);
			return _mapper.Map<IEnumerable<SubscriptionPlanDTO>>(plans);
		}

		public async Task<SubscriptionPlanDTO> GetPlanByIdAsync(int planId)
		{
			_logger.LogDebug("Getting subscription plan {PlanId}", planId);

			var plan = await _unitOfWork.SubscriptionPlans.GetAsync(p => p.PlanId == planId);
			return _mapper.Map<SubscriptionPlanDTO>(plan);
		}

		public async Task<SubscriptionPlanDTO> CreatePlanAsync(CreateSubscriptionPlanDTO createPlanDTO)
		{
			_logger.LogInformation("Creating new subscription plan: {PlanName}", createPlanDTO.Name);

			// Check if plan with same name already exists
			var existingPlan = await _unitOfWork.SubscriptionPlans.GetAsync(p => p.Name.ToLower() == createPlanDTO.Name.ToLower());
			if (existingPlan != null)
			{
				_logger.LogWarning("Plan with name '{PlanName}' already exists", createPlanDTO.Name);
				throw new ArgumentException($"Plan with name '{createPlanDTO.Name}' already exists");
			}

			var plan = _mapper.Map<SubscriptionPlan>(createPlanDTO);
			plan.CreatedAt = DateTime.UtcNow;
			plan.UpdatedAt = DateTime.UtcNow;
			plan.IsActive = true;

			var createdPlan = await _unitOfWork.SubscriptionPlans.AddAsync(plan);
			await _unitOfWork.CompleteAsync();

			_logger.LogInformation("Subscription plan created successfully: {PlanId} - {PlanName}",
				createdPlan.PlanId, createdPlan.Name);

			return _mapper.Map<SubscriptionPlanDTO>(createdPlan);
		}

		public async Task<SubscriptionPlanDTO> UpdatePlanAsync(int planId, CreateSubscriptionPlanDTO updatePlanDTO)
		{
			_logger.LogInformation("Updating subscription plan {PlanId}", planId);

			var plan = await _unitOfWork.SubscriptionPlans.GetAsync(p => p.PlanId == planId);
			if (plan == null)
			{
				_logger.LogWarning("Plan not found for update: {PlanId}", planId);
				throw new ArgumentException("Plan not found");
			}

			// Check if another plan with same name exists
			var existingPlan = await _unitOfWork.SubscriptionPlans.GetAsync(
				p => p.Name.ToLower() == updatePlanDTO.Name.ToLower() && p.PlanId != planId);
			if (existingPlan != null)
			{
				_logger.LogWarning("Another plan with name '{PlanName}' already exists", updatePlanDTO.Name);
				throw new ArgumentException($"Another plan with name '{updatePlanDTO.Name}' already exists");
			}

			_mapper.Map(updatePlanDTO, plan);
			plan.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.SubscriptionPlans.UpdateAsync(plan);
			await _unitOfWork.CompleteAsync();

			_logger.LogInformation("Subscription plan updated successfully: {PlanId} - {PlanName}",
				plan.PlanId, plan.Name);

			return _mapper.Map<SubscriptionPlanDTO>(plan);
		}

		public async Task<bool> DeletePlanAsync(int planId)
		{
			_logger.LogInformation("Deleting (deactivating) subscription plan {PlanId}", planId);

			var plan = await _unitOfWork.SubscriptionPlans.GetAsync(p => p.PlanId == planId);
			if (plan == null)
			{
				_logger.LogWarning("Plan not found for deletion: {PlanId}", planId);
				return false;
			}

			// Check if plan has active subscriptions
			var activeSubscriptions = await _unitOfWork.UserSubscriptions.AnyAsync(
				s => s.PlanId == planId && s.Status == "Active");

			if (activeSubscriptions)
			{
				_logger.LogWarning("Cannot delete plan {PlanId} - has active subscriptions", planId);
				throw new InvalidOperationException("Cannot delete plan with active subscriptions");
			}

			// Soft delete - just mark as inactive
			plan.IsActive = false;
			plan.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.SubscriptionPlans.UpdateAsync(plan);
			await _unitOfWork.CompleteAsync();

			_logger.LogInformation("Subscription plan deactivated successfully: {PlanId}", planId);

			return true;
		}

		#endregion

		#region Usage Tracking

		public async Task<bool> CanUserConsultAsync(string userId)
		{
			_logger.LogDebug("Checking if user {UserId} can consult", userId);

			var subscription = await _unitOfWork.UserSubscriptions.GetAsync(
				s => s.UserId == userId && s.Status == "Active",
				includeProperties: "SubscriptionPlan");

			if (subscription == null)
			{
				_logger.LogDebug("User {UserId} has no active subscription", userId);
				return false;
			}

			// Check if subscription is expired
			if (subscription.EndDate < DateTime.UtcNow)
			{
				_logger.LogDebug("User {UserId} subscription is expired", userId);
				return false;
			}

			// Check consultation limits
			if (subscription.SubscriptionPlan.ConsultationsPerMonth == -1)
			{
				_logger.LogDebug("User {UserId} has unlimited consultations", userId);
				return true; // Unlimited
			}

			var canConsult = subscription.ConsultationsUsed < subscription.SubscriptionPlan.ConsultationsPerMonth;
			_logger.LogDebug("User {UserId} can consult: {CanConsult} (Used: {Used}, Limit: {Limit})",
				userId, canConsult, subscription.ConsultationsUsed, subscription.SubscriptionPlan.ConsultationsPerMonth);

			return canConsult;
		}

		public async Task<bool> RecordConsultationUsageAsync(string userId, int appointmentId, string consultationType)
		{
			_logger.LogInformation("Recording consultation usage for user {UserId}, appointment {AppointmentId}, type {Type}",
				userId, appointmentId, consultationType);

			var subscription = await _unitOfWork.UserSubscriptions.GetAsync(
				s => s.UserId == userId && s.Status == "Active");

			if (subscription == null)
			{
				_logger.LogWarning("No active subscription found for user {UserId}", userId);
				return false;
			}

			// Check if usage already recorded for this appointment
			var existingUsage = await _unitOfWork.ConsultationUsages.GetAsync(
				u => u.AppointmentId == appointmentId && u.UserId == userId);

			if (existingUsage != null)
			{
				_logger.LogWarning("Usage already recorded for appointment {AppointmentId}", appointmentId);
				return false;
			}

			using var transaction = await _unitOfWork.BeginTransactionAsync();
			try
			{
				// Increment usage
				subscription.ConsultationsUsed++;
				subscription.LastConsultationDate = DateTime.UtcNow;
				subscription.UpdatedAt = DateTime.UtcNow;

				// Record detailed usage
				var usage = new ConsultationUsage
				{
					SubscriptionId = subscription.SubscriptionId,
					AppointmentId = appointmentId,
					UserId = userId,
					UsageDate = DateTime.UtcNow,
					ConsultationType = consultationType
				};

				await _unitOfWork.ConsultationUsages.AddAsync(usage);
				await _unitOfWork.UserSubscriptions.UpdateAsync(subscription);
				await _unitOfWork.CompleteAsync();
				await transaction.CommitAsync();

				_logger.LogInformation("Consultation usage recorded successfully for user {UserId}. Total used: {Used}",
					userId, subscription.ConsultationsUsed);

				return true;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Error recording consultation usage for user {UserId}", userId);
				throw;
			}
		}

		public async Task<SubscriptionUsageDTO> GetUserUsageAsync(string userId)
		{
			_logger.LogDebug("Getting usage for user {UserId}", userId);

			var subscription = await _unitOfWork.UserSubscriptions.GetAsync(
				s => s.UserId == userId && s.Status == "Active",
				includeProperties: "SubscriptionPlan");

			if (subscription == null)
			{
				_logger.LogDebug("No active subscription found for user {UserId}", userId);
				return null;
			}

			var recentUsage = await _unitOfWork.ConsultationUsages.GetAllAsync(
				u => u.UserId == userId && u.SubscriptionId == subscription.SubscriptionId,
				includeProperties: "Appointment.Doctor.User");

			var usageDetails = recentUsage.OrderByDescending(u => u.UsageDate)
				.Take(10)
				.Select(u => new ConsultationUsageDetailDTO
				{
					UsageId = u.UsageId,
					AppointmentId = u.AppointmentId,
					UsageDate = u.UsageDate,
					ConsultationType = u.ConsultationType,
					DoctorName = u.Appointment?.Doctor?.User?.FullName ?? "Unknown"
				}).ToList();

			return new SubscriptionUsageDTO
			{
				ConsultationsUsed = subscription.ConsultationsUsed,
				ConsultationsRemaining = subscription.SubscriptionPlan.ConsultationsPerMonth == -1
					? -1
					: Math.Max(0, subscription.SubscriptionPlan.ConsultationsPerMonth - subscription.ConsultationsUsed),
				LastConsultationDate = subscription.LastConsultationDate,
				RecentUsage = usageDetails
			};
		}

		#endregion

		#region Subscription Privileges

		public async Task<bool> HasPrivilegeAsync(string userId, string privilege)
		{
			_logger.LogDebug("Checking privilege {Privilege} for user {UserId}", privilege, userId);

			var subscription = await _unitOfWork.UserSubscriptions.GetAsync(
				s => s.UserId == userId && s.Status == "Active",
				includeProperties: "SubscriptionPlan");

			if (subscription == null || subscription.EndDate < DateTime.UtcNow)
			{
				_logger.LogDebug("User {UserId} has no active subscription", userId);
				return false;
			}

			var plan = subscription.SubscriptionPlan;
			var hasPrivilege = privilege.ToLower() switch
			{
				"videocall" => plan.HasVideoCallSupport,
				"prioritybooking" => plan.HasPriorityBooking,
				"24x7support" => plan.Has24x7Support,
				"medicationreminders" => plan.HasMedicationReminders,
				"healthreports" => plan.HasHealthReports,
				"basicmedicalinfo" => plan.HasBasicMedicalInfo,
				"medicalrecordstorage" => plan.HasMedicalRecordStorage,
				"examreminders" => plan.HasExamReminders,
				_ => false
			};

			_logger.LogDebug("User {UserId} has privilege {Privilege}: {HasPrivilege}", userId, privilege, hasPrivilege);
			return hasPrivilege;
		}

		public async Task<bool> CanAccessDoctorAsync(string userId, int doctorId)
		{
			_logger.LogDebug("Checking if user {UserId} can access doctor {DoctorId}", userId, doctorId);

			var subscription = await _unitOfWork.UserSubscriptions.GetAsync(
				s => s.UserId == userId && s.Status == "Active",
				includeProperties: "SubscriptionPlan");

			if (subscription == null || subscription.EndDate < DateTime.UtcNow)
			{
				_logger.LogDebug("User {UserId} has no active subscription", userId);
				return false;
			}

			// If unlimited access to all doctors
			if (subscription.SubscriptionPlan.MaxDoctorsAccess == -1)
			{
				_logger.LogDebug("User {UserId} has unlimited doctor access", userId);
				return true;
			}

			// For limited access, implement your business logic
			// This could involve tracking which doctors the user has accessed
			// For now, returning true as a simplified implementation
			_logger.LogDebug("User {UserId} has limited doctor access (max: {Max})",
				userId, subscription.SubscriptionPlan.MaxDoctorsAccess);
			return true;
		}

		public async Task<decimal> GetDiscountPercentageAsync(string userId)
		{
			_logger.LogDebug("Getting discount percentage for user {UserId}", userId);

			var subscription = await _unitOfWork.UserSubscriptions.GetAsync(
				s => s.UserId == userId && s.Status == "Active",
				includeProperties: "SubscriptionPlan");

			if (subscription == null || subscription.EndDate < DateTime.UtcNow)
			{
				_logger.LogDebug("User {UserId} has no active subscription", userId);
				return 0;
			}

			var discount = subscription.SubscriptionPlan.DiscountPercentage;
			_logger.LogDebug("User {UserId} discount percentage: {Discount}%", userId, discount);
			return discount;
		}

		#endregion

		#region Admin Functions

		public async Task<IEnumerable<UserSubscriptionDTO>> GetAllSubscriptionsAsync()
		{
			_logger.LogDebug("Getting all subscriptions for admin");

			var subscriptions = await _unitOfWork.UserSubscriptions.GetAllAsync(
				includeProperties: "User,SubscriptionPlan");

			var result = _mapper.Map<IEnumerable<UserSubscriptionDTO>>(subscriptions);

			foreach (var dto in result)
			{
				var subscription = subscriptions.First(s => s.SubscriptionId == dto.SubscriptionId);
				CalculateRemainingConsultations(dto, subscription);
			}

			return result;
		}

		public async Task ProcessExpiredSubscriptionsAsync()
		{
			_logger.LogInformation("Processing expired subscriptions");

			var expiredSubscriptions = await _unitOfWork.UserSubscriptions.GetAllAsync(
				s => s.Status == "Active" && s.EndDate < DateTime.UtcNow);

			var expiredCount = 0;
			foreach (var subscription in expiredSubscriptions)
			{
				subscription.Status = "Expired";
				subscription.UpdatedAt = DateTime.UtcNow;
				await _unitOfWork.UserSubscriptions.UpdateAsync(subscription);
				expiredCount++;
			}

			// Process pending cancellations
			var pendingCancellations = await _unitOfWork.UserSubscriptions.GetAllAsync(
				s => s.Status == "PendingCancellation" && s.EndDate < DateTime.UtcNow);

			var cancelledCount = 0;
			foreach (var subscription in pendingCancellations)
			{
				subscription.Status = "Cancelled";
				subscription.UpdatedAt = DateTime.UtcNow;
				await _unitOfWork.UserSubscriptions.UpdateAsync(subscription);
				cancelledCount++;
			}

			await _unitOfWork.CompleteAsync();

			_logger.LogInformation("Processed {ExpiredCount} expired subscriptions and {CancelledCount} pending cancellations",
				expiredCount, cancelledCount);
		}

		#endregion

		#region Helper Methods

		private void CalculateRemainingConsultations(UserSubscriptionDTO dto, UserSubscription subscription)
		{
			if (subscription.SubscriptionPlan.ConsultationsPerMonth > 0)
			{
				dto.ConsultationsRemaining = Math.Max(0,
					subscription.SubscriptionPlan.ConsultationsPerMonth - subscription.ConsultationsUsed);
			}
			else
			{
				dto.ConsultationsRemaining = -1; // Unlimited
			}
		}
		#endregion

		#region Additional Utility Methods

		/// <summary>
		/// Get subscriptions expiring within specified days
		/// </summary>
		public async Task<IEnumerable<UserSubscriptionDTO>> GetSubscriptionsExpiringInDaysAsync(int days)
		{
			_logger.LogDebug("Getting subscriptions expiring within {Days} days", days);

			var expiryDate = DateTime.UtcNow.AddDays(days);
			var subscriptions = await _unitOfWork.UserSubscriptions.GetAllAsync(
				s => s.Status == "Active" && s.EndDate <= expiryDate && s.EndDate > DateTime.UtcNow,
				includeProperties: "User,SubscriptionPlan");

			var result = _mapper.Map<IEnumerable<UserSubscriptionDTO>>(subscriptions);

			foreach (var dto in result)
			{
				var subscription = subscriptions.First(s => s.SubscriptionId == dto.SubscriptionId);
				CalculateRemainingConsultations(dto, subscription);
			}

			return result;
		}

		/// <summary>
		/// Check if a user can upgrade to a specific plan
		/// </summary>
		public async Task<bool> CanUpgradeToPlanAsync(string userId, int planId)
		{
			_logger.LogDebug("Checking if user {UserId} can upgrade to plan {PlanId}", userId, planId);

			var currentSubscription = await _unitOfWork.UserSubscriptions.GetAsync(
				s => s.UserId == userId && s.Status == "Active",
				includeProperties: "SubscriptionPlan");

			if (currentSubscription == null)
			{
				_logger.LogDebug("User {UserId} has no active subscription", userId);
				return false;
			}

			var targetPlan = await _unitOfWork.SubscriptionPlans.GetAsync(p => p.PlanId == planId && p.IsActive);
			if (targetPlan == null)
			{
				_logger.LogDebug("Target plan {PlanId} not found or inactive", planId);
				return false;
			}

			// Check if it's actually an upgrade (higher price)
			var currentPrice = currentSubscription.BillingCycle == "Yearly"
				? currentSubscription.SubscriptionPlan.YearlyPrice
				: currentSubscription.SubscriptionPlan.MonthlyPrice;

			var targetPrice = currentSubscription.BillingCycle == "Yearly"
				? targetPlan.YearlyPrice
				: targetPlan.MonthlyPrice;

			var canUpgrade = targetPrice > currentPrice;
			_logger.LogDebug("User {UserId} can upgrade to plan {PlanId}: {CanUpgrade}", userId, planId, canUpgrade);

			return canUpgrade;
		}

		/// <summary>
		/// Reset consultation usage for a user (admin function)
		/// </summary>
		public async Task<bool> ResetConsultationUsageAsync(string userId)
		{
			_logger.LogInformation("Resetting consultation usage for user {UserId}", userId);

			var subscription = await _unitOfWork.UserSubscriptions.GetAsync(
				s => s.UserId == userId && s.Status == "Active");

			if (subscription == null)
			{
				_logger.LogWarning("No active subscription found for user {UserId}", userId);
				return false;
			}

			subscription.ConsultationsUsed = 0;
			subscription.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.UserSubscriptions.UpdateAsync(subscription);
			await _unitOfWork.CompleteAsync();

			_logger.LogInformation("Consultation usage reset successfully for user {UserId}", userId);
			return true;
		}

		/// <summary>
		/// Validate subscription plan configuration
		/// </summary>
		public bool ValidatePlanConfiguration(CreateSubscriptionPlanDTO planDTO)
		{
			var errors = new List<string>();

			if (planDTO.MonthlyPrice <= 0)
				errors.Add("Monthly price must be greater than 0");

			if (planDTO.YearlyPrice <= 0)
				errors.Add("Yearly price must be greater than 0");

			if (planDTO.YearlyPrice >= planDTO.MonthlyPrice * 12)
				errors.Add("Yearly price should offer some discount compared to monthly");

			if (planDTO.ConsultationsPerMonth < -1 || planDTO.ConsultationsPerMonth == 0)
				errors.Add("Consultations per month must be -1 (unlimited) or a positive number");

			if (planDTO.MaxDoctorsAccess < -1 || planDTO.MaxDoctorsAccess == 0)
				errors.Add("Max doctors access must be -1 (unlimited) or a positive number");

			if (planDTO.DiscountPercentage < 0 || planDTO.DiscountPercentage > 100)
				errors.Add("Discount percentage must be between 0 and 100");

			if (errors.Any())
			{
				_logger.LogWarning("Plan validation failed: {Errors}", string.Join(", ", errors));
				throw new ArgumentException($"Plan validation failed: {string.Join(", ", errors)}");
			}

			return true;
		}

		#endregion
	}
}