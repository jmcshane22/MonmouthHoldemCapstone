using System;
using System.Collections.Generic;
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
        private readonly IInAppNotificationService _inAppNotifService;
        private readonly IEmailNotificationService _emailNotifService;
        private readonly IRepository<Notification, ApplicationDbContext> _notifRepo;
        //private readonly IRepository<InAppNotification, ApplicationDbContext> _inAppNotifRepo;
        //private readonly IRepository<EmailNotification, ApplicationDbContext> _emailNotifRepo;

        public NotificationProcessor(ILogger<INotificationProcessor> logger,
            IInAppNotificationService inAppNotifService,
            IEmailNotificationService emailNotifService,
            IRepository<Notification, ApplicationDbContext> notifRepo)
            //IRepository<InAppNotification, ApplicationDbContext> inAppNotifRepo,
            //IRepository<EmailNotification, ApplicationDbContext> emailNotifRepo)
        {
            _logger = logger;
            _inAppNotifService = inAppNotifService;
            _emailNotifService = emailNotifService;
            _notifRepo = notifRepo;
            //_inAppNotifRepo = inAppNotifRepo;
            //_emailNotifRepo = emailNotifRepo;
        }

        public async Task ProcessUnsentNotificationsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
