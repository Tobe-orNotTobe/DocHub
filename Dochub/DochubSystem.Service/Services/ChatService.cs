using AutoMapper;
using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using DochubSystem.RepositoryContract.Interfaces;
using DochubSystem.ServiceContract.Interfaces;

namespace DochubSystem.Service.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChatService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ChatDTO> SaveMessageAsync(CreateChatWithUserDTO dto)
        {
            var chat = new Chat
            {
                AppointmentId = dto.AppointmentId,
                UserId = dto.UserId,
                Message = dto.Message,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.Chats.AddAsync(chat);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ChatDTO>(chat);
        }

        public async Task<IEnumerable<ChatDTO>> GetMessagesByAppointmentAsync(int appointmentId)
        {
            var messages = await _unitOfWork.Chats.GetByAppointmentIdAsync(appointmentId);
            return _mapper.Map<IEnumerable<ChatDTO>>(messages);
        }
    }
}
