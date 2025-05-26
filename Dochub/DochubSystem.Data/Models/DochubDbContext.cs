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
		public DbSet<AppointmentTransaction> AppointmentTransactions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Chat> Chats { get; set; }

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

			modelBuilder.Entity<Appointment>()
				.HasMany<AppointmentTransaction>()
				.WithOne(t => t.Appointment)
				.HasForeignKey(t => t.AppointmentId)
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

			// Configure decimal precision for Price
			modelBuilder.Entity<Appointment>()
				.Property(a => a.Price)
				.HasColumnType("decimal(18,2)");
		}
    }
}
