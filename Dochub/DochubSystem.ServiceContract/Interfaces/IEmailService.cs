using DochubSystem.Data.DTOs;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface IEmailService
	{
		public void SendEmail(EmailRequestDTO request);
		public void SendEmailConfirmation(string email, string confirmLink);
		Task SendEmailForgotPassword(string email, string resetLink);
	}
}
