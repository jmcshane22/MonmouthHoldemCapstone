using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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
    public class EmailNotificationServiceTests : NotificationServiceBaseTests<EmailNotificationService, EmailNotification>
    {
        #region TestOverhead

        protected override EmailNotificationService CreateSut() =>
            new EmailNotificationService(_mockNotificationRepository.Object, 
                _mockUserService.Object, NullLogger<INotificationService<EmailNotification>>.Instance);

        #endregion

        #region SendNotificationAsyncTests
        


        #endregion
        
        #region GetUnsentEmailNotificationsAsyncTests

        private static Expression<Func<EmailNotification, bool>> UnsentEmailNotificationsExpression =>
            n => n.DeliveredAt == null && n.SentAt.ToUniversalTime() < DateTimeOffset.UtcNow &&
                 n.FailureCount < 3 && (n.LastFailure == null 
                                        || n.LastFailure.Value.ToUniversalTime()
                                            .AddMinutes(n.FailureCount * 5) < DateTimeOffset.UtcNow);

        [Test]
        public async Task
            GetUnsentEmailNotificationsAsync_UnsetEmailNotificationsExists_ReturnsEnumerableOfEmailNotifications()
        {
            // Arrange
            var expected = new List<EmailNotification>
            {
                NotificationValidValuesSource.GetValidEmailNotification(),
                NotificationValidValuesSource.GetValidEmailNotification(),
                NotificationValidValuesSource.GetValidEmailNotification()
            };
            var testExpression = UnsentEmailNotificationsExpression;

            _mockNotificationRepository.Setup(r => 
                r.GetAllAsync(
                    It.Is<Expression<Func<EmailNotification, bool>>>(
                        e => Lambda.ExpressionsEqual(e, testExpression))
                    ))
                .ReturnsAsync(expected.AsQueryable()).Verifiable(Times.Once);

            // Act
            var result = await _sut.GetUnsentEmailNotificationsAsync();

            // Assert
            Assert.That(result, Is.EquivalentTo(expected));
            AssertAllMockVerifications();
        }

        [Test]
        public async Task GetUnsentEmailNotificationsAsync_NoUnsetEmailNotificationsExists_ReturnsEmptyEnumerable()
        {
            // Arrange
            var expected = Array.Empty<EmailNotification>();
            var testExpression = UnsentEmailNotificationsExpression;

            _mockNotificationRepository.Setup(r =>
                    r.GetAllAsync(
                        It.Is<Expression<Func<EmailNotification, bool>>>(
                            e => Lambda.ExpressionsEqual(e, testExpression))
                    ))
                .ReturnsAsync(expected.AsQueryable()).Verifiable(Times.Once);



            // Act
            var result = await _sut.GetUnsentEmailNotificationsAsync();

            // Assert
            Assert.That(result, Is.Empty);
            AssertAllMockVerifications();
        }

        #endregion
    }
}
