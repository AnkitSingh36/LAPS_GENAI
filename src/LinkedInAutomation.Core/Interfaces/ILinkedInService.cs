using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkedInAutomation.Core.Services
{
    public interface ILinkedInService
    {
        Task Initialize();
        Task<bool> PostContent(string content);
        Task<bool> IsAuthenticated();
        Task<string> GetUserProfile();
        Task<bool> AddHashtags(string[] hashtags);
        Task<string> PreviewPost(string content);
        Task<bool> SchedulePost(string content, DateTime scheduledGTime);

    }
}
