using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Lungisa.Services
{
    public class EmailHelper
    {
        private readonly string _fromEmail;
        private readonly string _fromPassword;
        private readonly string _host;
        private readonly int _port;

        public EmailHelper(IConfiguration configuration)
        {
            _fromEmail = configuration["SMTP:Email"];
            _fromPassword = configuration["SMTP:Password"];
            _host = configuration["SMTP:Host"];
            _port = int.Parse(configuration["SMTP:Port"]);
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string body)
        {
            using (var smtp = new SmtpClient
            {
                Host = _host,
                Port = _port,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(_fromEmail, _fromPassword),
                Timeout = 20000
            })
            using (var message = new MailMessage(new MailAddress(_fromEmail, "Lungisa NPO"),
                                                 new MailAddress(toEmail, toName))
            {
                Subject = subject,
                Body = body
            })
            {
                await smtp.SendMailAsync(message);
            }
        }
    }
}
