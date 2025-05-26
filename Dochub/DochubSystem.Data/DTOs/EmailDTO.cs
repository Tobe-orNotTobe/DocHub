namespace DochubSystem.Data.DTOs
{
	public class EmailRequestDTO
	{
		public string toEmail { get; set; } = string.Empty;
		public string Subject { get; set; } = string.Empty;
		public string Body { get; set; } = string.Empty;
	}
	public class ConfirmEmailRequest
	{
		public string Email { get; set; }
		public string Token { get; set; }
	}
}
