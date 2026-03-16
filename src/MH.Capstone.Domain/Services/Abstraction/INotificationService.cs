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
        /// but have not yet been set as "sent" by this email service.
        /// </summary>
        /// <returns>A Task containing the <see cref="IEnumerable{T}"/> of <see cref="EmailNotification"/> that
        /// haven't been sent (includes both never sent and send-failure). Returns an empty list if no records exist</returns>
        Task<IEnumerable<EmailNotification>> GetUnsentEmailNotificationsAsync();
    }

    /// <summary>
    /// This interface is intended to be implemented by a background service that continuously checks for unsent notifications.
    /// This should not be called directly by controllers or other services, but instead should be implemented by a background service
    /// that runs on a timer or similar mechanism to continuously check for unsent notifications and process them as needed.
    /// </summary>
    public interface INotificationProcessor
    {
        /// <summary>
        /// Processes all unsent notifications asynchronously using calls to the relevant <see cref="INotificationService{T}"/> (s).
        /// This method also handles failure/retry logic for failed notifications when applicable and updating the relevant notification
        /// records in the database accordingly (e.g., incrementing failure count, setting last failure time, adding derived notification
        /// (InApp, Email, etc.) to the correct table, etc.).
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task completes when all unsent notifications have
        /// been processed.</returns>
        Task ProcessUnsentNotificationsAsync();
    }
}