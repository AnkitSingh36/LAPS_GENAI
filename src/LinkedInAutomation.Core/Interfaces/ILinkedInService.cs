using System;
using System.Threading.Tasks;

namespace LinkedInAutomation.Core.Services
{
    public interface ILinkedinService
    {
        Task<bool> PostContentAsync(string content);
    }
}
