using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MH.Capstone.Domain.Constants;
using MH.Capstone.Domain.DataAccess;
using MH.Capstone.Domain.DataAccess.Repositories;
using MH.Capstone.Domain.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MH.Capstone.Domain.DataModels
{
    [Table("Notification")]
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [NotMapped]
        public Guid RecipientId
        {
            get => Guid.Parse(LinkedUserIdentityId);
            set => LinkedUserIdentityId =
                value.ToString(); // Convert Guid to string for storage in the AspNetCore Identity ID column
        }

        [Required]
        [Column("RecipientId")]
        [MaxLength(450)] // The size of the RecipientId column should match the size of the primary key in the AspNetUsers table (nvarchar(450))
        [ForeignKey(nameof(Recipient))]
        public string LinkedUserIdentityId { get; set; } = null!;

        [Required]
        [MinLength(1)]
        [MaxLength(50)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        [MaxLength(250)]
        public string Message { get; set; } = string.Empty;

        [Required] 
        public DateTimeOffset SentAt { get; set; }

        [Required]
        [EnumDataType(typeof(NotificationType))]
        public NotificationType Type { get; set; } = NotificationType.Undefined;

        [JsonIgnore]
        public virtual ApplicationUser Recipient { get; set; } = null!;

        // EF Core requires a parameterless constructor for materialization of entities from the database,
        // so we need to include one even though we don't want it to be used directly in our code.
        public Notification()
        { }

        // For general use within the application, we want to require all properties to be set at the time of object creation,
        // so we provide this constructor for that purpose.
        public Notification(Guid recipientId, string title, string message, DateTimeOffset sentAt)
        {
            RecipientId = recipientId;
            Title = title;
            Message = message;
            SentAt = sentAt;
        }

        // For testing and general use within the application, we want to require all properties to be set
        // at the time of object creation, so we provide this constructor for that purpose.
        public Notification(Guid id, Guid recipientId, string title, string message, DateTimeOffset sentAt)
        {
            Id = id;
            RecipientId = recipientId;
            Title = title;
            Message = message;
            SentAt = sentAt;
        }

        /// <summary>
        /// Enqueues this notification for processing by saving it to the database using the provided repository.
        /// </summary>
        /// <param name="repo">The repository for this entity</param>
        /// <returns>A <see cref="Task"/> object representing this operation.</returns>
        public async Task EnqueueForProcessing(IRepository<Notification, ApplicationDbContext> repo)
            => await this.SaveModelAsync(repo);
    }

    [Table("EmailNotification")]
    public class EmailNotification : Notification
    {
        public DateTimeOffset? DeliveredAt { get; set; } = null;

        public DateTimeOffset? LastFailure { get; set; } = null;

        [Range(0, 3)] // Allow at most 3 failures before giving up on sending the email notification.
        public int FailureCount { get; set; } = 0;

        // For EF Core materialization of entities from the database, we need to include a parameterless constructor.
        public EmailNotification() { }
        
        public EmailNotification(Guid recipientId, string title, string message, DateTimeOffset sentAt) 
            : base(recipientId, title, message, sentAt)
        { }

        public EmailNotification(Notification baseNotification) : base(baseNotification.Id, 
            baseNotification.RecipientId, baseNotification.Title, baseNotification.Message, 
            baseNotification.SentAt)
        { }
    }

    [Table("InAppNotification")]
    public class InAppNotification : Notification
    {
        public bool IsRead { get; set; } = false;

        // For EF Core materialization of entities from the database, we need to include a parameterless constructor.
        public InAppNotification() { }
        
        public InAppNotification(Guid recipientId, string title, string message, DateTimeOffset sentAt) 
            : base(recipientId, title, message, sentAt)
        { }

        public InAppNotification(Notification baseNotification) : base(baseNotification.Id, 
            baseNotification.RecipientId, baseNotification.Title, baseNotification.Message, 
            baseNotification.SentAt)
        { }
    }
}