using DochubSystem.Common.Helper;
using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using DochubSystem.RepositoryContract.Interfaces;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DochubSystem.Service.Services
{
	public class PaymentService : IPaymentService
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<PaymentService> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IUnitOfWork _unitOfWork;

		public PaymentService(
			IConfiguration configuration,
			ILogger<PaymentService> logger,
			IHttpClientFactory httpClientFactory,
			IUnitOfWork unitOfWork)
		{
			_configuration = configuration;
			_logger = logger;
			_httpClientFactory = httpClientFactory;
			_unitOfWork = unitOfWork;
		}

		/// <summary>
		/// Tạo URL thanh toán VNPay cho subscription
		/// </summary>
		public async Task<PaymentUrlResponseDTO> CreateVNPayPaymentUrlAsync(CreatePaymentRequestDTO request)
		{
			try
			{
				_logger.LogInformation("Creating VNPay payment URL for subscription {SubscriptionType}", request.SubscriptionType);

				var paymentTransaction = await CreatePaymentTransactionAsync(request);

				var tick = DateTime.Now.Ticks.ToString();
				var txnRef = $"TXN{paymentTransaction.PaymentTransactionId}_TIME{tick}";

				var vnpay = new VnPayLibrary();

				vnpay.AddRequestData("vnp_Version", _configuration["VnPay:Version"]);
				vnpay.AddRequestData("vnp_Command", _configuration["VnPay:Command"]);
				vnpay.AddRequestData("vnp_TmnCode", _configuration["VnPay:TmnCode"]);
				vnpay.AddRequestData("vnp_Amount", (Convert.ToInt64(request.Amount) * 100).ToString());
				vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
				vnpay.AddRequestData("vnp_CurrCode", _configuration["VnPay:CurrCode"]);
				vnpay.AddRequestData("vnp_IpAddr", request.IpAddress);
				vnpay.AddRequestData("vnp_Locale", _configuration["VnPay:Locale"]);
				vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan goi {request.SubscriptionType} - {request.BillingCycle}");
				vnpay.AddRequestData("vnp_OrderType", "subscription"); // Đánh dấu là subscription
				vnpay.AddRequestData("vnp_ReturnUrl", _configuration["VnPay:SubscriptionReturnUrl"]);
				vnpay.AddRequestData("vnp_TxnRef", txnRef);

				var paymentUrl = vnpay.CreateRequestUrl(_configuration["VnPay:BaseUrl"], _configuration["VnPay:HashSecret"]);

				// Cập nhật transaction với txnRef
				paymentTransaction.PaymentGatewayTransactionId = txnRef;
				await _unitOfWork.PaymentTransactions.UpdateAsync(paymentTransaction);
				await _unitOfWork.CompleteAsync();

				return new PaymentUrlResponseDTO
				{
					PaymentUrl = paymentUrl,
					TransactionRef = txnRef,
					Success = true
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating VNPay payment URL");
				return new PaymentUrlResponseDTO
				{
					Success = false,
					ErrorMessage = "Lỗi tạo liên kết thanh toán VNPay"
				};
			}
		}

		/// <summary>
		/// Xác minh thanh toán VNPay callback - tương tự PaymentExecute
		/// </summary>
		public async Task<PaymentVerificationDTO> VerifyVNPayPaymentAsync(Dictionary<string, string> vnpayParams)
		{
			try
			{
				_logger.LogInformation("Verifying VNPay payment for transaction {TxnRef}",
					vnpayParams.GetValueOrDefault("vnp_TxnRef"));

				// Xác minh chữ ký giống code cũ
				var vnpHashSecret = _configuration["VNPay:HashSecret"];
				if (string.IsNullOrEmpty(vnpHashSecret))
				{
					return new PaymentVerificationDTO
					{
						Success = false,
						ErrorMessage = "Cài đặt VNPay không được cấu hình đúng!"
					};
				}

				string vnpSecureHash = vnpayParams["vnp_SecureHash"];

				var signParams = new SortedList<string, string>();
				foreach (var param in vnpayParams)
				{
					if (!param.Key.Equals("vnp_SecureHash"))
					{
						signParams.Add(param.Key, param.Value);
					}
				}

				var signData = new StringBuilder();
				foreach (var param in signParams)
				{
					signData.Append(WebUtility.UrlEncode(param.Key) + "=" + WebUtility.UrlEncode(param.Value) + "&");
				}

				if (signData.Length > 0)
				{
					signData.Remove(signData.Length - 1, 1);
				}

				var hmacSha512 = new HMACSHA512(Encoding.UTF8.GetBytes(vnpHashSecret));
				var hash = hmacSha512.ComputeHash(Encoding.UTF8.GetBytes(signData.ToString()));
				var calculatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

				if (calculatedSignature != vnpSecureHash.ToLower())
				{
					return new PaymentVerificationDTO
					{
						Success = false,
						ErrorMessage = "Chữ ký không hợp lệ"
					};
				}

				// Extract transaction ID từ txnRef giống code cũ
				var match = Regex.Match(vnpayParams["vnp_TxnRef"], @"TXN(\d+)_TIME");
				if (!match.Success)
				{
					return new PaymentVerificationDTO
					{
						Success = false,
						ErrorMessage = "Transaction reference không hợp lệ"
					};
				}

				int paymentTransactionId = int.Parse(match.Groups[1].Value);
				var responseCode = vnpayParams.GetValueOrDefault("vnp_ResponseCode");
				var amount = decimal.Parse(vnpayParams.GetValueOrDefault("vnp_Amount", "0")) / 100;

				// Cập nhật trạng thái transaction
				if (responseCode != "00")
				{
					await UpdatePaymentTransactionStatusAsync(paymentTransactionId, "Failed", responseCode);
					return new PaymentVerificationDTO
					{
						Success = false,
						TransactionRef = vnpayParams["vnp_TxnRef"],
						Amount = amount,
						ErrorMessage = GetVNPayErrorMessage(responseCode)
					};
				}

				await UpdatePaymentTransactionStatusAsync(paymentTransactionId, "Completed", responseCode);

				return new PaymentVerificationDTO
				{
					Success = true,
					TransactionRef = vnpayParams["vnp_TxnRef"],
					Amount = amount,
					PaymentMethod = "VNPay",
					PaymentGatewayTransactionId = vnpayParams.GetValueOrDefault("vnp_TransactionNo"),
					PaymentDate = DateTime.UtcNow
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error verifying VNPay payment");
				return new PaymentVerificationDTO
				{
					Success = false,
					ErrorMessage = "Lỗi xác minh thanh toán"
				};
			}
		}

		/// <summary>
		/// Tạo PaymentTransaction record
		/// </summary>
		private async Task<PaymentTransaction> CreatePaymentTransactionAsync(CreatePaymentRequestDTO request)
		{
			var paymentTransaction = new PaymentTransaction
			{
				TransactionRef = Guid.NewGuid().ToString(), // Temporary, sẽ update với VNPay txnRef
				UserId = request.UserId,
				Amount = request.Amount,
				PaymentMethod = request.PaymentMethod,
				Status = "Pending",
				TransactionType = "Subscription",
				OrderInfo = $"Thanh toan goi {request.SubscriptionType} - {request.BillingCycle}",
				BillingCycle = request.BillingCycle,
				Currency = "VND",
				IpAddress = request.IpAddress,
				CreatedAt = DateTime.UtcNow
			};

			await _unitOfWork.PaymentTransactions.AddAsync(paymentTransaction);
			await _unitOfWork.CompleteAsync();

			return paymentTransaction;
		}

		/// <summary>
		/// Cập nhật trạng thái PaymentTransaction
		/// </summary>
		private async Task UpdatePaymentTransactionStatusAsync(int paymentTransactionId, string status, string responseCode)
		{
			var transaction = await _unitOfWork.PaymentTransactions.GetAsync(t => t.PaymentTransactionId == paymentTransactionId);
			if (transaction != null)
			{
				transaction.Status = status;
				transaction.ProcessedAt = DateTime.UtcNow;
				transaction.PaymentGatewayResponse = responseCode;

				await _unitOfWork.PaymentTransactions.UpdateAsync(transaction);
				await _unitOfWork.CompleteAsync();

				_logger.LogInformation("Updated payment transaction {TransactionId} status to {Status}",
					paymentTransactionId, status);
			}
		}

		/// <summary>
		/// Lấy PaymentTransaction bằng VNPay TxnRef
		/// </summary>
		public async Task<PaymentTransaction> GetPaymentTransactionByVNPayRefAsync(string vnpayTxnRef)
		{
			var match = Regex.Match(vnpayTxnRef, @"TXN(\d+)_TIME");
			if (!match.Success)
			{
				return null;
			}

			int paymentTransactionId = int.Parse(match.Groups[1].Value);
			return await _unitOfWork.PaymentTransactions.GetAsync(t => t.PaymentTransactionId == paymentTransactionId);
		}

		#region MoMo Implementation (giữ nguyên)

		public async Task<PaymentUrlResponseDTO> CreateMoMoPaymentUrlAsync(CreatePaymentRequestDTO request)
		{
			// Implementation cho MoMo - giữ nguyên như trước
			throw new NotImplementedException("MoMo implementation - implement theo yêu cầu");
		}

		public async Task<PaymentVerificationDTO> VerifyMoMoPaymentAsync(MoMoCallbackDTO callback)
		{
			// Implementation cho MoMo
			throw new NotImplementedException("MoMo verification - implement theo yêu cầu");
		}

		#endregion

		#region Bank Transfer Implementation

		public async Task<BankTransferInfoDTO> CreateBankTransferAsync(CreatePaymentRequestDTO request)
		{
			var paymentTransaction = await CreatePaymentTransactionAsync(request);

			return new BankTransferInfoDTO
			{
				BankName = _configuration["Banking:BankName"],
				AccountNumber = _configuration["Banking:AccountNumber"],
				AccountName = _configuration["Banking:AccountName"],
				TransferContent = $"DOCHUB SUB{paymentTransaction.PaymentTransactionId}",
				Amount = request.Amount,
				QRCodeUrl = $"{_configuration["AppSettings:BaseUrl"]}/qr/bank-transfer/{paymentTransaction.PaymentTransactionId}"
			};
		}

		public async Task<PaymentVerificationDTO> VerifyBankTransferAsync(string transactionRef, decimal amount)
		{
			// Implementation cho bank transfer verification
			throw new NotImplementedException("Bank transfer verification - implement theo yêu cầu");
		}

		#endregion

		#region Helper Methods

		private string GetVNPayErrorMessage(string responseCode)
		{
			return responseCode switch
			{
				"07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
				"09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
				"10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
				"11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
				"12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
				"13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
				"24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
				"51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
				"65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
				"75" => "Ngân hàng thanh toán đang bảo trì.",
				"79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định.",
				_ => "Giao dịch không thành công"
			};
		}

		#endregion

		#region Not Implemented Methods

		public async Task<List<PaymentMethodDTO>> GetAvailablePaymentMethodsAsync()
		{
			return new List<PaymentMethodDTO>
			{
				new PaymentMethodDTO
				{
					Code = "VNPay",
					Name = "VNPay",
					Description = "Thanh toán qua cổng VNPay",
					IconUrl = "/images/vnpay-icon.png",
					IsActive = true,
					SupportedCurrencies = new List<string> { "VND" }
				},
				new PaymentMethodDTO
				{
					Code = "Banking",
					Name = "Chuyển khoản ngân hàng",
					Description = "Chuyển khoản trực tiếp qua ngân hàng",
					IconUrl = "/images/bank-icon.png",
					IsActive = true,
					SupportedCurrencies = new List<string> { "VND" }
				}
			};
		}

		public async Task<PaymentStatusDTO> GetPaymentStatusAsync(string transactionRef)
		{
			throw new NotImplementedException();
		}

		public async Task<List<PaymentHistoryDTO>> GetUserPaymentHistoryAsync(string userId)
		{
			throw new NotImplementedException();
		}

		public async Task<bool> ProcessRefundAsync(string transactionRef, decimal amount, string reason)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
