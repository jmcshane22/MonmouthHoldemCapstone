using MH.Capstone.Domain.DataAccess;
using MH.Capstone.Domain.DataAccess.Repositories;
using MH.Capstone.Domain.DataModels;
using MH.Capstone.Domain.Services.Abstraction;
using MH.Capstone.Domain.Services.Notifications;
using Microsoft.EntityFrameworkCore;
using Moq;
using Neleus.LambdaCompare;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        [SetUp]
        public void SetUp()
        {
            _mockNotificationRepository =
                new Mock<IRepository<TNotifType, ApplicationDbContext>>();
            _mockUserService = new Mock<IUserService>();
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
    }

    public struct NotificationValidValuesSource
    {
        public static Notification GetValidNotification(Guid? recipientId = null) =>
            new Notification(Guid.NewGuid(), recipientId ?? Guid.NewGuid(),
                GetRandomStringOfLength(GetRandomIntInRange(1, 50)),
                GetRandomStringOfLength(GetRandomIntInRange(1, 250)),
                DateTimeOffset.UtcNow);

        public static InAppNotification GetValidInAppNotification(Guid? recipientId = null,
            bool isRead = false) => new InAppNotification(GetValidNotification())
        {
            IsRead = isRead
        };
    }
}