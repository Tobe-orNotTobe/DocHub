using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.RepositoryContract.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
		IDoctorRepository Doctors { get; }
		IAppointmentRepository Appointments { get; }
		IWalletRepository Wallets { get; }
		IWalletTransactionRepository WalletTransactions { get; }
		ISubscriptionPlanRepository SubscriptionPlans { get; }
		IUserSubscriptionRepository UserSubscriptions { get; }
		IConsultationUsageRepository ConsultationUsages { get; }
		IPaymentTransactionRepository PaymentTransactions { get; }

		Task<int> CompleteAsync();

        Task<IDbContextTransaction> BeginTransactionAsync();

    }
}
