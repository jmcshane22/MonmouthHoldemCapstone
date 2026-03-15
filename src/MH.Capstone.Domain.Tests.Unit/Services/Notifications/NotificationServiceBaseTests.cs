using MH.Capstone.Domain.DataAccess;
using MH.Capstone.Domain.DataAccess.Repositories;
using MH.Capstone.Domain.DataModels;
using MH.Capstone.Domain.Services.Abstraction;
using MH.Capstone.Domain.Services.Notifications;
using Moq;
using Neleus.LambdaCompare;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MH.Capstone.Tests.SharedInternals;
using static MH.Capstone.Tests.SharedInternals.RandomData;

namespace MH.Capstone.Domain.Tests.Unit.Services.Notifications
{
    [TestFixture]
    [Parallelizable]
    [ExcludeFromCodeCoverage]
    public abstract class NotificationServiceBaseTests<TService, TNotifType>
        where TNotifType : Notification, new()
        where TService : NotificationServiceBase<TNotifType>
    {
        #region TestOverhead

        protected Mock<IRepository<TNotifType, ApplicationDbContext>> _mockNotificationRepository;
        protected Mock<IUserService> _mockUserService;
        protected TService _sut;

        [SetUp]
        public void SetUp()
        {
            _mockNotificationRepository =
                new Mock<IRepository<TNotifType, ApplicationDbContext>>();
            _mockUserService = new Mock<IUserService>();
            _sut = CreateSut();
        }

        protected abstract TService CreateSut();

        protected void AssertAllMockVerifications()
        {
            // Asserts that the methods that were set up in the Moq were called in ways that we set up
            _mockNotificationRepository.VerifyAll();

            // Asserts that the Moq mocks were only called in ways that we set up with the Setup method,
            // failing if any method was called that was not set up
            _mockNotificationRepository.VerifyNoOtherCalls();
        }

        #endregion
        #region GetAllNotificationsAsyncTests

        private static Expression<Func<TNotifType, bool>> CreateAllNotificationTestExpression(Guid userId)
            => n => n.LinkedUserIdentityId == userId.ToString();

        [Test]
        public async Task GetAllNotificationsAsyncGuidId_HasNoNotifications_ReturnsTaskWithEmptyEnumerable()
        {
            // Arrange
            var userToAssert = new ApplicationUser
            {
                GuidId = Guid.NewGuid()
            };
            var testExpression = CreateAllNotificationTestExpression(userToAssert.GuidId);

            _mockUserService.Setup(s =>
                s.GetUserByIdAsync(It.Is<Guid>(id => id == userToAssert.GuidId)))
                .ReturnsAsync(userToAssert).Verifiable(Times.Once);

            _mockNotificationRepository.Setup(r =>
                    r.GetAllAsync(
                        It.Is<Expression<Func<TNotifType, bool>>>(
                            e => Lambda.ExpressionsEqual(e, testExpression))
                    ))
                .ReturnsAsync(Enumerable.Empty<TNotifType>().AsQueryable())
                .Verifiable(Times.Once);

            // Act
            var result = (await _sut.GetAllNotificationsAsync(userToAssert.GuidId)).ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
            AssertAllMockVerifications();
        }

        [Test]
        public async Task GetAllNotificationsAsyncUserModel_HasNoNotifications_ReturnsTaskWithEmptyEnumerable()
        {
            // Arrange
            var user = new ApplicationUser
            {
                GuidId = Guid.NewGuid()
            };
            var testExpression = CreateAllNotificationTestExpression(user.GuidId);

            _mockNotificationRepository.Setup(r =>
                    r.GetAllAsync(
                        It.Is<Expression<Func<TNotifType, bool>>>(
                            e => Lambda.ExpressionsEqual(e, testExpression))
                    ))
                .ReturnsAsync(Enumerable.Empty<TNotifType>().AsQueryable())
                .Verifiable(Times.Once);

            // Act
            var result = (await _sut.GetAllNotificationsAsync(user)).ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
            AssertAllMockVerifications();
        }

        [Test]
        public void GetAllNotificationsAsyncGuidId_UserIdNotFound_ReturnsFailedTaskThrowingArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserService.Setup(s =>
                    s.UserExistsAsync(userId.ToString()))
                .ReturnsAsync(false).Verifiable(Times.Once);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => _sut.GetAllNotificationsAsync(userId));
            AssertAllMockVerifications();
        }

        [Test]
        public void GetAllNotificationsAsyncUserModel_UserIdNotFound_ReturnsFailedTaskThrowingArgumentException()
        {
            // Arrange
            var user = new ApplicationUser
            {
                GuidId = Guid.NewGuid()
            };
            var testExpression = CreateAllNotificationTestExpression(user.GuidId);

            _mockNotificationRepository.Setup(r =>
                r.GetAllAsync(
                    It.Is<Expression<Func<TNotifType, bool>>>(
                        e => Lambda.ExpressionsEqual(e, testExpression))
                ))
                .ThrowsAsync(new SqlExceptionBuilder().WithNumber(
                    (int)SqlErrorNumber.ForeignKeyConstraintViolation).Build())
                .Verifiable(Times.Once);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(() => _sut.GetAllNotificationsAsync(user));
            AssertAllMockVerifications();
        }

        #endregion

    }

    internal struct NotificationValidValuesSource
    {
        public static InAppNotification GetValidInAppNotification(Guid? recipientId = null,
            bool isRead = false, DateTimeOffset? sentAt = null) => 
            new InAppNotification(GetValidNotification(recipientId))
        {
            IsRead = isRead
        };

        public static EmailNotification GetValidEmailNotification(Guid? recipientId = null,
            bool isRead = false, DateTimeOffset? sentAt = null, 
            DateTimeOffset? deliveredAt = null) => 
            new EmailNotification(GetValidNotification(recipientId))
        {
            DeliveredAt = deliveredAt
        };

        public static Notification GetValidNotification(Guid? recipientId = null, 
            DateTimeOffset? sentAt = null) => new Notification(Guid.NewGuid(), 
                recipientId ?? Guid.NewGuid(),
                GetRandomStringOfLength(GetRandomIntInRange(1, 50)),
                GetRandomStringOfLength(GetRandomIntInRange(1, 250)),
                sentAt ?? DateTimeOffset.UtcNow);
    }
}