using MH.Capstone.Domain.DataAccess;
using MH.Capstone.Domain.DataAccess.Repositories;
using MH.Capstone.Domain.DataModels;
using MH.Capstone.Domain.Services.Abstraction;
using MH.Capstone.Domain.Tools;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MH.Capstone.Domain.Services.Notifications
{
    public abstract class NotificationServiceBase<T> : INotificationService<T> where T : Notification, new()
    {
        protected readonly IRepository<T, ApplicationDbContext> _notificationRepo;
        protected readonly IUserService _userService;
        protected readonly ILogger<INotificationService<T>> _logger;

        protected NotificationServiceBase(IRepository<T, ApplicationDbContext> repo,
            IUserService userService, ILogger<INotificationService<T>> logger)
        {
            _notificationRepo = repo;
            _userService = userService;
            _logger = logger;
        }

        // Abstract method since the actual implementation will depend on the specific type of notification
        // service (ie email, in-app, sms, etc.)
        public abstract Task SendNotificationAsync(T notification);

        public async Task<IEnumerable<T>> GetAllNotificationsAsync(Guid userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User with id {userId} not found", nameof(userId));
            }

            return await GetAllNotificationsAsync(user);
        }

        public async Task<IEnumerable<T>> GetAllNotificationsAsync(ApplicationUser user)
            => await HandleSqlErrors(_notificationRepo.GetAllAsync(n => 
                n.LinkedUserIdentityId == user.GuidId.ToString()));

        protected static async Task<TOut> HandleSqlErrors<TOut>(Task<TOut> repoCmd, string? paramName = null)
        {
            try
            {
                return await repoCmd;
            }
            catch (Exception ex)
            {
                // Have to do the exception type checks this way because the underlying code is the same, but you cannot catch 
                // both types in the same catch block. Using an if statement on the base exception type allows us to check for
                // both without code duplication (DRY).
                if ((ex is SqlException sqlEx && sqlEx.IsOfErrorType(SqlErrorNumber.ForeignKeyConstraintViolation)) ||
                    (ex is DbUpdateException dbEx && dbEx.IsOfErrorType(SqlErrorNumber.ForeignKeyConstraintViolation)))
                {
                    throw new ArgumentException($"The {paramName ?? "UNK PARAM"} does not correspond " +
                                                $"to an existing record in the database.", paramName, ex);
                }

                throw;
            }
        }
    }
}