using System.Net.Mail;
using System.Net;
using Task_Management_System.Configurations;

namespace Task_Management_System.ExternalServices.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> SendEmail(string subject, string content, string receiver)
        {
            EmailSettings settings = new EmailSettings();

            _configuration.GetSection("MailCredientials").Bind(settings);

            var senderEmail = settings.Email;
            var senderPwd = settings.Password;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {

                Port = 587,
                EnableSsl = true,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(senderEmail, senderPwd),
                Timeout = 20000,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = subject,
                Body = content,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(receiver);

            smtpClient.Send(mailMessage);

            return "Successfully";
        }
    }
}
