using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Lungisa.Services
{
    // Helper class for sending emails via SMTP
    public class EmailHelper
    {
        private readonly string _fromEmail;      // Sender email address
        private readonly string _fromPassword;   // Sender email password or app-specific password
        private readonly string _host;           // SMTP server host (e.g., smtp.gmail.com)
        private readonly int _port;              // SMTP port (e.g., 587 for TLS)

        // Constructor: reads SMTP configuration from appsettings.json
        public EmailHelper(IConfiguration configuration)
        {
            _fromEmail = configuration["SMTP:Email"];
            _fromPassword = configuration["SMTP:Password"];
            _host = configuration["SMTP:Host"];
            _port = int.Parse(configuration["SMTP:Port"]); // Convert port to integer
        }

        // Send an email asynchronously
        public async Task SendEmailAsync(string toEmail, string toName, string subject, string body)
        {
            // Configure SMTP client
            using (var smtp = new SmtpClient
            {
                Host = _host,                                // SMTP server
                Port = _port,                                // SMTP port
                EnableSsl = true,                            // Enable SSL/TLS encryption
                DeliveryMethod = SmtpDeliveryMethod.Network, // Send via network
                Credentials = new NetworkCredential(_fromEmail, _fromPassword), // Authentication
                Timeout = 20000                              // Timeout in milliseconds (20s)
            })
            // Create the email message
            using (var message = new MailMessage(
                new MailAddress(_fromEmail, "Lungisa NPO"), // Sender info
                new MailAddress(toEmail, toName))          // Recipient info
            {
                Subject = subject, // Email subject
                Body = body        // Email body
            })
            {
                // Send the email asynchronously
                await smtp.SendMailAsync(message);
            }
        }
    }
}