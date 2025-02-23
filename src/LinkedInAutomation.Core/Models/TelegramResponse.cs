using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkedInAutomation.Core.Models
{
    public class TelegramResponse
    {
        public long MessageId { get; set; }
        public string? ResponseText { get; set; }
        public bool IsApproved { get; set; }
        public DateTime ReceivedAt { get; set; }
        public long ChatId { get; set; }
        public string? Username { get; set; }

    }
}
