using DochubSystem.Data.DTOs;

namespace DochubSystem.ServiceContract.Interfaces
{
	public interface INotificationTemplateService
	{
		Task<NotificationTemplateDTO> GetTemplateByIdAsync(int templateId);
		Task<NotificationTemplateDTO> GetTemplateByTypeAsync(string type);
		Task<IEnumerable<NotificationTemplateDTO>> GetAllTemplatesAsync();
		Task<IEnumerable<NotificationTemplateDTO>> GetTemplatesByRoleAsync(string targetRole);
		Task<NotificationTemplateDTO> CreateTemplateAsync(CreateNotificationTemplateDTO createTemplateDTO);
		Task<NotificationTemplateDTO> UpdateTemplateAsync(int templateId, UpdateNotificationTemplateDTO updateTemplateDTO);
		Task<bool> DeleteTemplateAsync(int templateId);
		Task<bool> ActivateTemplateAsync(int templateId);
		Task<bool> DeactivateTemplateAsync(int templateId);
		Task<bool> ExistsByTypeAsync(string type);
		Task SeedDefaultTemplatesAsync();
	}
}
