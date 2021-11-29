using System;

namespace Hosts.Repository.Models
{
    public class Alert : BaseModel
    {
        public string Contents { get; set; }

        public DateTime CreationTime { get; set; }

        public bool HasReported { get; set; }

        public DateTime ReportedTime { get; set; }
    }
}
