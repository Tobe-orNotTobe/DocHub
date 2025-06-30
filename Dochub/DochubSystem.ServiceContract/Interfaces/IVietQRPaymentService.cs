using DochubSystem.Data.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface IVietQRPaymentService
	{
		// Customer Flow
		Task<VietQRPaymentResponseDTO> CreatePaymentRequestAsync(string userId, CreateVietQRPaymentRequestDTO request);
		Task<PaymentRequestDTO> GetPaymentRequestAsync(int paymentRequestId);
		Task<PaymentRequestDTO> GetPaymentRequestByTransferCodeAsync(string transferCode);
		Task<IEnumerable<PaymentRequestDTO>> GetUserPaymentRequestsAsync(string userId);

		// Admin Flow
		Task<IEnumerable<PaymentRequestDTO>> SearchPaymentRequestsAsync(PaymentRequestSearchDTO searchDto);
		Task<bool> ConfirmPaymentAsync(int paymentRequestId, string adminId, ConfirmPaymentRequestDTO confirmDto);
		Task<TransactionRecordDTO> GetTransactionRecordAsync(int transactionId);
		Task<IEnumerable<TransactionRecordDTO>> GetUserTransactionHistoryAsync(string userId);

		// System Operations
		Task ProcessExpiredPaymentRequestsAsync();
		Task<string> GenerateVietQRCodeAsync(decimal amount, string transferCode, string bankCode, string accountNo, string accountName);
		Task SendPaymentNotificationToAdminsAsync(PaymentRequestDTO paymentRequest);
	}
}
