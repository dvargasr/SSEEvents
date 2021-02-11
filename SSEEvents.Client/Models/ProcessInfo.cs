using System;

namespace SSEEvents.Client.Models
{
    public class ProcessInfo
    {
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public double Memory { get; set; }
        public double CPU { get; set; }
    }
}
