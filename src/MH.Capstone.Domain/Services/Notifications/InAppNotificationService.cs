using MH.Capstone.Domain.DataAccess;
using MH.Capstone.Domain.DataAccess.Repositories;
using MH.Capstone.Domain.DataModels;
using MH.Capstone.Domain.Services.Abstraction;
using MH.Capstone.Domain.Tools;
using Microsoft.Extensions.Logging;

namespace MH.Capstone.Domain.Services.Notifications
{
    public class InAppNotificationService : NotificationServiceBase<InAppNotification>, IInAppNotificationService
    {
        public InAppNotificationService(IRepository<InAppNotification, ApplicationDbContext> repo,
            IUserService userService, ILogger<INotificationService<InAppNotification>> logger) 
            : base(repo, userService, logger) { }

        public override async Task SendNotificationAsync(InAppNotification notification)
        {
            if (!notification.TryValidateEntity(out var errors))
            {
                var errList = errors.ToList();
                var errPropNames = string.Empty;
                errList.ForEach(prop => errPropNames += $" {prop}");
                throw new ArgumentException($"The argument {nameof(notification)} has {errList.Count} " +
                                            $"data annotation violations...\n" +
                                            $"\tInvalid Properties: {errPropNames}", 
                    nameof(notification));
            }

            // Ensure the notification is marked as unread when sent
            notification.IsRead = false;

            await HandleSqlErrors(_notificationRepo.AddOrUpdateAsync(notification),nameof(notification));
        }

        public async Task<IEnumerable<InAppNotification>> GetPendingNotificationsAsync(Guid userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with id {userId} not found", nameof(userId));
            }

            return await GetPendingNotificationsAsync(user);
        }

        public async Task<IEnumerable<InAppNotification>> GetPendingNotificationsAsync(ApplicationUser user)
            => await HandleSqlErrors(_notificationRepo.GetAllAsync(n =>
                n.LinkedUserIdentityId == user.GuidId.ToString() && !n.IsRead));
    }
}
