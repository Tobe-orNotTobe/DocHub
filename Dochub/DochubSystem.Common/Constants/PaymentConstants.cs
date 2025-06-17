namespace DochubSystem.Common.Constants
{
	public static class PaymentConstants
	{
		public static class Status
		{
			public const string Pending = "Pending";
			public const string Completed = "Completed";
			public const string Failed = "Failed";
			public const string Expired = "Expired";
			public const string Refunded = "Refunded";
			public const string Cancelled = "Cancelled";
		}

		public static class Methods
		{
			public const string VNPay = "VNPay";
			public const string MoMo = "MoMo";
			public const string Banking = "Banking";
			public const string Cash = "Cash";
		}

		public static class TransactionTypes
		{
			public const string Subscription = "Subscription";
			public const string Renewal = "Renewal";
			public const string Upgrade = "Upgrade";
			public const string Downgrade = "Downgrade";
			public const string Refund = "Refund";
		}

		public static class VNPayResponseCodes
		{
			public const string Success = "00";
			public const string SuspiciousTransaction = "07";
			public const string InternetBankingNotRegistered = "09";
			public const string AuthenticationFailed = "10";
			public const string PaymentExpired = "11";
			public const string AccountLocked = "12";
			public const string WrongOTP = "13";
			public const string UserCancelled = "24";
			public const string InsufficientFunds = "51";
			public const string DailyLimitExceeded = "65";
			public const string BankMaintenance = "75";
			public const string WrongPasswordLimit = "79";
		}
	}
}
