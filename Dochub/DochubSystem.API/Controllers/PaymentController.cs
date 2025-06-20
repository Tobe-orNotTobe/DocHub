using DochubSystem.Common.Helper;
using DochubSystem.Data.DTOs;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace DochubSystem.API.Controllers
{
	[Route("api/subscription/[controller]")]
	[ApiController]
	[Authorize(AuthenticationSchemes = "Bearer")]
	public class PaymentController : ControllerBase
	{
		private readonly ISubscriptionService _subscriptionService;
		private readonly IPaymentService _paymentService;
		private readonly APIResponse _response;
		private readonly ILogger<PaymentController> _logger;
		private readonly IConfiguration _configuration;
		private readonly IMemoryCache _cache;

		public PaymentController(
			ISubscriptionService subscriptionService,
			IPaymentService paymentService,
			APIResponse response,
			ILogger<PaymentController> logger, IConfiguration configuration, 
			IMemoryCache cache)
		{
			_subscriptionService = subscriptionService;
			_paymentService = paymentService;
			_response = response;
			_logger = logger;
			_configuration = configuration; 
			_cache = cache;
		}

		/// <summary>
		/// Bước 1: Tạo liên kết thanh toán cho gói thành viên
		/// </summary>
		[HttpPost("create-payment-url")]
		public async Task<IActionResult> CreatePaymentUrl([FromBody] CreateSubscriptionPaymentDTO request)
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				
				// Kiểm tra user đã có subscription chưa
				var existingSubscription = await _subscriptionService.GetUserSubscriptionAsync(currentUserId);
				if (existingSubscription != null)
				{
					_response.StatusCode = HttpStatusCode.Conflict;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Bạn đã có gói thành viên đang hoạt động");
					return Conflict(_response);
				}

				// Lấy thông tin gói subscription
				var plan = await _subscriptionService.GetPlanByIdAsync(request.PlanId);
				if (plan == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Gói thành viên không tồn tại");
					return NotFound(_response);
				}

				// Tính toán số tiền
				var amount = request.BillingCycle == "Yearly" ? plan.YearlyPrice : plan.MonthlyPrice;
				var transactionRef = $"SUB_{currentUserId}_{DateTime.UtcNow:yyyyMMddHHmmss}";

				// Tạo payment request
				var paymentRequest = new CreatePaymentRequestDTO
				{
					Amount = amount,
					TransactionRef = transactionRef,
					SubscriptionType = plan.Name,
					BillingCycle = request.BillingCycle,
					UserId = currentUserId,
					PlanId = request.PlanId,
					IpAddress = GetClientIpAddress(),
					PaymentMethod = request.PaymentMethod
				};

				// Tạo URL thanh toán theo phương thức được chọn
				PaymentUrlResponseDTO paymentResponse = request.PaymentMethod.ToLower() switch
				{
					"vnpay" => await _paymentService.CreateVNPayPaymentUrlAsync(paymentRequest),
					"momo" => await _paymentService.CreateMoMoPaymentUrlAsync(paymentRequest),
					_ => new PaymentUrlResponseDTO { Success = false, ErrorMessage = "Phương thức thanh toán không được hỗ trợ" }
				};

				if (!paymentResponse.Success)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add(paymentResponse.ErrorMessage);
					return BadRequest(_response);
				}

				// Lưu thông tin pending payment vào cache hoặc database
				await SavePendingPaymentAsync(transactionRef, paymentRequest);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new
				{
					PaymentUrl = paymentResponse.PaymentUrl,
					TransactionRef = transactionRef,
					Amount = amount,
					PlanName = plan.Name,
					BillingCycle = request.BillingCycle
				};

				return Ok(_response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating payment URL for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("Lỗi hệ thống khi tạo liên kết thanh toán");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Bước 2: Xử lý callback từ VNPay
		/// </summary>
		[HttpGet("vnpay-callback")]
		[AllowAnonymous]
		public async Task<IActionResult> VNPayCallback([FromQuery] Dictionary<string, string> vnpayData)
		{
			try
			{
				_logger.LogInformation("Received VNPay callback for transaction {TxnRef}",
					vnpayData.GetValueOrDefault("vnp_TxnRef"));

				// Xác minh thanh toán
				var verification = await _paymentService.VerifyVNPayPaymentAsync(vnpayData);

				if (verification.Success)
				{
					// Lấy thông tin pending payment
					var pendingPayment = await GetPendingPaymentAsync(verification.TransactionRef);
					if (pendingPayment != null)
					{
						// Tạo subscription
						var createSubscriptionDTO = new CreateSubscriptionDTO
						{
							UserId = pendingPayment.UserId,
							PlanId = pendingPayment.PlanId,
							BillingCycle = pendingPayment.BillingCycle,
							PaymentMethod = "VNPay",
							PaymentGatewayTransactionId = verification.PaymentGatewayTransactionId
						};

						var subscription = await _subscriptionService.CreateSubscriptionAsync(createSubscriptionDTO);

						// Xóa pending payment
						await RemovePendingPaymentAsync(verification.TransactionRef);

						// Redirect về trang thành công
						return Redirect($"{_configuration["AppSettings:FrontendUrl"]}/subscription/success?subscriptionId={subscription.SubscriptionId}");
					}
				}

				// Redirect về trang thất bại
				return Redirect($"{_configuration["AppSettings:FrontendUrl"]}/subscription/failed?error={verification.ErrorMessage}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing VNPay callback");
				return Redirect($"{_configuration["AppSettings:FrontendUrl"]}/subscription/failed?error=system_error");
			}
		}

		/// <summary>
		/// Bước 2: Xử lý callback từ MoMo
		/// </summary>
		[HttpPost("momo-callback")]
		[AllowAnonymous]
		public async Task<IActionResult> MoMoCallback([FromBody] MoMoCallbackDTO callback)
		{
			try
			{
				_logger.LogInformation("Received MoMo callback for order {OrderId}", callback.orderId);

				if (callback.resultCode == 0) // Thành công
				{
					var pendingPayment = await GetPendingPaymentAsync(callback.orderId);
					if (pendingPayment != null)
					{
						var createSubscriptionDTO = new CreateSubscriptionDTO
						{
							UserId = pendingPayment.UserId,
							PlanId = pendingPayment.PlanId,
							BillingCycle = pendingPayment.BillingCycle,
							PaymentMethod = "MoMo",
							PaymentGatewayTransactionId = callback.transId
						};

						var subscription = await _subscriptionService.CreateSubscriptionAsync(createSubscriptionDTO);
						await RemovePendingPaymentAsync(callback.orderId);

						return Ok(new { resultCode = 0, message = "Success" });
					}
				}

				return Ok(new { resultCode = callback.resultCode, message = "Failed" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing MoMo callback");
				return Ok(new { resultCode = -1, message = "System error" });
			}
		}

		/// <summary>
		/// Kiểm tra trạng thái thanh toán
		/// </summary>
		[HttpGet("payment-status/{transactionRef}")]
		public async Task<IActionResult> GetPaymentStatus(string transactionRef)
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

				// Kiểm tra subscription đã được tạo chưa
				var subscription = await _subscriptionService.GetUserSubscriptionAsync(currentUserId);
				if (subscription != null)
				{
					_response.StatusCode = HttpStatusCode.OK;
					_response.IsSuccess = true;
					_response.Result = new
					{
						Status = "Completed",
						Subscription = subscription
					};
					return Ok(_response);
				}

				// Kiểm tra pending payment
				var pendingPayment = await GetPendingPaymentAsync(transactionRef);
				if (pendingPayment != null)
				{
					_response.StatusCode = HttpStatusCode.OK;
					_response.IsSuccess = true;
					_response.Result = new
					{
						Status = "Pending",
						Message = "Đang chờ xác nhận thanh toán"
					};
					return Ok(_response);
				}

				_response.StatusCode = HttpStatusCode.NotFound;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("Không tìm thấy giao dịch");
				return NotFound(_response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting payment status");
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("Lỗi hệ thống");
				return StatusCode(500, _response);
			}
		}

		#region Helper Methods Implementation

		private string GetClientIpAddress()
		{
			var ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
			if (string.IsNullOrEmpty(ipAddress))
			{
				ipAddress = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
			}
			if (string.IsNullOrEmpty(ipAddress))
			{
				ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
			}
			return ipAddress ?? "127.0.0.1";
		}

		private async Task SavePendingPaymentAsync(string transactionRef, CreatePaymentRequestDTO payment)
		{
			try
			{
				var pendingPayment = new
				{
					UserId = payment.UserId,
					PlanId = payment.PlanId,
					BillingCycle = payment.BillingCycle,
					PaymentMethod = payment.PaymentMethod,
					Amount = payment.Amount,
					SubscriptionType = payment.SubscriptionType,
					CreatedAt = DateTime.UtcNow,
					IpAddress = payment.IpAddress
				};

				var json = JsonSerializer.Serialize(pendingPayment);
				var cacheKey = $"pending_payment_{transactionRef}";

				// Store for 30 minutes
				_cache.Set(cacheKey, json, TimeSpan.FromMinutes(30));

				_logger.LogInformation("Saved pending payment for transaction {TransactionRef}", transactionRef);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error saving pending payment for transaction {TransactionRef}", transactionRef);
			}
		}

		private async Task<CreatePaymentRequestDTO> GetPendingPaymentAsync(string transactionRef)
		{
			try
			{
				var cacheKey = $"pending_payment_{transactionRef}";

				if (_cache.TryGetValue(cacheKey, out string json))
				{
					var pendingPayment = JsonSerializer.Deserialize<CreatePaymentRequestDTO>(json);
					_logger.LogInformation("Retrieved pending payment for transaction {TransactionRef}", transactionRef);
					return pendingPayment;
				}

				_logger.LogWarning("No pending payment found for transaction {TransactionRef}", transactionRef);
				return null;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving pending payment for transaction {TransactionRef}", transactionRef);
				return null;
			}
		}

		private async Task RemovePendingPaymentAsync(string transactionRef)
		{
			try
			{
				var cacheKey = $"pending_payment_{transactionRef}";
				_cache.Remove(cacheKey);

				_logger.LogInformation("Removed pending payment for transaction {TransactionRef}", transactionRef);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error removing pending payment for transaction {TransactionRef}", transactionRef);
			}
		}
		#endregion
	}
}

