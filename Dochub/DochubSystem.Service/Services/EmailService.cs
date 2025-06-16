using DochubSystem.Common.Helper;
using DochubSystem.Data.DTOs;
using DochubSystem.ServiceContract.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DochubSystem.Service.Services
{
	public class EmailService : IEmailService
	{
		private readonly EmailSetting _emailSetting;
		private readonly IConfiguration _configuration;
		private readonly ILogger<EmailService> _logger;

		public EmailService(IOptions<EmailSetting> emailOptions, ILogger<EmailService> logger, IConfiguration configuration)
		{
			_logger = logger;
			_emailSetting = emailOptions.Value;
			_configuration = configuration;

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

		public void SendWelcomeEmail(string email, string fullName, string username, string password)
		{
			if (string.IsNullOrEmpty(email) || !email.Contains("@"))
			{
				throw new Exception("Địa chỉ email không hợp lệ: " + email);
			}

			var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:7057";
			var loginUrl = $"{frontendUrl}/login";

			string body = $@"
			<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f5f5f5;'>
				<div style='background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
					<div style='text-align: center; margin-bottom: 30px;'>
						<h1 style='color: #5288DB; margin: 0; font-size: 28px;'>Chào mừng đến với Dochub!</h1>
						<p style='color: #666; margin: 10px 0 0 0;'>Nền tảng tư vấn y tế trực tuyến</p>
					</div>
					
					<div style='margin-bottom: 25px;'>
						<p style='color: #333; font-size: 16px; line-height: 1.6;'>
							Xin chào <strong>{fullName}</strong>,
						</p>
						<p style='color: #333; font-size: 16px; line-height: 1.6;'>
							Chúc mừng! Tài khoản của bạn đã được tạo thành công trên hệ thống Dochub. 
							Dưới đây là thông tin đăng nhập của bạn:
						</p>
					</div>

					<div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 25px 0; border-left: 4px solid #5288DB;'>
						<h3 style='color: #5288DB; margin-top: 0; margin-bottom: 15px;'>Thông tin đăng nhập:</h3>
						<p style='margin: 8px 0; color: #333;'><strong>Tên đăng nhập:</strong> {username}</p>
						<p style='margin: 8px 0; color: #333;'><strong>Mật khẩu:</strong> {password}</p>
					</div>

					<div style='background-color: #fff3cd; padding: 15px; border-radius: 8px; margin: 25px 0; border: 1px solid #ffeaa7;'>
						<p style='margin: 0; color: #856404; font-weight: 500;'>
							<strong>📝 Lưu ý quan trọng:</strong> Sau khi đăng nhập lần đầu, vui lòng thay đổi mật khẩu và cập nhật thông tin bác sĩ của mình để bảo mật tài khoản và hoàn thiện hồ sơ.
						</p>
					</div>

					<div style='text-align: center; margin: 30px 0;'>
						<a href='{loginUrl}' style='background-color: #5288DB; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: 500; display: inline-block;'>
							Đăng nhập ngay
						</a>
					</div>

					<div style='border-top: 1px solid #eee; padding-top: 20px; margin-top: 30px;'>
						<p style='color: #666; font-size: 14px; text-align: center; margin: 0;'>
							Cảm ơn bạn đã tham gia cộng đồng Dochub!<br>
							Nếu bạn có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi.
						</p>
					</div>
				</div>
			</div>";

			EmailRequestDTO request = new EmailRequestDTO
			{
				Subject = "Chào mừng đến với Dochub - Thông tin tài khoản của bạn",
				toEmail = email,
				Body = body
			};

			SendEmail(request);
		}
	}
}
