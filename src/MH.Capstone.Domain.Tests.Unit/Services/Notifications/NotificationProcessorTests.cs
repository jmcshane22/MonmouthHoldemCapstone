using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MH.Capstone.Domain.Constants;
using MH.Capstone.Domain.DataAccess;
using MH.Capstone.Domain.DataAccess.Repositories;
using MH.Capstone.Domain.DataModels;
using MH.Capstone.Domain.Services.Abstraction;
using MH.Capstone.Domain.Services.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace MH.Capstone.Domain.Tests.Unit.Services.Notifications
{
    [TestFixture]
    [Parallelizable]
    [ExcludeFromCodeCoverage]
    public class InAppNotificationProcessorTests
    {
        #region TestOverhead

        private InAppNotificationProcessor _sut;
        private Mock<IInAppNotificationService> _inAppServiceMock;
        private Mock<IRepository<InAppNotification, ApplicationDbContext>> _inAppRepoMock;

        [SetUp]
        public void SetUp()
        {
            _inAppServiceMock = new Mock<IInAppNotificationService>();
            _inAppRepoMock = new Mock<IRepository<InAppNotification, ApplicationDbContext>>();
            _sut = CreateSut();
        }

        private InAppNotificationProcessor CreateSut() =>
            new InAppNotificationProcessor(NullLogger<INotificationProcessor>.Instance,
                _inAppServiceMock.Object, _inAppRepoMock.Object);

        private void AssertAllMockSetupVerifications()
        {
            _inAppServiceMock.Verify();
            _inAppServiceMock.VerifyNoOtherCalls();

            _inAppRepoMock.Verify();
            _inAppRepoMock.VerifyNoOtherCalls();
        }

        #endregion

        #region ProcessPendingNotificationsAsyncTests



        #endregion
    }

    public class EmailNotificationProcessorTests
    {
        #region TestOverhead

        private EmailNotificationProcessor _sut;
        private Mock<IEmailNotificationService> _emailServiceMock;
        private Mock<IRepository<EmailNotification, ApplicationDbContext>> _emailRepoMock;

        [SetUp]
        public void SetUp()
        {
            _emailServiceMock = new Mock<IEmailNotificationService>();
            _emailRepoMock = new Mock<IRepository<EmailNotification, ApplicationDbContext>>();
            _sut = CreateSut();
        }

        private EmailNotificationProcessor CreateSut() => 
            new EmailNotificationProcessor(NullLogger<INotificationProcessor>.Instance, 
                _emailServiceMock.Object, _emailRepoMock.Object);

        private void AssertAllMockSetupVerifications()
        {
            _emailServiceMock.Verify();
            _emailServiceMock.VerifyNoOtherCalls();
            _emailRepoMock.Verify();
            _emailRepoMock.VerifyNoOtherCalls();
        }

        #endregion

        #region ProcessPendingNotificationsAsyncTests
        
        
        
        #endregion
    }
}