using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkedInAutomation.Core.Models;

namespace LinkedInAutomation.Core.Services
{
    public interface ITelegramService
    {
        Task<string> AskForTopic();
        Task<bool> RequestApproval(string content);
        Task SendMessage(string message);
        Task<TelegramResponse> WaitForResponse(TimeSpan? timeout = null);
        Task SendErrorNotification(string error);
        bool IsUserAuthorized(long chatId);
    }
}
