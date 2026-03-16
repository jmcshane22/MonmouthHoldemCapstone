using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MH.Capstone.Domain.Constants;
using MH.Capstone.Domain.DataAccess;
using MH.Capstone.Domain.DataAccess.Repositories;
using MH.Capstone.Domain.DataModels;
using MH.Capstone.Domain.Services.Abstraction;
using MH.Capstone.Domain.Services.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Neleus.LambdaCompare;

namespace MH.Capstone.Domain.Tests.Unit.Services.Notifications
{
    [TestFixture]
    [Parallelizable]
    [ExcludeFromCodeCoverage]
    public class NotificationProcessorTests
    {
        #region TestingOverhead

        private Mock<IInAppNotificationService> _inAppNotifServiceMock;
        private Mock<IEmailNotificationService> _emailNotifServiceMock;
        private Mock<IRepository<Notification, ApplicationDbContext>> _notifRepoMock;
        private NotificationProcessor _sut;

        [SetUp]
        public void SetUp()
        {
            _inAppNotifServiceMock = new Mock<IInAppNotificationService>();
            _emailNotifServiceMock = new Mock<IEmailNotificationService>();
            _notifRepoMock = new Mock<IRepository<Notification, ApplicationDbContext>>();
            _sut = CreateSut();
        }

        private NotificationProcessor CreateSut() =>
            new NotificationProcessor(NullLogger<INotificationProcessor>.Instance,
                _inAppNotifServiceMock.Object, _emailNotifServiceMock.Object,
                _notifRepoMock.Object);

        private void AssertVerifyAllMockSetups()
        {
            _inAppNotifServiceMock.Verify();
            _inAppNotifServiceMock.VerifyNoOtherCalls();

            _emailNotifServiceMock.Verify();
            _emailNotifServiceMock.VerifyNoOtherCalls();

            _notifRepoMock.Verify();
            _notifRepoMock.VerifyNoOtherCalls();
        }

        #endregion

        #region ProcessUnsentNotificationsAsyncTests

        //private readonly Expression<Func<Notification, bool>> _expectedUnsentNotifsExpression = n =>
        //    n.LinkedEmail == null && n.LinkedInApp == null && n.SentAt.ToUniversalTime() < DateTimeOffset.UtcNow;
        private readonly Expression<Func<Notification, bool>> _expectedUnsentNotifsExpression = n => true;

        public async Task
            ProcessUnsentNotificationsAsync_WithRangingTypesAndDeliveryMethodsForUser_ProcessesSuccessfully(
                NotificationType notifType, NotificationDeliveryMethods deliveryMethods)
        {
            // Arrange


            // Act


            // Assert

        }

        [Test]
        [Ignore("WIP")]
        public async Task ProcessUnsentNotificationsAsync_EmailNotifError_HandlesErrorGracefullyAndUpdatesEntity()
        {
            // Arrange
            var user = new ApplicationUser
            {
                GuidId = Guid.NewGuid(),
                Email = "fake@waid.net",
                NotificationSettings =
                [
                    new UserNotificationSetting
                    {
                        DeliveryMethods = NotificationDeliveryMethods.Email,
                        NotificationType = NotificationType.AccountNotification
                    }
                ]
            };
            user.NotificationSettings[0].User = user;
            user.NotificationSettings[0].UserId = user.GuidId;

            var baseNotif = new Notification(user.GuidId, "Testing Failure", "A test of failure handling...",
                DateTimeOffset.Now)
            {
                Recipient = user,
                RecipientId = user.GuidId
            };

            var expected = new List<Notification>
            {
                baseNotif
            };

            _notifRepoMock.Setup(r => 
                r.GetAllAsync(It.Is<Expression<Func<Notification, bool>>>(e => 
                    Lambda.ExpressionsEqual(e, _expectedUnsentNotifsExpression))))
                .ReturnsAsync(expected.AsQueryable()).Verifiable(Times.Once);

            _notifRepoMock.Setup(r => r.AddOrUpdateAsync(
                It.Is<EmailNotification>(n => n.Id == baseNotif.Id 
                                              && string.Equals(baseNotif.Title, n.Title, StringComparison.InvariantCultureIgnoreCase)
                                              && string.Equals(baseNotif.Message, n.Message, StringComparison.InvariantCultureIgnoreCase)
                                              && baseNotif.SentAt == n.SentAt
                                              && n.LastFailure != null
                                              && n.FailureCount == 1)))
                .Verifiable(Times.Once);

            // Act
            await _sut.ProcessUnsentNotificationsAsync();

            // Assert
            AssertVerifyAllMockSetups();
        }

        [Test]
        [Ignore("WIP")]
        public async Task ProcessUnsentNotificationsAsync_NoUnsentNotificationsExist_ExitsWithoutProcessing()
        {
            // Arrange
            var expectedNotifs = Array.Empty<Notification>();

            _notifRepoMock.Setup(r => 
                r.GetAllAsync(It.Is<Expression<Func<Notification, bool>>>(e => 
                    Lambda.ExpressionsEqual(e, _expectedUnsentNotifsExpression))))
                .ReturnsAsync(expectedNotifs.AsQueryable()).Verifiable(Times.Once);
            
            // Act
            await _sut.ProcessUnsentNotificationsAsync();

            // Assert
            AssertVerifyAllMockSetups();
        }

        #endregion
    }
}
