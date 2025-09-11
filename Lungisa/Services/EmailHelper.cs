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
        // Sender email address
        private readonly string _fromEmail;
        // Sender email password or app-specific password
        private readonly string _fromPassword;
        // SMTP server host (e.g., smtp.gmail.com)
        private readonly string _host;
        // SMTP port (e.g., 587 for TLS)
        private readonly int _port;              

        // Constructor: reads SMTP configuration from appsettings.json
        public EmailHelper(IConfiguration configuration)
        {
            _fromEmail = configuration["SMTP:Email"];
            _fromPassword = configuration["SMTP:Password"];
            _host = configuration["SMTP:Host"];
            // Convert port to integer
            _port = int.Parse(configuration["SMTP:Port"]); 
        }

        // Send an email asynchronously
        public async Task SendEmailAsync(string toEmail, string toName, string subject, string body)
        {
            // Configure SMTP client
            using (var smtp = new SmtpClient
            {
                // SMTP server
                Host = _host,
                // SMTP port
                Port = _port,
                // Enable SSL/TLS encryption
                EnableSsl = true,
                // Send via network
                DeliveryMethod = SmtpDeliveryMethod.Network,
                // Authentication
                Credentials = new NetworkCredential(_fromEmail, _fromPassword),
                // Timeout in milliseconds (20s)
                Timeout = 20000                              
            })
            // Create the email message
            using (var message = new MailMessage(
                // Sender info
                new MailAddress(_fromEmail, "Lungisa NPO"),
                // Recipient info
                new MailAddress(toEmail, toName))          
            {
                // Email subject
                Subject = subject,
                // Email body
                Body = body        
            })
            {
                // Send the email asynchronously
                await smtp.SendMailAsync(message);
            }
        }
    }
}