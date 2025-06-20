using AutoMapper;
using DochubSystem.Data.Constants;
using DochubSystem.Data.DTOs;
using DochubSystem.Data.Entities;
using DochubSystem.RepositoryContract.Interfaces;
using DochubSystem.ServiceContract.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Service.Services
{
	public class NotificationTemplateService : INotificationTemplateService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly ILogger<NotificationTemplateService> _logger;

		public NotificationTemplateService(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			ILogger<NotificationTemplateService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_logger = logger;
		}

		public async Task<NotificationTemplateDTO> GetTemplateByIdAsync(int templateId)
		{
			var template = await _unitOfWork.NotificationTemplates.GetAsync(t => t.TemplateId == templateId);
			return _mapper.Map<NotificationTemplateDTO>(template);
		}

		public async Task<NotificationTemplateDTO> GetTemplateByTypeAsync(string type)
		{
			var template = await _unitOfWork.NotificationTemplates.GetByTypeAsync(type);
			return _mapper.Map<NotificationTemplateDTO>(template);
		}

		public async Task<IEnumerable<NotificationTemplateDTO>> GetAllTemplatesAsync()
		{
			var templates = await _unitOfWork.NotificationTemplates.GetAllAsync();
			return _mapper.Map<IEnumerable<NotificationTemplateDTO>>(templates);
		}

		public async Task<IEnumerable<NotificationTemplateDTO>> GetTemplatesByRoleAsync(string targetRole)
		{
			var templates = await _unitOfWork.NotificationTemplates.GetByTargetRoleAsync(targetRole);
			return _mapper.Map<IEnumerable<NotificationTemplateDTO>>(templates);
		}

		public async Task<NotificationTemplateDTO> CreateTemplateAsync(CreateNotificationTemplateDTO createTemplateDTO)
		{
			// Check if template type already exists
			var existingTemplate = await _unitOfWork.NotificationTemplates.ExistsByTypeAsync(createTemplateDTO.Type);
			if (existingTemplate)
			{
				throw new InvalidOperationException($"Template with type '{createTemplateDTO.Type}' already exists");
			}

			var template = new NotificationTemplate
			{
				Name = createTemplateDTO.Name,
				Type = createTemplateDTO.Type,
				Subject = createTemplateDTO.Subject,
				EmailBody = createTemplateDTO.EmailBody,
				NotificationBody = createTemplateDTO.NotificationBody,
				Priority = createTemplateDTO.Priority,
				TargetRole = createTemplateDTO.TargetRole,
				IsActive = createTemplateDTO.IsActive,
				RequiresEmail = createTemplateDTO.RequiresEmail,
				RequiresInApp = createTemplateDTO.RequiresInApp,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			var createdTemplate = await _unitOfWork.NotificationTemplates.AddAsync(template);
			await _unitOfWork.CompleteAsync();

			return _mapper.Map<NotificationTemplateDTO>(createdTemplate);
		}

		public async Task<NotificationTemplateDTO> UpdateTemplateAsync(int templateId, UpdateNotificationTemplateDTO updateTemplateDTO)
		{
			var template = await _unitOfWork.NotificationTemplates.GetAsync(t => t.TemplateId == templateId);
			if (template == null)
			{
				throw new ArgumentException("Template not found");
			}

			// Update only provided fields
			if (!string.IsNullOrEmpty(updateTemplateDTO.Name))
				template.Name = updateTemplateDTO.Name;

			if (!string.IsNullOrEmpty(updateTemplateDTO.Subject))
				template.Subject = updateTemplateDTO.Subject;

			if (!string.IsNullOrEmpty(updateTemplateDTO.EmailBody))
				template.EmailBody = updateTemplateDTO.EmailBody;

			if (!string.IsNullOrEmpty(updateTemplateDTO.NotificationBody))
				template.NotificationBody = updateTemplateDTO.NotificationBody;

			if (!string.IsNullOrEmpty(updateTemplateDTO.Priority))
				template.Priority = updateTemplateDTO.Priority;

			if (!string.IsNullOrEmpty(updateTemplateDTO.TargetRole))
				template.TargetRole = updateTemplateDTO.TargetRole;

			if (updateTemplateDTO.IsActive.HasValue)
				template.IsActive = updateTemplateDTO.IsActive.Value;

			if (updateTemplateDTO.RequiresEmail.HasValue)
				template.RequiresEmail = updateTemplateDTO.RequiresEmail.Value;

			if (updateTemplateDTO.RequiresInApp.HasValue)
				template.RequiresInApp = updateTemplateDTO.RequiresInApp.Value;

			template.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.NotificationTemplates.UpdateAsync(template);
			await _unitOfWork.CompleteAsync();

			return _mapper.Map<NotificationTemplateDTO>(template);
		}

		public async Task<bool> DeleteTemplateAsync(int templateId)
		{
			var template = await _unitOfWork.NotificationTemplates.GetAsync(t => t.TemplateId == templateId);
			if (template == null)
				return false;

			await _unitOfWork.NotificationTemplates.DeleteAsync(template);
			await _unitOfWork.CompleteAsync();
			return true;
		}

		public async Task<bool> ActivateTemplateAsync(int templateId)
		{
			var template = await _unitOfWork.NotificationTemplates.GetAsync(t => t.TemplateId == templateId);
			if (template == null)
				return false;

			template.IsActive = true;
			template.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.NotificationTemplates.UpdateAsync(template);
			await _unitOfWork.CompleteAsync();
			return true;
		}

		public async Task<bool> DeactivateTemplateAsync(int templateId)
		{
			var template = await _unitOfWork.NotificationTemplates.GetAsync(t => t.TemplateId == templateId);
			if (template == null)
				return false;

			template.IsActive = false;
			template.UpdatedAt = DateTime.UtcNow;

			await _unitOfWork.NotificationTemplates.UpdateAsync(template);
			await _unitOfWork.CompleteAsync();
			return true;
		}

		public async Task<bool> ExistsByTypeAsync(string type)
		{
			return await _unitOfWork.NotificationTemplates.ExistsByTypeAsync(type);
		}

		public async Task SeedDefaultTemplatesAsync()
		{
			try
			{
				var defaultTemplates = GetDefaultTemplates();

				foreach (var templateData in defaultTemplates)
				{
					var exists = await _unitOfWork.NotificationTemplates.ExistsByTypeAsync(templateData.Type);
					if (!exists)
					{
						var template = new NotificationTemplate
						{
							Name = templateData.Name,
							Type = templateData.Type,
							Subject = templateData.Subject,
							EmailBody = templateData.EmailBody,
							NotificationBody = templateData.NotificationBody,
							Priority = templateData.Priority,
							TargetRole = templateData.TargetRole,
							IsActive = true,
							RequiresEmail = templateData.RequiresEmail,
							RequiresInApp = templateData.RequiresInApp,
							CreatedAt = DateTime.UtcNow,
							UpdatedAt = DateTime.UtcNow
						};

						await _unitOfWork.NotificationTemplates.AddAsync(template);
					}
				}

				await _unitOfWork.CompleteAsync();
				_logger.LogInformation("Default notification templates seeded successfully");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error seeding default notification templates");
				throw;
			}
		}

		private List<NotificationTemplate> GetDefaultTemplates()
		{
			return new List<NotificationTemplate>
			{
                // Patient Notifications
                new NotificationTemplate
				{
					Name = "Appointment Booked",
					Type = NotificationTypes.APPOINTMENT_BOOKED,
					Subject = "Lịch hẹn đã được đặt thành công",
					NotificationBody = "Lịch hẹn của bạn với bác sĩ {DoctorName} vào lúc {AppointmentDate} đã được đặt thành công.",
					EmailBody = @"
                        <h2>Lịch hẹn đã được đặt thành công</h2>
                        <p>Xin chào {PatientName},</p>
                        <p>Lịch hẹn của bạn đã được đặt thành công với các thông tin sau:</p>
                        <ul>
                            <li><strong>Bác sĩ:</strong> {DoctorName}</li>
                            <li><strong>Thời gian:</strong> {AppointmentDate}</li>
                            <li><strong>Mã lịch hẹn:</strong> #{AppointmentId}</li>
                        </ul>
                        <p>Vui lòng có mặt đúng giờ. Cảm ơn bạn đã sử dụng dịch vụ Dochub!</p>",
					Priority = NotificationPriority.NORMAL,
					TargetRole = TargetRoles.CUSTOMER,
					RequiresEmail = true,
					RequiresInApp = true
				},

				new NotificationTemplate
				{
					Name = "Appointment Reminder Patient",
					Type = NotificationTypes.APPOINTMENT_REMINDER_PATIENT,
					Subject = "Nhắc nhở: Lịch hẹn sắp tới",
					NotificationBody = "Bạn có lịch hẹn với bác sĩ {DoctorName} vào lúc {AppointmentDate}. Vui lòng chuẩn bị và có mặt đúng giờ.",
					EmailBody = @"
                        <h2>Nhắc nhở lịch hẹn</h2>
                        <p>Xin chào {PatientName},</p>
                        <p>Đây là lời nhắc nhở về lịch hẹn sắp tới của bạn:</p>
                        <ul>
                            <li><strong>Bác sĩ:</strong> {DoctorName}</li>
                            <li><strong>Thời gian:</strong> {AppointmentDate}</li>
                            <li><strong>Mã lịch hẹn:</strong> #{AppointmentId}</li>
                        </ul>
                        <p>Vui lòng có mặt đúng giờ để đảm bảo cuộc tư vấn diễn ra suôn sẻ.</p>",
					Priority = NotificationPriority.HIGH,
					TargetRole = TargetRoles.CUSTOMER,
					RequiresEmail = true,
					RequiresInApp = true
				},

				new NotificationTemplate
				{
					Name = "Appointment Modified",
					Type = NotificationTypes.APPOINTMENT_MODIFIED,
					Subject = "Lịch hẹn đã được cập nhật",
					NotificationBody = "Lịch hẹn #{AppointmentId} của bạn đã được cập nhật. Thời gian mới: {AppointmentDate}",
					EmailBody = @"
                        <h2>Thông báo thay đổi lịch hẹn</h2>
                        <p>Xin chào {PatientName},</p>
                        <p>Lịch hẹn của bạn đã được cập nhật:</p>
                        <ul>
                            <li><strong>Mã lịch hẹn:</strong> #{AppointmentId}</li>
                            <li><strong>Bác sĩ:</strong> {DoctorName}</li>
                            <li><strong>Thời gian mới:</strong> {AppointmentDate}</li>
                            <li><strong>Loại thay đổi:</strong> {UpdateType}</li>
                        </ul>
                        <p>Vui lòng kiểm tra và xác nhận thời gian mới.</p>",
					Priority = NotificationPriority.HIGH,
					TargetRole = TargetRoles.CUSTOMER,
					RequiresEmail = true,
					RequiresInApp = true
				},

                // Doctor Notifications
                new NotificationTemplate
				{
					Name = "New Appointment",
					Type = NotificationTypes.NEW_APPOINTMENT,
					Subject = "Có lịch hẹn mới",
					NotificationBody = "Bạn có lịch hẹn mới từ bệnh nhân {PatientName} vào lúc {AppointmentDate}",
					EmailBody = @"
                        <h2>Thông báo lịch hẹn mới</h2>
                        <p>Xin chào Bác sĩ {DoctorName},</p>
                        <p>Bạn có lịch hẹn mới với thông tin sau:</p>
                        <ul>
                            <li><strong>Bệnh nhân:</strong> {PatientName}</li>
                            <li><strong>Thời gian:</strong> {AppointmentDate}</li>
                            <li><strong>Mã lịch hẹn:</strong> #{AppointmentId}</li>
                            <li><strong>Triệu chứng:</strong> {Symptoms}</li>
                        </ul>
                        <p>Vui lòng chuẩn bị cho cuộc tư vấn.</p>",
					Priority = NotificationPriority.NORMAL,
					TargetRole = TargetRoles.DOCTOR,
					RequiresEmail = true,
					RequiresInApp = true
				},

				new NotificationTemplate
				{
					Name = "Appointment Reminder Doctor",
					Type = NotificationTypes.APPOINTMENT_REMINDER_DOCTOR,
					Subject = "Nhắc nhở: Lịch hẹn sắp tới",
					NotificationBody = "Bạn có lịch hẹn với bệnh nhân {PatientName} vào lúc {AppointmentDate}",
					EmailBody = @"
                        <h2>Nhắc nhở lịch hẹn</h2>
                        <p>Xin chào Bác sĩ {DoctorName},</p>
                        <p>Đây là lời nhắc nhở về lịch hẹn sắp tới:</p>
                        <ul>
                            <li><strong>Bệnh nhân:</strong> {PatientName}</li>
                            <li><strong>Thời gian:</strong> {AppointmentDate}</li>
                            <li><strong>Mã lịch hẹn:</strong> #{AppointmentId}</li>
                        </ul>
                        <p>Vui lòng chuẩn bị cho cuộc tư vấn.</p>",
					Priority = NotificationPriority.HIGH,
					TargetRole = TargetRoles.DOCTOR,
					RequiresEmail = true,
					RequiresInApp = true
				},

                // Membership Notifications
                new NotificationTemplate
				{
					Name = "Membership Expiring",
					Type = NotificationTypes.MEMBERSHIP_EXPIRING,
					Subject = "Gói membership sắp hết hạn",
					NotificationBody = "Gói membership của bạn sẽ hết hạn vào {ExpirationDate}. Còn {DaysLeft} ngày.",
					EmailBody = @"
                        <h2>Thông báo gia hạn gói membership</h2>
                        <p>Gói membership của bạn sẽ hết hạn vào {ExpirationDate}.</p>
                        <p>Còn lại <strong>{DaysLeft} ngày</strong> để gia hạn.</p>
                        <p>Gia hạn ngay để tiếp tục sử dụng các dịch vụ cao cấp của Dochub!</p>",
					Priority = NotificationPriority.HIGH,
					TargetRole = TargetRoles.CUSTOMER,
					RequiresEmail = true,
					RequiresInApp = true
				},

				new NotificationTemplate
				{
					Name = "Membership Renewed",
					Type = NotificationTypes.MEMBERSHIP_RENEWED,
					Subject = "Gói membership đã được gia hạn",
					NotificationBody = "Gói membership của bạn đã được gia hạn thành công. Cảm ơn bạn đã tin tưởng Dochub!",
					EmailBody = @"
                        <h2>Gia hạn membership thành công</h2>
                        <p>Gói membership của bạn đã được gia hạn thành công!</p>
                        <p>Bạn có thể tiếp tục sử dụng tất cả các dịch vụ cao cấp của Dochub.</p>
                        <p>Cảm ơn bạn đã tin tưởng và đồng hành cùng chúng tôi!</p>",
					Priority = NotificationPriority.NORMAL,
					TargetRole = TargetRoles.CUSTOMER,
					RequiresEmail = true,
					RequiresInApp = true
				},

                // System Notifications
                new NotificationTemplate
				{
					Name = "Welcome Message",
					Type = NotificationTypes.WELCOME_MESSAGE,
					Subject = "Chào mừng đến với Dochub",
					NotificationBody = "Chào mừng bạn đến với Dochub! Hãy bắt đầu hành trình chăm sóc sức khỏe của bạn.",
					EmailBody = @"
                        <h2>Chào mừng đến với Dochub!</h2>
                        <p>Cảm ơn bạn đã đăng ký tài khoản tại Dochub.</p>
                        <p>Bạn có thể bắt đầu đặt lịch hẹn với các bác sĩ chuyên khoa ngay hôm nay!</p>
                        <p>Chúc bạn có trải nghiệm tuyệt vời với dịch vụ của chúng tôi.</p>",
					Priority = NotificationPriority.NORMAL,
					TargetRole = TargetRoles.ALL,
					RequiresEmail = true,
					RequiresInApp = true
				},
				new NotificationTemplate
		{
			Name = "Membership Activated",
			Type = NotificationTypes.MEMBERSHIP_ACTIVATED,
			Subject = "🎉 Chúc mừng! Gói membership đã được kích hoạt",
			NotificationBody = "Gói {PlanName} của bạn đã được kích hoạt thành công! Bạn có thể bắt đầu sử dụng các tính năng premium ngay bây giờ.",
			EmailBody = @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='text-align: center; margin-bottom: 30px;'>
                        <h1 style='color: #14b8a6; margin-bottom: 10px;'>🎉 Chúc mừng!</h1>
                        <h2 style='color: #374151; margin: 0;'>Gói membership đã được kích hoạt</h2>
                    </div>
                    
                    <div style='background: #f0fdfa; border: 1px solid #14b8a6; border-radius: 8px; padding: 20px; margin-bottom: 25px;'>
                        <h3 style='color: #14b8a6; margin-top: 0;'>Thông tin gói membership</h3>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 8px 0; color: #6b7280;'>Gói:</td>
                                <td style='padding: 8px 0; font-weight: bold; color: #374151;'>{PlanName}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px 0; color: #6b7280;'>Chu kỳ:</td>
                                <td style='padding: 8px 0; font-weight: bold; color: #374151;'>{BillingCycle}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px 0; color: #6b7280;'>Số tiền:</td>
                                <td style='padding: 8px 0; font-weight: bold; color: #14b8a6;'>{Amount} VND</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px 0; color: #6b7280;'>Ngày bắt đầu:</td>
                                <td style='padding: 8px 0; font-weight: bold; color: #374151;'>{StartDate}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px 0; color: #6b7280;'>Ngày hết hạn:</td>
                                <td style='padding: 8px 0; font-weight: bold; color: #374151;'>{EndDate}</td>
                            </tr>
                        </table>
                    </div>
                    
                    <div style='margin-bottom: 25px;'>
                        <h3 style='color: #374151;'>Những gì bạn có thể làm ngay bây giờ:</h3>
                        <ul style='color: #6b7280; line-height: 1.6;'>
                            <li>Đặt lịch hẹn với bác sĩ chuyên khoa</li>
                            <li>Tư vấn sức khỏe qua video call</li>
                            <li>Truy cập báo cáo sức khỏe chi tiết</li>
                            <li>Nhận ưu đãi giảm phí khám bệnh</li>
                            <li>Hỗ trợ 24/7 từ đội ngũ chuyên gia</li>
                        </ul>
                    </div>
                    
                    <div style='text-align: center; margin-bottom: 25px;'>
                        <a href='https://dochub.vn{ActionUrl}' style='background: #14b8a6; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>
                            Xem chi tiết membership
                        </a>
                    </div>
                    
                    <div style='background: #f9fafb; padding: 20px; border-radius: 8px; margin-bottom: 25px;'>
                        <h4 style='color: #374151; margin-top: 0;'>💡 Mẹo sử dụng:</h4>
                        <p style='color: #6b7280; margin: 0; line-height: 1.6;'>
                            Hãy bắt đầu bằng việc cập nhật hồ sơ y tế của bạn để các bác sĩ có thể tư vấn chính xác nhất. 
                            Bạn cũng có thể đặt lịch hẹn đầu tiên để khám sức khỏe tổng quát.
                        </p>
                    </div>
                    
                    <div style='text-align: center; color: #6b7280; font-size: 14px;'>
                        <p>Cảm ơn bạn đã tin tưởng và lựa chọn Dochub!</p>
                        <p>Mã subscription: {SubscriptionId}</p>
                    </div>
                </div>",
			Priority = "NORMAL",
			TargetRole = "CUSTOMER",
			RequiresEmail = true,
			RequiresInApp = true,
			IsActive = true
		},

        // Add payment success template (for immediate feedback)
        new NotificationTemplate
		{
			Name = "Membership Payment Success",
			Type = NotificationTypes.MEMBERSHIP_PAYMENT_SUCCESS,
			Subject = "✅ Thanh toán thành công",
			NotificationBody = "Thanh toán gói {PlanName} thành công! Gói membership sẽ được kích hoạt trong vòng vài phút.",
			EmailBody = @"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='text-align: center; margin-bottom: 30px;'>
                        <h1 style='color: #059669;'>✅</h1>
                        <h2 style='color: #374151; margin: 0;'>Thanh toán thành công!</h2>
                    </div>
                    
                    <div style='background: #ecfdf5; border: 1px solid #10b981; border-radius: 8px; padding: 20px; text-align: center;'>
                        <p style='color: #374151; margin: 0; font-size: 16px;'>
                            Cảm ơn bạn đã thanh toán gói <strong>{PlanName}</strong>!<br>
                            Gói membership sẽ được kích hoạt trong vòng vài phút.
                        </p>
                    </div>
                    
                    <div style='margin: 25px 0; text-align: center; color: #6b7280;'>
                        <p>Bạn sẽ nhận được email xác nhận khi gói membership được kích hoạt.</p>
                    </div>
                </div>",
			Priority = "HIGH",
			TargetRole = "CUSTOMER",
			RequiresEmail = true,
			RequiresInApp = true,
			IsActive = true
		}

			};
		}
	}
}
