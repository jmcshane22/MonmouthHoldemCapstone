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
        public EmailNotificationService(IRepository<EmailNotification, ApplicationDbContext> repo, 
            IUserService userService, ILogger<INotificationService<EmailNotification>> logger) 
            : base(repo, userService, logger)
        { }

        public override async Task SendNotificationAsync(EmailNotification notification)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<EmailNotification>> GetUnsentEmailNotificationsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
