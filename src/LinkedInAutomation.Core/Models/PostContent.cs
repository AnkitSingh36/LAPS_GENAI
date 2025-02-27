using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkedInAutomation.Core.Models
{
    public class PostContent
    {
        public required string Topic { get; set; }
        public required string Content { get; set; }
        public DateTime GeneratedAt { get; set; }

        public bool IsApproved { get; set; }
    }
}
