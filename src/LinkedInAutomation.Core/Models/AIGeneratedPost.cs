using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkedInAutomation.Core.Models
{
    public class AIGeneratedPost
    {
        public string? Topic { get; set; }
        public string? Content { get; set; }
        public DateTime GeneratedAt { get; set; }
        public required string[] Hashtags { get; set; }
        public string? ModelUsed { get; set; }
        public bool IsApproved { get; set; }
        public string? GenerationPrompt { get; set; }
    }
}
