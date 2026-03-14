using MH.Capstone.Domain.DataModels;
using MH.Capstone.Domain.Services.Abstraction;
using MH.Capstone.Domain.Services.Notifications;
using MH.Capstone.Tests.SharedInternals;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Neleus.LambdaCompare;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace MH.Capstone.Domain.Tests.Unit.Services.Notifications
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class InAppNotificationServiceTests : NotificationServiceBaseTests<InAppNotificationService, InAppNotification>
    {
        #region TestOverhead

        protected override InAppNotificationService CreateSut()
            => new InAppNotificationService(_mockNotificationRepository.Object,
                _mockUserService.Object,
                NullLogger<INotificationService<InAppNotification>>.Instance);

        #endregion
        #region SendNotificationAsyncTests

        [TestCase(false)]
        // This should get reset to false when the notification is sent,
        // but we want to make sure that happens regardless of the initial value of IsRead
        [TestCase(true)]
        public async Task SendNotificationAsync_NotificationValid_CreatesNotificationRecordInDbAsUnread(bool readValue)
        {
            // Arrange
            var recipientId = Guid.NewGuid();
            var notif = NotificationValidValuesSource.GetValidInAppNotification(recipientId, readValue);

            // Expect AddOrUpdateAsync to be called with the notification marked as unread (IsRead == false)
            _mockNotificationRepository
                .Setup(r => r.AddOrUpdateAsync(It.Is<InAppNotification>(n =>
                    n.LinkedUserIdentityId == notif.LinkedUserIdentityId &&
                    n.Title == notif.Title &&
                    n.Message == notif.Message &&
                    n.SentAt == notif.SentAt &&
                    n.IsRead == false)))
                .ReturnsAsync((InAppNotification n) => n)
                .Verifiable();

            // Act
            var sut = CreateSut();
            await sut.SendNotificationAsync(notif);

            // Assert
            AssertAllMockVerifications();
        }

        [Test]
        public void SendNotificationAsync_RecipientIdNotFound_ReturnsFailedTaskThrowingArgumentException()
        {
            // Arrange
            var notif = NotificationValidValuesSource.GetValidInAppNotification();

            // Simulate SQL foreign key violation when attempting to persist - repository throws DbUpdateException with inner SqlException(547)
            _mockNotificationRepository
                .Setup(r => r.AddOrUpdateAsync(
                    It.Is<InAppNotification>(n => n.Id == notif.Id)))
                .ThrowsAsync(new DbUpdateException("Foreign key violation",
                    new SqlExceptionBuilder().WithNumber(547).Build()))
                .Verifiable();

            // Act
            var sut = CreateSut();

            // Assert
            Assert.ThrowsAsync<ArgumentException>(() => sut.SendNotificationAsync(notif));
            AssertAllMockVerifications();
        }

        [TestCase(-1)] // Means null
        [TestCase(0)] // Means empty string
        [TestCase(52)] // Exceeds max length of 50
        public void SendNotificationAsync_InvalidTitle_ReturnsFailedTaskThrowingArgumentException(int titleLen)
        {
            // Arrange
            var notif = NotificationValidValuesSource.GetValidInAppNotification();

            if (titleLen == -1)
                notif.Title = null!;
            else if (titleLen == 0)
                notif.Title = string.Empty;
            else
                notif.Title = RandomData.GetRandomStringOfLength(titleLen);

            var sut = CreateSut();

            // Act & Assert - should fail validation before repository is called
            Assert.ThrowsAsync<ArgumentException>(() => sut.SendNotificationAsync(notif));
            AssertAllMockVerifications();
        }

        [TestCase(-1)] // Means null
        [TestCase(0)] // Means empty string
        [TestCase(257)] // Exceeds max length of 250
        public void SendNotificationAsync_InvalidMessage_ReturnsFailedTaskThrowingArgumentException(int messageLen)
        {
            // Arrange
            var notif = NotificationValidValuesSource.GetValidInAppNotification();

            if (messageLen == -1)
                notif.Message = null!;
            else if (messageLen == 0)
                notif.Message = string.Empty;
            else
                notif.Message = RandomData.GetRandomStringOfLength(messageLen);

            var sut = CreateSut();

            // Act & Assert - should fail validation before repository is called
            Assert.ThrowsAsync<ArgumentException>(() => sut.SendNotificationAsync(notif));
            AssertAllMockVerifications();
        }

        #endregion
        #region GetPendingNotificationsAsyncTests

        private static Expression<Func<InAppNotification, bool>> CreatePendingNotificationTestExpression(Guid userId)
            => n => n.LinkedUserIdentityId == userId.ToString() && !n.IsRead;

        [Test]
        public async Task GetPendingNotificationsAsyncGuidId_HasNoPendingNotifications_ReturnsTaskWithEmptyEnumerable()
        {
            // Arrange
            var userToAssert = new ApplicationUser
            {
                GuidId = Guid.NewGuid()
            };
            var testExpression = CreatePendingNotificationTestExpression(userToAssert.GuidId);

            _mockUserService.Setup(s =>
                s.GetUserByIdAsync(It.Is<Guid>(id => id == userToAssert.GuidId)))
                .ReturnsAsync(userToAssert).Verifiable(Times.Once);

            _mockNotificationRepository.Setup(r =>
                    r.GetAllAsync(
                        It.Is<Expression<Func<InAppNotification, bool>>>(
                            e => Lambda.ExpressionsEqual(e, testExpression))
                    ))
                .ReturnsAsync(Enumerable.Empty<InAppNotification>().AsQueryable())
                .Verifiable(Times.Once);

            // Act
            var sut = CreateSut();
            var result = (await sut.GetPendingNotificationsAsync(userToAssert.GuidId)).ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetPendingNotificationsAsyncUserModel_HasNoPendingNotifications_ReturnsTaskWithEmptyEnumerable()
        {
            // Arrange
            var user = new ApplicationUser
            {
                GuidId = Guid.NewGuid()
            };
            var testExpression = CreatePendingNotificationTestExpression(user.GuidId);

            _mockNotificationRepository.Setup(r =>
                    r.GetAllAsync(
                        It.Is<Expression<Func<InAppNotification, bool>>>(
                            e => Lambda.ExpressionsEqual(e, testExpression))
                    ))
                .ReturnsAsync(Enumerable.Empty<InAppNotification>().AsQueryable())
                .Verifiable(Times.Once);

            // Act
            var sut = CreateSut();
            var result = (await sut.GetPendingNotificationsAsync(user)).ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task
            GetPendingNotificationsAsyncGuidId_HasPendingNotifications_ReturnsTaskWithEnumerableOfPendingNotifications()
        {
            // Arrange
            var userToAssert = new ApplicationUser
            {
                GuidId = Guid.NewGuid()
            };
            var pending = new[]
            {
                NotificationValidValuesSource.GetValidInAppNotification(userToAssert.GuidId),
                NotificationValidValuesSource.GetValidInAppNotification(userToAssert.GuidId)
            };
            var testExpression = CreatePendingNotificationTestExpression(userToAssert.GuidId);

            _mockUserService.Setup(s =>
                    s.GetUserByIdAsync(It.Is<Guid>(id => id == userToAssert.GuidId)))
                .ReturnsAsync(userToAssert).Verifiable(Times.Once);

            _mockNotificationRepository.Setup(r =>
                    r.GetAllAsync(
                        It.Is<Expression<Func<InAppNotification, bool>>>(
                            e => Lambda.ExpressionsEqual(e, testExpression))
                    ))
                .ReturnsAsync(pending.AsQueryable())
                .Verifiable(Times.Once);

            // Act
            var sut = CreateSut();
            var result = (await sut.GetPendingNotificationsAsync(userToAssert.GuidId)).ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(pending.Length));
            Assert.That(
                result.Select(n => n.Id),
                Is.EquivalentTo(pending.Select(p => p.Id))
            );
            AssertAllMockVerifications();
        }

        [Test]
        public async Task
            GetPendingNotificationsAsyncUserModel_HasPendingNotifications_ReturnsTaskWithEnumerableOfPendingNotifications()
        {
            // Arrange
            var user = new ApplicationUser
            {
                GuidId = Guid.NewGuid()
            };
            var pending = new[]
            {
                NotificationValidValuesSource.GetValidInAppNotification(user.GuidId),
                NotificationValidValuesSource.GetValidInAppNotification(user.GuidId)
            };
            var testExpression = CreatePendingNotificationTestExpression(user.GuidId);

            _mockNotificationRepository.Setup(r =>
                    r.GetAllAsync(
                        It.Is<Expression<Func<InAppNotification, bool>>>(
                            e => Lambda.ExpressionsEqual(e, testExpression))
                    ))
                .ReturnsAsync(pending.AsQueryable())
                .Verifiable(Times.Once);

            // Act
            var sut = CreateSut();
            var result = (await sut.GetPendingNotificationsAsync(user)).ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(pending.Length));
            Assert.That(
                result.Select(n => n.Id),
                Is.EquivalentTo(pending.Select(p => p.Id))
            );
            AssertAllMockVerifications();
        }

        [Test]
        public void GetPendingNotificationsAsyncGuidId_UserIdNotFound_ReturnsFailedTaskThrowingArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserService.Setup(s =>
                s.UserExistsAsync(userId.ToString()))
                .ReturnsAsync(false).Verifiable(Times.Once);

            // Act
            var sut = CreateSut();

            // Assert
            Assert.ThrowsAsync<ArgumentException>(() => sut.GetPendingNotificationsAsync(userId));
            AssertAllMockVerifications();
        }

        [Test]
        public void GetPendingNotificationsAsyncUserModel_UserNotFound_ReturnsFailedTaskThrowingArgumentException()
        {
            // Arrange
            var user = new ApplicationUser
            {
                GuidId = Guid.NewGuid()
            };
            var testExpression = CreatePendingNotificationTestExpression(user.GuidId);

            _mockNotificationRepository.Setup(r =>
                    r.GetAllAsync(
                        It.Is<Expression<Func<InAppNotification, bool>>>(
                            e => Lambda.ExpressionsEqual(e, testExpression))
                        ))
                .ThrowsAsync(new SqlExceptionBuilder().WithNumber(
                        (int)SqlErrorNumber.ForeignKeyConstraintViolation).Build())
                .Verifiable(Times.Once);

            // Act
            var sut = CreateSut();

            // Assert
            Assert.ThrowsAsync<ArgumentException>(() => sut.GetPendingNotificationsAsync(user));
            AssertAllMockVerifications();
        }

        #endregion
        #region GetAllNotificationsAsyncTests

        private static Expression<Func<Notification, bool>> CreateAllNotificationTestExpression(Guid userId)
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
                        It.Is<Expression<Func<InAppNotification, bool>>>(
                            e => Lambda.ExpressionsEqual(e, testExpression))
                    ))
                .ReturnsAsync(Enumerable.Empty<InAppNotification>().AsQueryable())
                .Verifiable(Times.Once);

            // Act
            var sut = CreateSut();
            var result = (await sut.GetAllNotificationsAsync(userToAssert.GuidId)).ToList();

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
                        It.Is<Expression<Func<InAppNotification, bool>>>(
                            e => Lambda.ExpressionsEqual(e, testExpression))
                    ))
                .ReturnsAsync(Enumerable.Empty<InAppNotification>().AsQueryable())
                .Verifiable(Times.Once);

            // Act
            var sut = CreateSut();
            var result = (await sut.GetAllNotificationsAsync(user)).ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
            AssertAllMockVerifications();
        }

        [Test]
        public async Task GetPendingNotificationsAsyncGuidId_HasNotifications_ReturnsTaskWithEnumerableOfNotifications()
        {
            // Arrange
            var userToAssert = new ApplicationUser
            {
                GuidId = Guid.NewGuid()
            };
            var all = new[]
            {
                NotificationValidValuesSource.GetValidInAppNotification(userToAssert.GuidId),
                NotificationValidValuesSource.GetValidInAppNotification(userToAssert.GuidId, true)
            };
            var testExpression = CreateAllNotificationTestExpression(userToAssert.GuidId);

            _mockUserService.Setup(s =>
                    s.GetUserByIdAsync(It.Is<Guid>(id => id == userToAssert.GuidId)))
                .ReturnsAsync(userToAssert).Verifiable(Times.Once);

            _mockNotificationRepository.Setup(r =>
                    r.GetAllAsync(
                        It.Is<Expression<Func<InAppNotification, bool>>>(
                            e => Lambda.ExpressionsEqual(e, testExpression))
                    ))
                .ReturnsAsync(all.AsQueryable())
                .Verifiable(Times.Once);

            // Act
            var sut = CreateSut();
            var result = (await sut.GetAllNotificationsAsync(userToAssert.GuidId)).ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(all.Length));
            Assert.That(
                result.Select(n => n.Id),
                Is.EquivalentTo(all.Select(a => a.Id))
            );
            AssertAllMockVerifications();
        }

        [Test]
        public async Task GetPendingNotificationsAsyncUserModel_HasNotifications_ReturnsTaskWithEnumerableOfNotifications()
        {
            // Arrange
            var user = new ApplicationUser
            {
                GuidId = Guid.NewGuid()
            };
            var all = new[]
            {
                NotificationValidValuesSource.GetValidInAppNotification(user.GuidId),
                NotificationValidValuesSource.GetValidInAppNotification(user.GuidId, true)
            };
            var testExpression = CreateAllNotificationTestExpression(user.GuidId);

            _mockNotificationRepository.Setup(r =>
                    r.GetAllAsync(
                        It.Is<Expression<Func<InAppNotification, bool>>>(
                            e => Lambda.ExpressionsEqual(e, testExpression))
                    ))
                .ReturnsAsync(all.AsQueryable())
                .Verifiable(Times.Once);

            // Act
            var sut = CreateSut();
            var result = (await sut.GetAllNotificationsAsync(user)).ToList();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(all.Length));
            Assert.That(
                result.Select(n => n.Id),
                Is.EquivalentTo(all.Select(a => a.Id))
            );
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

            // Act
            var sut = CreateSut();

            // Assert
            Assert.ThrowsAsync<ArgumentException>(() => sut.GetAllNotificationsAsync(userId));
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
                    It.Is<Expression<Func<InAppNotification, bool>>>(
                        e => Lambda.ExpressionsEqual(e, testExpression))
                ))
                .ThrowsAsync(new SqlExceptionBuilder().WithNumber(
                    (int)SqlErrorNumber.ForeignKeyConstraintViolation).Build())
                .Verifiable(Times.Once);

            // Act
            var sut = CreateSut();

            // Assert
            Assert.ThrowsAsync<ArgumentException>(() => sut.GetAllNotificationsAsync(user));
            AssertAllMockVerifications();
        }

        #endregion

    }
}
