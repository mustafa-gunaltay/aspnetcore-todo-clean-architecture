using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoBackend.Application.BackgroundJobs;

public interface IEmailSenderService
{
    Task SendTaskReminderAsync(string toEmail, List<string> taskTitles);
    Task<bool> IsEmailServiceAvailableAsync();
}
