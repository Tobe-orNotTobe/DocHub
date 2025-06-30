using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;

namespace DochubSystem.RepositoryContract.Interfaces
{
	public interface IPaymentRequestRepository : IRepository<PaymentRequest>
	{
		Task<PaymentRequest> GetByTransferCodeAsync(string transferCode);
		Task<PaymentRequest> GetByIdWithDetailsAsync(int paymentRequestId);
		Task<IEnumerable<PaymentRequest>> GetPendingRequestsAsync();
		Task<IEnumerable<PaymentRequest>> GetExpiredRequestsAsync();
		Task<IEnumerable<PaymentRequest>> GetUserPaymentRequestsAsync(string userId);
		Task<IEnumerable<PaymentRequest>> SearchPaymentRequestsAsync(PaymentRequestSearchDTO searchDto);
		Task<int> GetTotalPaymentRequestsCountAsync(PaymentRequestSearchDTO searchDto);
		Task<bool> HasPendingPaymentRequestAsync(string userId, int planId);
		Task MarkAsExpiredAsync(int paymentRequestId);
		Task<IEnumerable<PaymentRequest>> GetRequestsToExpireAsync();
	}
}
