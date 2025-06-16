using DochubSystem.Data.DTOs;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface IEmailService
	{
		public void SendEmail(EmailRequestDTO request);
		public void SendEmailConfirmation(string email, string confirmLink);
		Task SendEmailForgotPassword(string email, string resetLink);
		void SendWelcomeEmail(string email, string fullName, string username, string password);
	}
}
