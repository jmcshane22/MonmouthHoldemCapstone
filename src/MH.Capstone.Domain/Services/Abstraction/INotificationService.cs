using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MH.Capstone.Domain.DataModels;

namespace MH.Capstone.Domain.Services.Abstraction
{
    public interface INotificationService<T> where T : Notification
    {
        /// <summary>
        /// Send a notification to a <see cref="ApplicationUser"/> that is defined within the <see cref="T"/> argument.
        /// </summary>
        /// <param name="notification">The <see cref="T"/> to send</param>
        /// <returns>A <see cref="Task"/> object representing the async task for this method</returns>
        Task SendNotificationAsync(T notification);

        //TODO - Allow for date-range filtering of notifications, and/or pagination of results for users with many notifications.
        /// <summary>
        /// Gets all notifications - both read and pending - for a user with the specified id.
        /// </summary>
        /// <param name="userId">The id of the user to get pending notifications for.</param>
        /// <returns>
        /// A <see cref="Task"/> object containing an ><see cref="IEnumerable{T}"/> of <see cref="T"/>
        /// that have been sent to the user. Will be empty if the user has no notifications.
        /// </returns>
        Task<IEnumerable<T>> GetAllNotificationsAsync(Guid userId);

        /// <summary>
        /// Gets all notifications - both read and pending - for the passed <see cref="ApplicationUser"/>.
        /// </summary>
        /// <param name="user">The <see cref="ApplicationUser"/> to get pending notifications for.</param>
        /// <returns>
        /// A <see cref="Task"/> object containing an ><see cref="IEnumerable{T}"/> of <see cref="T"/>
        /// that have been sent to the user. Will be empty if the user has no notifications.
        /// </returns>
        Task<IEnumerable<T>> GetAllNotificationsAsync(ApplicationUser user);
    }

    public interface IInAppNotificationService : INotificationService<InAppNotification>
    {
        //TODO - Allow for date-range filtering of notifications, and/or pagination of results for users with many notifications.
        /// <summary>
        /// Gets all pending notifications for a user with the specified id.
        /// Pending notifications are those that have not yet been read by the intended user.
        /// </summary>
        /// <param name="userId">The id of the user to get pending notifications for.</param>
        /// <returns>
        /// A <see cref="Task"/> object containing an ><see cref="IEnumerable{T}"/> of <see cref="InAppNotification"/>
        /// that have not yet been read by the intended user. Will be empty if the user has no pending notifications.
        /// </returns>
        Task<IEnumerable<InAppNotification>> GetPendingNotificationsAsync(Guid userId);

        /// <summary>
        /// Gets all pending notifications for the passed <see cref="ApplicationUser"/>.
        /// Pending notifications are those that have not yet been read by the intended user.
        /// </summary>
        /// <param name="user">The <see cref="ApplicationUser"/> to get pending notifications for.</param>
        /// <returns>
        /// A <see cref="Task"/> object containing an ><see cref="IEnumerable{T}"/> of <see cref="InAppNotification"/>
        /// that have not yet been read by the intended user. Will be empty if the user has no pending notifications.
        /// </returns>
        Task<IEnumerable<InAppNotification>> GetPendingNotificationsAsync(ApplicationUser user);
    }

    public interface IEmailNotificationService : INotificationService<EmailNotification>
    {
        /// <summary>
        /// Gets all email notifications that have been "sent to users" (added to the database for sending)
        /// but have not yet been set as "sent" by the email service.
        /// </summary>
        /// <returns>A Task containing the <see cref="IEnumerable{T}"/> of <see cref="EmailNotification"/> that
        /// haven't been sent (includes both never sent and send-failure). Returns an empty list of no records exist</returns>
        Task<IEnumerable<EmailNotification>> GetUnsentEmailNotificationsAsync();
    }

    public interface INotificationProcessor
    {
        /// <summary>
        /// Process all pending notifications that have not yet been sent to users by any delivery method.
        /// This method is intended to be called by a background worker service that runs on a schedule to ensure timely delivery of notifications.
        /// </summary>
        /// <returns>A <see cref="Task"/> object representing the async task for this method</returns>
        Task ProcessUnsentNotificationsAsync();
    }
}
