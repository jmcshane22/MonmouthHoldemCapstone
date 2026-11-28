using System;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MH.Capstone.Domain.Services.Abstraction;

namespace MH.Capstone.Domain.Services
{
    // Provider-neutral sender over SMTP + STARTTLS. Used with Amazon SES's SMTP interface
    // (email-smtp.<region>.amazonaws.com:587), but any SMTP relay works by changing config.
    public class SmtpEmailService : IEmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly NetworkCredential _credentials;
        private readonly ILogger<SmtpEmailService> _logger;

        public string SenderAddress { get; }

        public SmtpEmailService(string host, int port, string username, string password, string senderAddress, ILogger<SmtpEmailService> logger)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("SMTP host is required.", nameof(host));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("SMTP username is required.", nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("SMTP password is required.", nameof(password));
            if (string.IsNullOrWhiteSpace(senderAddress)) throw new ArgumentException("Sender address is required.", nameof(senderAddress));

            _host = host;
            _port = port;
            _credentials = new NetworkCredential(username, password);
            SenderAddress = senderAddress;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendAsync(string to, string subject, string htmlBody, string? plainText = null, CancellationToken cancellationToken = default)
        {
            using var message = BuildMessage(SenderAddress, to, subject, htmlBody, plainText);
            // SmtpClient isn't thread-safe; one per send is fine at this app's volume.
            using var client = new SmtpClient(_host, _port) { EnableSsl = true, Credentials = _credentials };

            try
            {
                await client.SendMailAsync(message, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Email sent via SMTP to {Recipient}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", to);
                throw;
            }
        }

        // Builds a multipart/alternative message: plain text first, HTML as the preferred view.
        public static MailMessage BuildMessage(string from, string to, string subject, string htmlBody, string? plainText)
        {
            if (string.IsNullOrWhiteSpace(to)) throw new ArgumentException("Recipient is required", nameof(to));
            if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("Subject is required", nameof(subject));

            var message = new MailMessage(from, to) { Subject = subject };
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(plainText ?? string.Empty, null, MediaTypeNames.Text.Plain));
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody ?? string.Empty, null, MediaTypeNames.Text.Html));
            return message;
        }
    }
}
