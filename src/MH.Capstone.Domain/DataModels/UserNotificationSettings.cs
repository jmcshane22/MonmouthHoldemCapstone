using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MH.Capstone.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace MH.Capstone.Domain.DataModels
{
    // PK is a composite key of UserIdentityId and NotificationType,
    // since this is a 1:1 relationship with ApplicationUser for each notification type,
    // thus enforcing that there can only be one setting for each notification type per user.
    [PrimaryKey(nameof(UserIdentityId), nameof(NotificationType))]
    [Table("UserNotificationSetting")]
    public class UserNotificationSetting
    {
        // Foreign key to AspNetUsers.Id; Also table PK since this is a 1:1 relationship with ApplicationUser
        [Column("UserId")]
        [MaxLength(450)] // The size of the UserId column should match the size of the primary key in the AspNetUsers table (nvarchar(450))
        [ForeignKey(nameof(User))]
        public string UserIdentityId { get; set; } = null!;

        [Required]
        [Column("Type")]
        [EnumDataType(typeof(NotificationType))]
        public NotificationType NotificationType { get; set; }

        [Required]
        [EnumDataType(typeof(NotificationDeliveryMethods))]
        public NotificationDeliveryMethods DeliveryMethods { get; set; }

        // This property is not mapped to the database, but provides a convenient way
        // to work with the UserId as a Guid instead of a string. Will read and persist
        // to the underlying string UserIdentityId property, so this property should
        // always be equivalent to Guid.Parse(UserIdentityId), including when setting a new id.
        [NotMapped]
        public Guid UserId
        {
            get => Guid.Parse(UserIdentityId);
            set => UserIdentityId = value.ToString();
        }

        // Note: The properties in this region should match up with the values in the NotificationType enum,
        // and should be used to store user preferences for each notification type that override the default delivery method
        // that is a property of every ApplicationUser.
        [NotMapped]
        public ApplicationUser User { get; set; } = null!;
    }
}
