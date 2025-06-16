using DochubSystem.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DochubSystem.Data.Models
{
	public class DochubDbContext : IdentityDbContext<User, IdentityRole, string>
	{
		public DochubDbContext(DbContextOptions<DochubDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<Session> Sessions { get; set; }
		public DbSet<Appointment> Appointments { get; set; }
		public DbSet<Notification> Notifications { get; set; }
		public DbSet<MedicalRecord> MedicalRecords { get; set; }
		public DbSet<Doctor> Doctors { get; set; }
		public DbSet<Chat> Chats { get; set; }
		public DbSet<Wallet> Wallets { get; set; }
		public DbSet<WalletTransaction> WalletTransactions { get; set; }
		public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
		public DbSet<UserSubscription> UserSubscriptions { get; set; }
		public DbSet<ConsultationUsage> ConsultationUsages { get; set; }
		public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<User>().ToTable("AspNetUsers")
			.Ignore(u => u.PhoneNumberConfirmed)
			.Ignore(u => u.TwoFactorEnabled)
			.Ignore(u => u.LockoutEnd)
			.Ignore(u => u.LockoutEnabled)
			.Ignore(u => u.AccessFailedCount);

			modelBuilder.Entity<IdentityRole>().Ignore(u => u.ConcurrencyStamp);

			modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable(null as string);
			modelBuilder.Entity<IdentityUserToken<string>>().ToTable(null as string);
			modelBuilder.Entity<IdentityUserLogin<string>>().ToTable(null as string);
			modelBuilder.Entity<IdentityUserClaim<string>>().ToTable(null as string);

			modelBuilder.Entity<Session>()
				.HasOne(s => s.User)
				.WithMany(u => u.Sessions)
				.HasForeignKey(s => s.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Appointment>()
				.HasOne(a => a.User)
				.WithMany(u => u.Appointments)
				.HasForeignKey(a => a.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Appointment>()
				.HasOne(a => a.Doctor)
				.WithMany(d => d.Appointments)
				.HasForeignKey(a => a.DoctorId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Notification>()
				.HasOne(n => n.User)
				.WithMany(u => u.Notifications)
				.HasForeignKey(n => n.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<MedicalRecord>()
				.HasOne(mr => mr.Appointment)
				.WithMany(a => a.MedicalRecords)
				.HasForeignKey(mr => mr.AppointmentId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Chat>()
				.HasOne(c => c.User)
				.WithMany(u => u.Chats)
				.HasForeignKey(c => c.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			// Doctor relationships
			modelBuilder.Entity<Doctor>()
				.HasOne(d => d.User)
				.WithMany()
				.HasForeignKey(d => d.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			// Configure Appointment indexes for better performance
			modelBuilder.Entity<Appointment>()
			.HasIndex(a => a.AppointmentDate)
			.HasDatabaseName("IX_Appointment_Date");

			modelBuilder.Entity<Appointment>()
				.HasIndex(a => a.Status)
				.HasDatabaseName("IX_Appointment_Status");

			modelBuilder.Entity<Appointment>()
				.HasIndex(a => new { a.DoctorId, a.AppointmentDate })
				.HasDatabaseName("IX_Appointment_Doctor_Date");

			modelBuilder.Entity<Appointment>()
				.Property(a => a.Symptoms)
				.HasMaxLength(1000)
				.IsRequired(false);

			// Subscription Plan configuration
			modelBuilder.Entity<SubscriptionPlan>()
				.Property(sp => sp.MonthlyPrice)
				.HasColumnType("decimal(18,2)");

			modelBuilder.Entity<SubscriptionPlan>()
				.Property(sp => sp.YearlyPrice)
				.HasColumnType("decimal(18,2)");

			modelBuilder.Entity<SubscriptionPlan>()
				.Property(sp => sp.DiscountPercentage)
				.HasColumnType("decimal(5,2)");

			modelBuilder.Entity<SubscriptionPlan>()
				.HasIndex(sp => sp.Name)
				.IsUnique()
				.HasDatabaseName("IX_SubscriptionPlan_Name");

			// User Subscription configuration
			modelBuilder.Entity<UserSubscription>()
				.HasOne(us => us.User)
				.WithMany()
				.HasForeignKey(us => us.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<UserSubscription>()
				.HasOne(us => us.SubscriptionPlan)
				.WithMany(sp => sp.UserSubscriptions)
				.HasForeignKey(us => us.PlanId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<UserSubscription>()
				.HasOne(us => us.PendingPlan)
				.WithMany()
				.HasForeignKey(us => us.PendingPlanId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<UserSubscription>()
				.Property(us => us.PaidAmount)
				.HasColumnType("decimal(18,2)");

			modelBuilder.Entity<UserSubscription>()
				.HasIndex(us => us.UserId)
				.HasDatabaseName("IX_UserSubscription_UserId");

			modelBuilder.Entity<UserSubscription>()
				.HasIndex(us => us.Status)
				.HasDatabaseName("IX_UserSubscription_Status");

			modelBuilder.Entity<UserSubscription>()
				.HasIndex(us => us.EndDate)
				.HasDatabaseName("IX_UserSubscription_EndDate");

			// Consultation Usage configuration
			modelBuilder.Entity<ConsultationUsage>()
				.HasOne(cu => cu.UserSubscription)
				.WithMany()
				.HasForeignKey(cu => cu.SubscriptionId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ConsultationUsage>()
				.HasOne(cu => cu.Appointment)
				.WithMany()
				.HasForeignKey(cu => cu.AppointmentId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ConsultationUsage>()
				.HasOne(cu => cu.User)
				.WithMany()
				.HasForeignKey(cu => cu.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ConsultationUsage>()
				.HasIndex(cu => new { cu.UserId, cu.UsageDate })
				.HasDatabaseName("IX_ConsultationUsage_User_Date");

			modelBuilder.Entity<ConsultationUsage>()
				.HasIndex(cu => cu.SubscriptionId)
				.HasDatabaseName("IX_ConsultationUsage_SubscriptionId");
		}
	}
}
