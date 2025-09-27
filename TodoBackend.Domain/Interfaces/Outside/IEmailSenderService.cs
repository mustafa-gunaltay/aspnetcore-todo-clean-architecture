using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoBackend.Domain.Interfaces.Outside;

public interface IEmailSenderService
{
    Task SendTaskReminderAsync(string toEmail, List<string> taskTitles);
    Task<bool> IsEmailServiceAvailableAsync();
}
