using DocumentFormat.OpenXml.Vml;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LibraryProjectRepository.Repositories.Users
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        public SmtpEmailSender(IConfiguration config) => _config = config;

        public async Task SendVerificationEmailAsync(string toEmail, string code)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_config["Smtp:From"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "تأكيد البريد الإلكتروني";
            message.Body = new TextPart("html")
            {
                Text = $@"
                <p>شكراً لتسجيلك. رمز التحقق الخاص بك:</p>
                <h2>{code}</h2>
                <p>رمز التحقق صالح لمدة 15 دقيقة.</p>"
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), bool.Parse(_config["Smtp:UseSsl"]));
            await client.AuthenticateAsync(_config["Smtp:User"], _config["Smtp:Pass"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
    public interface IEmailSender
    {
        Task SendVerificationEmailAsync(string toEmail, string code);
    }
    
}
