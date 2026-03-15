using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MH.Capstone.Domain.DataAccess;
using MH.Capstone.Domain.DataAccess.Repositories;
using MH.Capstone.Domain.DataModels;
using MH.Capstone.Domain.Services.Abstraction;
using MH.Capstone.Domain.Services.Notifications;
using Moq;

namespace MH.Capstone.Domain.Tests.Unit.Services.Notifications
{
    [TestFixture]
    [Parallelizable]
    [ExcludeFromCodeCoverage]
    public class NotificationProcessorTests
    {
        #region TestOverhead

        private NotificationProcessor _sut;
        private Mock<IInAppNotificationService> _inAppServiceMock;
        private Mock<IEmailNotificationService> _emailServiceMock;
        private Mock<IRepository<InAppNotification, ApplicationDbContext>> _inAppRepoMock;
        private Mock<IRepository<EmailNotification, ApplicationDbContext>> _emailRepoMock;

        [SetUp]
        public void SetUp()
        {
            _inAppServiceMock = new Mock<IInAppNotificationService>();
            _emailServiceMock = new Mock<IEmailNotificationService>();
            _inAppRepoMock = new Mock<IRepository<InAppNotification, ApplicationDbContext>>();
            _emailRepoMock = new Mock<IRepository<EmailNotification, ApplicationDbContext>>();
            _sut = CreateSut();
        }

        private NotificationProcessor CreateSut() => throw new NotImplementedException();

        private void AssertAllMockSetupVerifications()
        {
            _inAppServiceMock.Verify();
            _inAppServiceMock.VerifyNoOtherCalls();

            _emailServiceMock.Verify();
            _emailServiceMock.VerifyNoOtherCalls();

            _inAppRepoMock.Verify();
            _inAppRepoMock.VerifyNoOtherCalls();

            _emailRepoMock.Verify();
            _emailRepoMock.VerifyNoOtherCalls();
        }

        #endregion

        #region ProcessPendingNotificationsAsyncTests



        #endregion

        #region EnqueueNotificationForProssessingAsyncTests



        #endregion
    }
}
