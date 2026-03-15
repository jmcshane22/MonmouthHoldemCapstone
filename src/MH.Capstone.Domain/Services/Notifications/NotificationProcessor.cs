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
    public class InAppNotificationProcessor : INotificationProcessor
    {
        private readonly ILogger<INotificationProcessor> _logger;
        private readonly IInAppNotificationService _inAppService;
        private readonly IRepository<InAppNotification, ApplicationDbContext> _inAppRepo;

        public InAppNotificationProcessor(ILogger<INotificationProcessor> logger,
            IInAppNotificationService inAppService,
            IRepository<InAppNotification, ApplicationDbContext> inAppRepo)
        {
            _logger = logger;
            _inAppService = inAppService;
            _inAppRepo = inAppRepo;
        }

        public async Task ProcessUnsentNotificationsAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class EmailNotificationProcessor : INotificationProcessor
    {
        private readonly ILogger<INotificationProcessor> _logger;
        private readonly IEmailNotificationService _emailService;
        private readonly IRepository<EmailNotification, ApplicationDbContext> _emailRepo;
        public EmailNotificationProcessor(ILogger<INotificationProcessor> logger,
            IEmailNotificationService emailService,
            IRepository<EmailNotification, ApplicationDbContext> emailRepo)
        {
            _logger = logger;
            _emailService = emailService;
            _emailRepo = emailRepo;
        }

        public async Task ProcessUnsentNotificationsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
