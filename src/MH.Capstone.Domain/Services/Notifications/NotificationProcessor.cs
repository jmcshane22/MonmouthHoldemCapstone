using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MH.Capstone.Domain.DataAccess;
using MH.Capstone.Domain.DataAccess.Repositories;
using MH.Capstone.Domain.DataModels;
using MH.Capstone.Domain.Services.Abstraction;
using Microsoft.Extensions.Logging;

namespace MH.Capstone.Domain.Services.Notifications
{
    public class NotificationProcessor : INotificationProcessor
    {
        private readonly ILogger<INotificationProcessor> _logger;
        private readonly IInAppNotificationService _inAppService;
        private readonly IEmailNotificationService _emailService;
        private readonly IRepository<InAppNotification, ApplicationDbContext> _inAppRepo;
        private readonly IRepository<EmailNotification, ApplicationDbContext> _emailRepo;

        public NotificationProcessor(ILogger<INotificationProcessor> logger, 
            IInAppNotificationService inAppService, IEmailNotificationService emailService,
            IRepository<InAppNotification, ApplicationDbContext> inAppRepo,
            IRepository<EmailNotification, ApplicationDbContext> emailRepo)
        {
            _logger = logger;
            _inAppService = inAppService;
            _emailService = emailService;
            _inAppRepo = inAppRepo;
            _emailRepo = emailRepo;
        }

        public async Task ProcessPendingNotificationsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task EnqueueNotificationForProcessingAsync(Notification notification)
        {
            throw new NotImplementedException();
        }

        private async Task ProcessInAppNotificationAsync(InAppNotification notification)
        {
            throw new NotImplementedException();
        }

        private async Task ProcessEmailNotificationAsync(EmailNotification notification)
        {
            throw new NotImplementedException();
        }

        private async Task EnqueueInAppNotificationAsync(InAppNotification notification)
        {
            throw new NotImplementedException();
        }

        private async Task EnqueueEmailNotificationAsync(EmailNotification notification)
        {
            throw new NotImplementedException();
        }
    }
}
