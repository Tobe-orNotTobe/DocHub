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
    public class ChatController : ControllerBase
    {
        private readonly APIResponse _response;
        private readonly IChatService _chatService;

        public ChatController(APIResponse response, IChatService chatService)
        {
            _response = response;
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] CreateChatDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // 👈 lấy từ token

                var fullDto = new CreateChatWithUserDTO
                {
                    AppointmentId = dto.AppointmentId,
                    Message = dto.Message,
                    UserId = userId
                };

                var result = await _chatService.SaveMessageAsync(fullDto);

                _response.Result = result;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.Created;

                return StatusCode(201, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add($"Lỗi hệ thống: {ex.Message}");
                return StatusCode(500, _response);
            }
        }

        [HttpGet("appointment/{appointmentId}")]
        public async Task<IActionResult> GetMessages(int appointmentId)
        {
            try
            {
                var messages = await _chatService.GetMessagesByAppointmentAsync(appointmentId);
                _response.Result = messages;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add($"Lỗi hệ thống: {ex.Message}");
                return StatusCode(500, _response);
            }
        }
    }
}
