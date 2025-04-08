using Quartz;
using System.Text;
using Task_Management_System.ExternalServices.EmailService;
using Task_Management_System.Services.TaskService;

namespace Task_Management_System.Services.BackgroundJobs
{
    public class SendReminderJobs : IJob
    {
        private readonly IEmailService _emailService;
        private readonly ITaskService task;

        public SendReminderJobs(IEmailService emailService, ITaskService issueService)
        {
            _emailService = emailService;
            task = issueService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var retrievedList = await task.IssueDeadlineList();

            var groupIssueByUser = retrievedList.GroupBy(user => user.User);

            foreach (var userIssues in groupIssueByUser)
            {
                string email = userIssues.Key;

                var issueList = userIssues.Select(issue => $"- {issue.Name} (Deadline: {issue.EndDate})").ToList();

                StringBuilder emailBody = new StringBuilder();
                emailBody.AppendLine("Hello,");
                emailBody.AppendLine("You have the following issue(s) nearing their deadline:");
                emailBody.AppendLine();
                emailBody.AppendLine(string.Join("\n", issueList));
                emailBody.AppendLine();
                emailBody.AppendLine("Please take necessary action.");

                string subject = "Issue Reminder Deadline";
                await _emailService.SendEmail(subject, emailBody.ToString(), email);
            }
            await Task.CompletedTask;
        }
    }
}
