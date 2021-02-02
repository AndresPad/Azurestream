using apa.BOL;
using apa.BOL.Config;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace azurestream.Services
{
    //--------------------------------------------------------------------------------------------------------------
    public class EmailSender : IEmailSender, ISmsSender
    {
        private IConfiguration Configuration { get; }
        private EmailConfig _emailConfig { get; }
        private SendGridConfig _sendgridConfig { get; }
        //----------------------------------------------------------------------------------------------------------
        public EmailSender(IConfiguration configuration, IOptions<EmailConfig> emailConfig, IOptions<SendGridConfig> sendgridConfig)
        {
            _emailConfig = emailConfig.Value;
            _sendgridConfig = sendgridConfig.Value;

            Configuration = configuration;
            _sendgridConfig.SendGridUser = Configuration["SendGrid:SendGridUser"];
            _sendgridConfig.SendGridKey = Configuration["SendGrid:SendGridKey"];
        }

        //----------------------------------------------------------------------------------------------------------
        public Task SendEmailAsync(string email, string subject, string message)
        {
            // Plug in your email service here to send an email.
            if (_emailConfig.EmailService == "SendGrid")
            {
                return Execute(_sendgridConfig.SendGridKey, email, subject, message);
            }
            else
            {
                return ExecuteAsync(email, subject, message);
            }
        }

        //SendGrid
        //----------------------------------------------------------------------------------------------------------
        public Task Execute(string apiKey, string email, string subject, string message)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_emailConfig.EmailMailFrom, _emailConfig.EmailMailSender),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = "<html><body><br />" + message + "<br/><br/><a href='http://www.azurestream.io'><img src='https://azurestream.blob.core.windows.net/images/azurestream.png' alt='azurestream' border='0' /></a></body></html>"
            };

            foreach (var address in email.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                msg.AddTo(new EmailAddress(address));
            }

            return client.SendEmailAsync(msg);
        }

        //MailKit
        //----------------------------------------------------------------------------------------------------------
        public async Task ExecuteAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_emailConfig.EmailMailSender, _emailConfig.EmailMailFrom));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new SmtpClient())
            {
                client.LocalDomain = _emailConfig.EmailMailSender;
                await client.ConnectAsync(_emailConfig.EmailMailHost, _emailConfig.EmailMailHostPort, SecureSocketOptions.None).ConfigureAwait(false);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }

        //----------------------------------------------------------------------------------------------------------
        public async Task<SendGridMessage> ContactUs(ContactUsViewModel model, IViewRender viewRenderService, List<String> recipients, String From)
        {
            SendGridMessage msg = new SendGridMessage()
            {
                From = new EmailAddress(From)
            };

            List<EmailAddress> addresses = recipients.Select(r => new EmailAddress() { Email = r }).ToList();

            msg.AddTos(addresses);
            msg.Subject = "You Have Mail From Azurestream Contact Form - From: " + model.Name;
            msg.HtmlContent = await viewRenderService.RenderToStringAsync("Mailer/mailcontactus", model);

            return msg;
        }

        //----------------------------------------------------------------------------------------------------------
        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
