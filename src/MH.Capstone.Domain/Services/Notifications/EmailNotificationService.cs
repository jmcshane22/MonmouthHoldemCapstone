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
    public class EmailNotificationService : NotificationServiceBase<EmailNotification>, IEmailNotificationService
    {
        private const int _MAX_FAILURE_COUNT = 3;
        private const int _RETRY_DELAY_MINUTES = 5;

        public EmailNotificationService(IRepository<EmailNotification, ApplicationDbContext> repo, 
            IUserService userService, ILogger<INotificationService<EmailNotification>> logger) 
            : base(repo, userService, logger)
        { }

        public override async Task SendNotificationAsync(EmailNotification notification)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<EmailNotification>> GetUnsentEmailNotificationsAsync() =>
            await _notificationRepo.GetAllAsync(n => n.DeliveredAt == null // Not delivered yet
                && n.SentAt.ToUniversalTime() < DateTimeOffset.UtcNow // Scheduled to be sent already
                && n.FailureCount < _MAX_FAILURE_COUNT // Hasn't failed too many times
                && (n.LastFailure == null // Never failed before
                    || n.LastFailure.Value.ToUniversalTime()
                        .AddMinutes(n.FailureCount * _RETRY_DELAY_MINUTES) < DateTimeOffset.UtcNow) // Enough time has passed since last failure
                );
    }
}
