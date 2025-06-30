using System.ComponentModel.DataAnnotations;

namespace DochubSystem.Data.DTOs
{
	public class CreateVietQRPaymentRequestDTO
	{
		[Required]
		public int PlanId { get; set; }

		[Required]
		[StringLength(20)]
		public string BillingCycle { get; set; } // Monthly, Yearly
	}

	public class VietQRPaymentResponseDTO
	{
		public int PaymentRequestId { get; set; }
		public string TransferCode { get; set; }
		public decimal Amount { get; set; }
		public string PlanName { get; set; }
		public string BillingCycle { get; set; }
		public string QRCodeUrl { get; set; }
		public DateTime ExpiresAt { get; set; }
		public int ExpiresInMinutes { get; set; }
		public BankAccountInfo BankAccount { get; set; }
	}

	public class BankAccountInfo
	{
		public string AccountNo { get; set; }
		public string AccountName { get; set; }
		public string BankCode { get; set; }
		public string BankName { get; set; }
	}

	public class ConfirmPaymentRequestDTO
	{
		[Required]
		public int PaymentRequestId { get; set; }

		[StringLength(500)]
		public string? Notes { get; set; }
	}

	public class PaymentRequestSearchDTO
	{
		public string? TransferCode { get; set; }
		public string? Status { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}

	public class PaymentRequestDTO
	{
		public int PaymentRequestId { get; set; }
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string UserEmail { get; set; }
		public int PlanId { get; set; }
		public string PlanName { get; set; }
		public string TransferCode { get; set; }
		public decimal Amount { get; set; }
		public string BillingCycle { get; set; }
		public string Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime ExpiresAt { get; set; }
		public DateTime? ConfirmedAt { get; set; }
		public string? ConfirmedByAdminName { get; set; }
		public string? Notes { get; set; }
		public bool IsExpired { get; set; }
		public BankAccountInfo BankAccount { get; set; }
	}

	public class TransactionRecordDTO
	{
		public int TransactionId { get; set; }
		public string UserId { get; set; }
		public string UserName { get; set; }
		public string UserEmail { get; set; }
		public string TransferCode { get; set; }
		public decimal Amount { get; set; }
		public string PlanName { get; set; }
		public string BillingCycle { get; set; }
		public string Status { get; set; }
		public DateTime TransactionDate { get; set; }
		public string ProcessedByAdminName { get; set; }
		public string? Notes { get; set; }
		public BankAccountInfo BankAccount { get; set; }
	}

	public class VietQRGenerateRequest
	{
		public string accountNo { get; set; }
		public string accountName { get; set; }
		public string acqId { get; set; }  // Bank code
		public int amount { get; set; }    // Integer amount in VND
		public string addInfo { get; set; } // Additional info (transfer code)
		public string format { get; set; } = "text";
		public string template { get; set; } = "compact";
	}

	public class VietQRGenerateResponse
	{
		public bool Success { get; set; }
		public string Message { get; set; }
		public VietQRData Data { get; set; }

		// Alternative fields for different API versions
		public string Code { get; set; }
		public string Desc { get; set; }
		public string Status { get; set; }
	}

	public class VietQRData
	{
		public string QrDataURL { get; set; }
		public string QrCode { get; set; }

		// Alternative field names
		public string QrDataUrl { get; set; }
		public string QRDataURL { get; set; }
		public string QRCode { get; set; }
	}
}
