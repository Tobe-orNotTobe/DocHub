namespace DochubSystem.Data.Constants
{
	public static class NotificationTypes
	{
		// Notifications for Doctors
		public const string NEW_APPOINTMENT = "NEW_APPOINTMENT";
		public const string APPOINTMENT_CANCELLED = "APPOINTMENT_CANCELLED";
		public const string APPOINTMENT_UPDATED = "APPOINTMENT_UPDATED";
		public const string APPOINTMENT_REMINDER_DOCTOR = "APPOINTMENT_REMINDER_DOCTOR";
		public const string PATIENT_NO_SHOW = "PATIENT_NO_SHOW";
		public const string APPOINTMENT_CONFIRMED = "APPOINTMENT_CONFIRMED";

		// Notifications for Patients/Customers
		public const string APPOINTMENT_BOOKED = "APPOINTMENT_BOOKED";
		public const string APPOINTMENT_MODIFIED = "APPOINTMENT_MODIFIED";
		public const string APPOINTMENT_REMINDER_PATIENT = "APPOINTMENT_REMINDER_PATIENT";
		public const string APPOINTMENT_COMPLETED = "APPOINTMENT_COMPLETED";
		public const string REVIEW_REMINDER = "REVIEW_REMINDER";
		public const string MEMBERSHIP_EXPIRING = "MEMBERSHIP_EXPIRING";
		public const string MEMBERSHIP_RENEWED = "MEMBERSHIP_RENEWED";
		public const string MEMBERSHIP_EXPIRED = "MEMBERSHIP_EXPIRED";
		public const string MEMBERSHIP_ACTIVATED = "MEMBERSHIP_ACTIVATED";
		public const string MEMBERSHIP_PAYMENT_SUCCESS = "MEMBERSHIP_PAYMENT_SUCCESS";

		// System notifications
		public const string WELCOME_MESSAGE = "WELCOME_MESSAGE";
		public const string PASSWORD_RESET = "PASSWORD_RESET";
		public const string EMAIL_VERIFICATION = "EMAIL_VERIFICATION";
		public const string ACCOUNT_LOCKED = "ACCOUNT_LOCKED";
	}

	public static class NotificationPriority
	{
		public const string LOW = "low";
		public const string NORMAL = "normal";
		public const string HIGH = "high";
		public const string URGENT = "urgent";
	}

	public static class NotificationStatus
	{
		public const string PENDING = "pending";
		public const string SENT = "sent";
		public const string FAILED = "failed";
		public const string CANCELLED = "cancelled";
		public const string READ = "read";
		public const string UNREAD = "unread";
	}

	public static class TargetRoles
	{
		public const string DOCTOR = "doctor";
		public const string CUSTOMER = "customer";
		public const string ADMIN = "admin";
		public const string ALL = "all";
	}

	public static class DeliveryMethods
	{
		public const string EMAIL = "email";
		public const string IN_APP = "inapp";
		public const string BOTH = "both";
	}
}