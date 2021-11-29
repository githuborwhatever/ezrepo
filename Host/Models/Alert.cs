using System;

namespace Hosts.ShopBot.Models
{
    public class Alert
    {
        public string Contents { get; set; }

        public DateTime CreationTime { get; set; }

        public bool HasReported { get; set; }

        public DateTime ReportedTime { get; set; }
    }
}
