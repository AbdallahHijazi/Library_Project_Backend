using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace LibraryProjectRepository.Repositories.Users
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly string apiKey;
        private readonly string fromEmail;
        public SendGridEmailSender(IConfiguration config)
        {
            apiKey = config["SendGrid:ApiKey"];
            fromEmail = config["SendGrid:From"];
        }

        public async Task SendVerificationEmailAsync(string toEmail, string code)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, "YourApp");
            var to = new EmailAddress(toEmail);
            var subject = "تأكيد البريد الإلكتروني";
            var html = $"<p>رمز التحقق: <strong>{code}</strong></p><p>صالح لمدة 15 دقيقة.</p>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", html);
            await client.SendEmailAsync(msg);
        }
    }
}
