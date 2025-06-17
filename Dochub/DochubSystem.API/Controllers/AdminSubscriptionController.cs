using DochubSystem.Common.Helper;
using DochubSystem.Data.DTOs;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DochubSystem.API.Controllers
{
	[Route("api/admin/[controller]")]
	[ApiController]
	[Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin")]
	public class AdminSubscriptionController : ControllerBase
	{
		private readonly ISubscriptionService _subscriptionService;
		private readonly APIResponse _response;

		public AdminSubscriptionController(ISubscriptionService subscriptionService, APIResponse response)
		{
			_subscriptionService = subscriptionService;
			_response = response;
		}

		/// <summary>
		/// Create a new subscription plan
		/// </summary>
		[HttpPost("plans")]
		public async Task<IActionResult> CreatePlan([FromBody] CreateSubscriptionPlanDTO createPlanDTO)
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

				var plan = await _subscriptionService.CreatePlanAsync(createPlanDTO);

				_response.StatusCode = HttpStatusCode.Created;
				_response.IsSuccess = true;
				_response.Result = plan;
				return CreatedAtAction(nameof(GetPlanById), new { planId = plan.PlanId }, _response);
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
		/// Update an existing subscription plan
		/// </summary>
		[HttpPut("plans/{planId}")]
		public async Task<IActionResult> UpdatePlan(int planId, [FromBody] CreateSubscriptionPlanDTO updatePlanDTO)
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

				var plan = await _subscriptionService.UpdatePlanAsync(planId, updatePlanDTO);

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = plan;
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
		/// Delete (deactivate) a subscription plan
		/// </summary>
		[HttpDelete("plans/{planId}")]
		public async Task<IActionResult> DeletePlan(int planId)
		{
			try
			{
				var result = await _subscriptionService.DeletePlanAsync(planId);
				if (!result)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
					_response.IsSuccess = false;
					_response.ErrorMessages.Add("Plan not found");
					return NotFound(_response);
				}

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = new { Message = "Plan deactivated successfully" };
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
		/// Get all subscriptions (Admin only)
		/// </summary>
		[HttpGet("subscriptions")]
		public async Task<IActionResult> GetAllSubscriptions()
		{
			try
			{
				var subscriptions = await _subscriptionService.GetAllSubscriptionsAsync();

				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				_response.Result = subscriptions;
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