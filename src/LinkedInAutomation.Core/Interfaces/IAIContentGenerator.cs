using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkedInAutomation.Core.Services
{
    public interface IAIContentGenerator
    {
        Task<string> GenerateContentAsync(string topic);
    }
}
