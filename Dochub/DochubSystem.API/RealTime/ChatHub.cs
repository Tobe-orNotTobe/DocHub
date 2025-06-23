using DochubSystem.Data.DTOs;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DochubSystem.API.RealTime
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        // Lưu mapping userId ↔ connectionId để gửi tín hiệu video call đúng người
        private static readonly Dictionary<string, string> _userConnectionMap = new();

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task SendMessage(string appointmentId, string senderId, string message)
        {
            await Clients.Group(appointmentId).SendAsync("ReceiveMessage", senderId, message);

            var chatDto = new CreateChatWithUserDTO
            {
                AppointmentId = Convert.ToInt32(appointmentId),
                UserId = senderId,
                Message = message
            };

            await _chatService.SaveMessageAsync(chatDto);
        }

        public async Task SendVideoSignal(string receiverUserId, object signalData)
        {
            if (_userConnectionMap.TryGetValue(receiverUserId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveVideoSignal", Context.ConnectionId, signalData);
            }
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext?.Request.Query["userId"];
            var appointmentId = httpContext?.Request.Query["appointmentId"];

            if (!string.IsNullOrEmpty(userId))
            {
                _userConnectionMap[userId] = Context.ConnectionId;
            }

            if (!string.IsNullOrEmpty(appointmentId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, appointmentId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var appointmentId = Context.GetHttpContext()?.Request.Query["appointmentId"];

            // Gỡ khỏi group
            if (!string.IsNullOrEmpty(appointmentId))
            {
                await Groups.RemoveFromGroupAsync(connectionId, appointmentId);
            }

            // Gỡ khỏi mapping
            var userEntry = _userConnectionMap.FirstOrDefault(x => x.Value == connectionId);
            if (!string.IsNullOrEmpty(userEntry.Key))
            {
                _userConnectionMap.Remove(userEntry.Key);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
