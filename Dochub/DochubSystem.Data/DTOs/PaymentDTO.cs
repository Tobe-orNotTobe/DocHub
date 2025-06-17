using System.ComponentModel.DataAnnotations;

namespace DochubSystem.Data.DTOs
{
	public class CreateSubscriptionPaymentDTO
	{
		[Required]
		public int PlanId { get; set; }

		[Required]
		[RegularExpression("^(Monthly|Yearly)$", ErrorMessage = "BillingCycle must be 'Monthly' or 'Yearly'")]
		public string BillingCycle { get; set; }

		[Required]
		[RegularExpression("^(VNPay|MoMo|Banking)$", ErrorMessage = "PaymentMethod must be 'VNPay', 'MoMo', or 'Banking'")]
		public string PaymentMethod { get; set; }
	}

	public class CreatePaymentRequestDTO
	{
		public decimal Amount { get; set; }
		public string TransactionRef { get; set; }
		public string SubscriptionType { get; set; }
		public string BillingCycle { get; set; }
		public string UserId { get; set; }
		public int PlanId { get; set; }
		public string IpAddress { get; set; }
		public string PaymentMethod { get; set; }
	}

	public class PaymentUrlResponseDTO
	{
		public string PaymentUrl { get; set; }
		public string TransactionRef { get; set; }
		public bool Success { get; set; }
		public string ErrorMessage { get; set; }
	}

	public class PaymentVerificationDTO
	{
		public bool Success { get; set; }
		public string TransactionRef { get; set; }
		public decimal Amount { get; set; }
		public string PaymentMethod { get; set; }
		public string PaymentGatewayTransactionId { get; set; }
		public string ErrorMessage { get; set; }
		public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
	}

	public class MoMoResponseDTO
	{
		public string partnerCode { get; set; }
		public string requestId { get; set; }
		public string orderId { get; set; }
		public long amount { get; set; }
		public long responseTime { get; set; }
		public string message { get; set; }
		public int resultCode { get; set; }
		public string payUrl { get; set; }
		public string deeplink { get; set; }
		public string qrCodeUrl { get; set; }
	}

	public class MoMoCallbackDTO
	{
		public string partnerCode { get; set; }
		public string orderId { get; set; }
		public string requestId { get; set; }
		public long amount { get; set; }
		public string orderInfo { get; set; }
		public string orderType { get; set; }
		public string transId { get; set; }
		public int resultCode { get; set; }
		public string message { get; set; }
		public string payType { get; set; }
		public long responseTime { get; set; }
		public string extraData { get; set; }
		public string signature { get; set; }
	}

	public class PaymentStatusDTO
	{
		public string Status { get; set; } // Pending, Completed, Failed, Expired
		public string TransactionRef { get; set; }
		public decimal Amount { get; set; }
		public string PaymentMethod { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? CompletedAt { get; set; }
		public string ErrorMessage { get; set; }
		public UserSubscriptionDTO Subscription { get; set; }
	}

	public class BankTransferInfoDTO
	{
		public string BankName { get; set; }
		public string AccountNumber { get; set; }
		public string AccountName { get; set; }
		public string TransferContent { get; set; }
		public decimal Amount { get; set; }
		public string QRCodeUrl { get; set; }
	}

	public class PaymentMethodDTO
	{
		public string Code { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string IconUrl { get; set; }
		public bool IsActive { get; set; }
		public List<string> SupportedCurrencies { get; set; }
	}

	public class PaymentHistoryDTO
	{
		public int TransactionId { get; set; }
		public string TransactionRef { get; set; }
		public decimal Amount { get; set; }
		public string PaymentMethod { get; set; }
		public string Status { get; set; }
		public string TransactionType { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? ProcessedAt { get; set; }
		public string PlanName { get; set; }
		public string BillingCycle { get; set; }
		public string PaymentGatewayTransactionId { get; set; }
	}
}
