namespace DochubSystem.Data.DTOs
{
    public class CreateChatWithUserDTO
    {
        public int AppointmentId { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; } // được gán từ JWT
    }
}
