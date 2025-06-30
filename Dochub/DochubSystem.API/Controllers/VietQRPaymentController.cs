// VietQRPaymentController.cs - API Controller for VietQR payments
using DochubSystem.Common.Helper;
using DochubSystem.Data.DTOs;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace DochubSystem.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(AuthenticationSchemes = "Bearer")]
	public class VietQRPaymentController : ControllerBase
	{
		private readonly IVietQRPaymentService _vietQRPaymentService;
		private readonly APIResponse _response;
		private readonly ILogger<VietQRPaymentController> _logger;

		public VietQRPaymentController(
			IVietQRPaymentService vietQRPaymentService,
			APIResponse response,
			ILogger<VietQRPaymentController> logger)
		{
			_vietQRPaymentService = vietQRPaymentService;
			_response = response;
			_logger = logger;
		}

		#region Customer APIs

		/// <summary>
		/// Create a new VietQR payment request for subscription
		/// </summary>
		[HttpPost("create-request")]
		public async Task<IActionResult> CreatePaymentRequest([FromBody] CreateVietQRPaymentRequestDTO request)
		{
			try
			{
				var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (string.IsNullOrEmpty(userId))
				{
					_response.StatusCode = HttpStatusCode.Unauthorized;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("User not authenticated");
					return Unauthorized(_response);
				}

				if (!ModelState.IsValid)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages.AddRange(ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage));
					return BadRequest(_response);
				}

				var result = await _vietQRPaymentService.CreatePaymentRequestAsync(userId, request);

				_response.StatusCode = HttpStatusCode.Created;
				_response.IsSuccess = true;
				_response.Result = result;
				return Ok(_response);
			}
			catch (ArgumentException ex)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return BadRequest(_response);
			}
			catch (InvalidOperationException ex)
			{
				_response.StatusCode = HttpStatusCode.Conflict;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return Conflict(_response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating VietQR payment request");
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("An error occurred while creating payment request");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Get payment request by ID
		/// </summary>
		[HttpGet("request/{paymentRequestId}")]
		public async Task<IActionResult> GetPaymentRequest(int paymentRequestId)
		{
			try
			{
				var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (string.IsNullOrEmpty(userId))
				{
					_response.StatusCode = HttpStatusCode.Unauthorized;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("User not authenticated");
					return Unauthorized(_response);
				}

				var result = await _vietQRPaymentService.GetPaymentRequestAsync(paymentRequestId);

				// Check if the payment request belongs to the current user
				if (result.UserId != userId)
				{
					_response.StatusCode = HttpStatusCode.Forbidden;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Access denied");
					return Forbid();
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = result;
				return Ok(_response);
			}
			catch (ArgumentException ex)
			{
				_response.StatusCode = HttpStatusCode.NotFound;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return NotFound(_response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting payment request {PaymentRequestId}", paymentRequestId);
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("An error occurred while retrieving payment request");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Get payment request by transfer code
		/// </summary>
		[HttpGet("request/by-code/{transferCode}")]
		public async Task<IActionResult> GetPaymentRequestByTransferCode(string transferCode)
		{
			try
			{
				var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (string.IsNullOrEmpty(userId))
				{
					_response.StatusCode = HttpStatusCode.Unauthorized;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("User not authenticated");
					return Unauthorized(_response);
				}

				var result = await _vietQRPaymentService.GetPaymentRequestByTransferCodeAsync(transferCode);

				// Check if the payment request belongs to the current user
				if (result.UserId != userId)
				{
					_response.StatusCode = HttpStatusCode.Forbidden;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Access denied");
					return Forbid();
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = result;
				return Ok(_response);
			}
			catch (ArgumentException ex)
			{
				_response.StatusCode = HttpStatusCode.NotFound;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return NotFound(_response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting payment request by transfer code {TransferCode}", transferCode);
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("An error occurred while retrieving payment request");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Get user's payment requests history
		/// </summary>
		[HttpGet("my-requests")]
		public async Task<IActionResult> GetMyPaymentRequests()
		{
			try
			{
				var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (string.IsNullOrEmpty(userId))
				{
					_response.StatusCode = HttpStatusCode.Unauthorized;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("User not authenticated");
					return Unauthorized(_response);
				}

				var result = await _vietQRPaymentService.GetUserPaymentRequestsAsync(userId);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = result;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting user payment requests");
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("An error occurred while retrieving payment requests");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Get user's transaction history
		/// </summary>
		[HttpGet("my-transactions")]
		public async Task<IActionResult> GetMyTransactions()
		{
			try
			{
				var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (string.IsNullOrEmpty(userId))
				{
					_response.StatusCode = HttpStatusCode.Unauthorized;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("User not authenticated");
					return Unauthorized(_response);
				}

				var result = await _vietQRPaymentService.GetUserTransactionHistoryAsync(userId);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = result;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting user transaction history");
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("An error occurred while retrieving transaction history");
				return StatusCode(500, _response);
			}
		}

		#endregion

		#region Admin APIs

		/// <summary>
		/// Search payment requests (Admin only)
		/// </summary>
		[HttpGet("admin/search")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> SearchPaymentRequests([FromQuery] PaymentRequestSearchDTO searchDto)
		{
			try
			{
				var result = await _vietQRPaymentService.SearchPaymentRequestsAsync(searchDto);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = result;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error searching payment requests");
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("An error occurred while searching payment requests");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Confirm payment request (Admin only)
		/// </summary>
		[HttpPost("admin/confirm/{paymentRequestId}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> ConfirmPayment(int paymentRequestId, [FromBody] ConfirmPaymentRequestDTO confirmDto)
		{
			try
			{
				var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (string.IsNullOrEmpty(adminId))
				{
					_response.StatusCode = HttpStatusCode.Unauthorized;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Admin not authenticated");
					return Unauthorized(_response);
				}

				if (!ModelState.IsValid)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages.AddRange(ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage));
					return BadRequest(_response);
				}

				var result = await _vietQRPaymentService.ConfirmPaymentAsync(paymentRequestId, adminId, confirmDto);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Success = result };
				return Ok(_response);
			}
			catch (ArgumentException ex)
			{
				_response.StatusCode = HttpStatusCode.NotFound;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return NotFound(_response);
			}
			catch (InvalidOperationException ex)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return BadRequest(_response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error confirming payment request {PaymentRequestId}", paymentRequestId);
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("An error occurred while confirming payment");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Get transaction record by ID (Admin only)
		/// </summary>
		[HttpGet("admin/transaction/{transactionId}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetTransactionRecord(int transactionId)
		{
			try
			{
				var result = await _vietQRPaymentService.GetTransactionRecordAsync(transactionId);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = result;
				return Ok(_response);
			}
			catch (ArgumentException ex)
			{
				_response.StatusCode = HttpStatusCode.NotFound;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return NotFound(_response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting transaction record {TransactionId}", transactionId);
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add("An error occurred while retrieving transaction record");
				return StatusCode(500, _response);
			}
		}

		#endregion
	}
}