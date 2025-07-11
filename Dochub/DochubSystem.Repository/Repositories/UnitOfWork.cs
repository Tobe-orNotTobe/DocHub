﻿using DochubSystem.Data.Models;
using DochubSystem.RepositoryContract.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DochubSystem.Repository.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly DochubDbContext _context;

		public IUserRepository Users { get; }
		public IDoctorRepository Doctors { get; }
		public IAppointmentRepository Appointments { get; }
		public IWalletRepository Wallets { get; }
		public IWalletTransactionRepository WalletTransactions { get; }
		public ISubscriptionPlanRepository SubscriptionPlans { get; }
		public IUserSubscriptionRepository UserSubscriptions { get; }
		public IConsultationUsageRepository ConsultationUsages { get; }
		public INotificationRepository Notifications { get; }
		public INotificationTemplateRepository NotificationTemplates { get; }
		public INotificationQueueRepository NotificationQueues { get; }
		public INotificationHistoryRepository NotificationHistories { get; }
		public IChatRepository Chats { get; }
		public IPaymentRequestRepository PaymentRequests { get; }
		public ITransactionRecordRepository TransactionRecords { get; }

		public UnitOfWork(DochubDbContext context,
					   IUserRepository userRepository,
						IDoctorRepository doctorRepository,
					   IAppointmentRepository appointmentRepository,
					   IWalletRepository walletRepository,
					   IWalletTransactionRepository walletTransactionRepository,
						 ISubscriptionPlanRepository subscriptionPlanRepository,
			   IUserSubscriptionRepository userSubscriptionRepository,
				IConsultationUsageRepository consultationUsageRepository,
				 INotificationRepository notificationRepository,
						 INotificationTemplateRepository notificationTemplateRepository,
						 INotificationQueueRepository notificationQueueRepository,
						 INotificationHistoryRepository notificationHistoryRepository, IChatRepository chatRepository, 
						 IPaymentRequestRepository paymentRequestRepository,
							 ITransactionRecordRepository transactionRecordRepository)
		{
			_context = context;
			Users = userRepository;
			Doctors = doctorRepository;
			Appointments = appointmentRepository;
			Wallets = walletRepository;
			WalletTransactions = walletTransactionRepository;
			SubscriptionPlans = subscriptionPlanRepository;
			UserSubscriptions = userSubscriptionRepository;
			ConsultationUsages = consultationUsageRepository;
			Notifications = notificationRepository;
			NotificationTemplates = notificationTemplateRepository;
			NotificationQueues = notificationQueueRepository;
			NotificationHistories = notificationHistoryRepository;
			Chats = chatRepository;
			PaymentRequests = paymentRequestRepository;
			TransactionRecords = transactionRecordRepository;
		}


		public async Task<int> CompleteAsync()
		{
			return await _context.SaveChangesAsync();
		}
		public async Task<IDbContextTransaction> BeginTransactionAsync()
		{
			return await _context.Database.BeginTransactionAsync();
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
