using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MH.Capstone.Domain.Services.Abstraction;
using Microsoft.Extensions.Logging;

namespace MH.Capstone.Domain.Services.Notifications
{
    public class NotificationProcessor : INotificationProcessor
    {
        private readonly ILogger<INotificationProcessor> _logger;
        private readonly INotificationServiceFactory _notifServiceFactory;

        public NotificationProcessor(INotificationServiceFactory notifServiceFactory, 
            ILogger<INotificationProcessor> logger)
        {
            _notifServiceFactory = notifServiceFactory;
            _logger = logger;
        }

        public async Task ProcessUnsentNotificationsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
