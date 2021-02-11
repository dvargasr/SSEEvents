using System;

namespace SSEEvents.Client.Models
{
    public class SystemInfo
    {
        public double TotalCPU { get; set; }
        public double TotalMemory { get; set; }
        public double CommittedMemory { get; set; }
        public double AvailableMemory { get; set; }
        public double MemoryLoadPercentage { get; set; }
    }
}
