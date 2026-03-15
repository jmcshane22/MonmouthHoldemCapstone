using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MH.Capstone.Domain.Constants
{
    public enum NotificationType
    {
        /// <summary>
        /// Default value to indicate an uninitialized state; should not be used in practice
        /// </summary>
        Undefined = -1,
        /// <summary>
        /// User receives for critical alerts related to their account or the app in general.
        /// Should be used in situations where it is needed to override user notification settings
        /// to ensure they receive critical information.
        /// </summary>
        SystemCriticalAlert = 0,
        /// <summary>
        /// User receives when their sighting is successfully uploaded
        /// </summary>
        SightingUploaded = 1,
        /// <summary>
        /// User receives when they submit a report of an issue (e.g., inappropriate content, app bug, etc.)
        /// </summary>
        ReportSubmitted = 2,
        /// <summary>
        /// For account-related notifications like password changes, login from new device, etc.
        /// DO NOT USE for critical security alerts that should override user settings;
        /// those should use the <see cref="SystemCriticalAlert"/> type instead.
        /// </summary>
        AccountNotification = 3,
    }
}
