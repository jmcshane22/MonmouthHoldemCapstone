using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using MH.Capstone.Domain.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace MH.Capstone.Domain.Tests.Unit.Services;

[TestFixture]
[Parallelizable]
[ExcludeFromCodeCoverage]
public class SmtpEmailServiceTests
{
    [Test]
    public void BuildMessage_HasSenderRecipientSubjectAndBothBodies()
    {
        using var message = SmtpEmailService.BuildMessage("noreply@example.com", "user@example.com", "Hello", "<p>Hi</p>", "Hi");

        Assert.Multiple(() =>
        {
            Assert.That(message.From!.Address, Is.EqualTo("noreply@example.com"));
            Assert.That(message.To.Single().Address, Is.EqualTo("user@example.com"));
            Assert.That(message.Subject, Is.EqualTo("Hello"));
            Assert.That(message.AlternateViews.Select(v => v.ContentType.MediaType), Is.EqualTo(new[] { "text/plain", "text/html" }));
            Assert.That(new StreamReader(message.AlternateViews[1].ContentStream).ReadToEnd(), Is.EqualTo("<p>Hi</p>"));
        });
    }

    [TestCase("", "Subject")]
    [TestCase("user@example.com", "")]
    public void BuildMessage_MissingRecipientOrSubject_Throws(string to, string subject)
    {
        Assert.Throws<ArgumentException>(() => SmtpEmailService.BuildMessage("noreply@example.com", to, subject, "<p>x</p>", null));
    }

    [TestCase("", "user", "pass", "noreply@example.com")]
    [TestCase("smtp.example.com", "", "pass", "noreply@example.com")]
    [TestCase("smtp.example.com", "user", "", "noreply@example.com")]
    [TestCase("smtp.example.com", "user", "pass", "")]
    public void Constructor_MissingSetting_Throws(string host, string user, string pass, string sender)
    {
        Assert.Throws<ArgumentException>(() => new SmtpEmailService(host, 587, user, pass, sender, NullLogger<SmtpEmailService>.Instance));
    }
}
