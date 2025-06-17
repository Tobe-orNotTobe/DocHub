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
	public class SubscriptionController : ControllerBase
	{
		private readonly ISubscriptionService _subscriptionService;
		private readonly APIResponse _response;

		public SubscriptionController(ISubscriptionService subscriptionService, APIResponse response)
		{
			_subscriptionService = subscriptionService;
			_response = response;
		}

		/// <summary>
		/// Get all available subscription plans
		/// </summary>
		[HttpGet("plans")]
		[AllowAnonymous]
		public async Task<IActionResult> GetAllPlans()
		{
			try
			{
				var plans = await _subscriptionService.GetAllPlansAsync();
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = plans;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Error: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Get subscription plan by ID
		/// </summary>
		[HttpGet("plans/{planId}")]
		[AllowAnonymous]
		public async Task<IActionResult> GetPlanById(int planId)
		{
			try
			{
				var plan = await _subscriptionService.GetPlanByIdAsync(planId);
				if (plan == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Plan not found");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = plan;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Error: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Create a new subscription for the current user
		/// </summary>
		[HttpPost("subscribe")]
		public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionDTO createSubscriptionDTO)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages = ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage)
						.ToList();
					return BadRequest(_response);
				}

				// Get current user ID from JWT token
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				createSubscriptionDTO.UserId = currentUserId;

				var subscription = await _subscriptionService.CreateSubscriptionAsync(createSubscriptionDTO);

				_response.StatusCode = HttpStatusCode.Created;
				_response.IsSuccess = true;
				_response.Result = subscription;
				return CreatedAtAction(nameof(GetUserSubscription), null, _response);
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
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Error: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Get current user's subscription
		/// </summary>
		[HttpGet("my-subscription")]
		public async Task<IActionResult> GetUserSubscription()
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var subscription = await _subscriptionService.GetUserSubscriptionAsync(currentUserId);

				if (subscription == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("No active subscription found");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = subscription;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Error: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Change subscription plan
		/// </summary>
		[HttpPost("change-plan")]
		public async Task<IActionResult> ChangePlan([FromBody] ChangePlanDTO changePlanDTO)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					_response.StatusCode = HttpStatusCode.BadRequest;
					_response.IsSuccess = false;
					_response.ErrorMessages = ModelState.Values
						.SelectMany(v => v.Errors)
						.Select(e => e.ErrorMessage)
						.ToList();
					return BadRequest(_response);
				}

				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var subscription = await _subscriptionService.ChangePlanAsync(currentUserId, changePlanDTO);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = subscription;
				return Ok(_response);
			}
			catch (ArgumentException ex)
			{
				_response.StatusCode = HttpStatusCode.BadRequest;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add(ex.Message);
				return BadRequest(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Error: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Get subscription usage statistics
		/// </summary>
		[HttpGet("usage")]
		public async Task<IActionResult> GetUsage()
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var usage = await _subscriptionService.GetUserUsageAsync(currentUserId);

				if (usage == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("No subscription found");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = usage;
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Error: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Check if user can make a consultation
		/// </summary>
		[HttpGet("can-consult")]
		public async Task<IActionResult> CanConsult()
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var canConsult = await _subscriptionService.CanUserConsultAsync(currentUserId);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { CanConsult = canConsult };
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Error: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Check if user has a specific privilege
		/// </summary>
		[HttpGet("privilege/{privilege}")]
		public async Task<IActionResult> HasPrivilege(string privilege)
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var hasPrivilege = await _subscriptionService.HasPrivilegeAsync(currentUserId, privilege);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { HasPrivilege = hasPrivilege };
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Error: {ex.Message}");
				return StatusCode(500, _response);
			}
		}

		/// <summary>
		/// Get user's discount percentage
		/// </summary>
		[HttpGet("discount")]
		public async Task<IActionResult> GetDiscount()
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var discount = await _subscriptionService.GetDiscountPercentageAsync(currentUserId);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { DiscountPercentage = discount };
				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.StatusCode = HttpStatusCode.InternalServerError;
				_response.IsSuccess = false;
				_response.ErrorMessages.Add($"Error: {ex.Message}");
				return StatusCode(500, _response);
			}
		}
	}
}