namespace Task_Management_System.ExternalServices.EmailService
{
    public interface IEmailService
    {
        Task<string> SendEmail(string subject, string content, string receiver);
    }
}
