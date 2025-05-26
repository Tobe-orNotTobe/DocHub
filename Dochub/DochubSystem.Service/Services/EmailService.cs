using DochubSystem.Common.Helper;
using DochubSystem.Data.DTOs;
using DochubSystem.ServiceContract.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DochubSystem.Service.Services
{
	public class EmailService : IEmailService
	{
		private readonly EmailSetting _emailSetting;
		private readonly ILogger<EmailService> _logger;

		public EmailService(IOptions<EmailSetting> emailOptions, ILogger<EmailService> logger)
		{
			_logger = logger;
			_emailSetting = emailOptions.Value;

		}
		public void SendEmail(EmailRequestDTO request)
		{
			var email = new MimeMessage();
			email.From.Add(MailboxAddress.Parse(_emailSetting.Email));
			email.To.Add(MailboxAddress.Parse(request.toEmail));
			email.Subject = request.Subject;

			var builder = new BodyBuilder { HtmlBody = request.Body };
			email.Body = builder.ToMessageBody();

			try
			{
				using var smtp = new SmtpClient();
				smtp.Connect(_emailSetting.Host, _emailSetting.Port, SecureSocketOptions.StartTls);
				smtp.Authenticate(_emailSetting.Email, _emailSetting.Password);
				smtp.Send(email);
				smtp.Disconnect(true);
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Failed to send email to {Email}", request.toEmail);
				throw new Exception("Failed to send email: " + e.Message);
			}
		}

		public void SendEmailConfirmation(string email, string confirmLink)
		{
			if (string.IsNullOrEmpty(email) || !email.Contains("@"))
			{
				throw new Exception("Địa chỉ email không hợp lệ: " + email);
			}

			string body = $@"
        <div style='font-family:Arial;'>
            <h2>Xác nhận tài khoản</h2>
            <p>Click vào link sau để xác nhận tài khoản:</p>
            <a href='{confirmLink}'>confirm</a>
        </div>";

			EmailRequestDTO request = new EmailRequestDTO
			{
				Subject = "Dochub - Xác nhận Email",
				toEmail = email,
				Body = body
			};

			SendEmail(request);
		}

		public async Task SendEmailForgotPassword(string email, string resetLink)
		{
			if (string.IsNullOrEmpty(email) || !email.Contains("@"))
				throw new Exception("Địa chỉ email không hợp lệ: " + email);

			string body = $@"
        <div style='font-family:Arial;'>
            <h2>Đặt lại mật khẩu</h2>
            <p>Nhấn vào nút bên dưới để đặt lại mật khẩu:</p>
            <a href='{resetLink}' style='background:#5288DB;color:#fff;padding:10px 15px;border-radius:5px;text-decoration:none;'>Đặt lại mật khẩu</a>
            <p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>
        </div>";

			var request = new EmailRequestDTO
			{
				Subject = "Đặt lại mật khẩu",
				toEmail = email,
				Body = body
			};

			await Task.Run(() => SendEmail(request));
		}
	}
}
