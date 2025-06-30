using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Common.Helper
{
	public class VietQRSettings
	{
		public VietQRConfig VietQR { get; set; }
		public BankAccountConfig BankAccount { get; set; }
		public AdminNotificationConfig AdminNotification { get; set; }
	}

	public class VietQRConfig
	{
		[Required]
		public string BaseUrl { get; set; }

		[Required]
		public string ClientId { get; set; }

		[Required]
		public string ApiKey { get; set; }

		public bool EnableApi { get; set; } = true; // Add flag to enable/disable API calls
	}

	public class BankAccountConfig
	{
		[Required]
		public string AccountNo { get; set; }

		[Required]
		public string AccountName { get; set; }

		[Required]
		public string BankCode { get; set; }
	}

	public class AdminNotificationConfig
	{
		[Required]
		public List<string> AdminEmails { get; set; }
	}
}
