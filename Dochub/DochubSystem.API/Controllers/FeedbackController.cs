using DochubSystem.Common.Helper;
using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using DochubSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DochubSystem.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _service;
        private readonly UserManager<User> _userManager;

        public FeedbackController(IFeedbackService service, UserManager<User> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetFeedbacksByDoctorId(int doctorId)
        {
            var list = await _service.GetFeedbacksByDoctorId(doctorId);
            return Ok(new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                Result = list
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackById(int id)
        {
            var feedback = await _service.GetFeedbackById(id);
            if (feedback == null)
                return NotFound(new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "Feedback not found" }
                });

            return Ok(new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                Result = feedback
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddFeedback([FromBody] CreateFeedbackDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new APIResponse
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "UserId not found in token" }
                });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "User not found" }
                });
            }

            var userName = user.FullName ?? user.UserName ?? "Anonymous";
            var initials = string.Concat(
                userName.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(x => x[0])
            ).ToUpper();

            var result = await _service.AddFeedback(dto, userId, userName, initials);

            return StatusCode((int)HttpStatusCode.Created, new APIResponse
            {
                StatusCode = HttpStatusCode.Created,
                Result = result
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeedback(int id, [FromBody] FeedbackDTO dto)
        {
            var updated = await _service.UpdateFeedback(id, dto);
            if (updated == null)
            {
                return NotFound(new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "Feedback not found" }
                });
            }

            return Ok(new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                Result = updated
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var success = await _service.DeleteFeedback(id);
            if (!success)
            {
                return NotFound(new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { "Feedback not found" }
                });
            }

            return Ok(new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                Result = new { message = "Feedback deleted successfully" }
            });
        }
    }

}
