using Microsoft.Extensions.Configuration;
using SSEEvents.Client.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Threading;

namespace SSEEvents.Client.Services
{
    public class ProcessMonitorService
    {
        public Queue<AlertInfo> _alertsQueue { get; set; }
        public IConfiguration _configuration { get; set; }

        public ProcessMonitorService(IConfiguration configuration)
        {
            _configuration = configuration;
            _alertsQueue = new Queue<AlertInfo>();
        }

        public SystemInfo GetSystemInfo()
        {
            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
            var totalMemory = new PerformanceCounter("Memory", "Available MBytes", null);
            var memoryInUse = new PerformanceCounter("Memory", "Committed Bytes", null);
            var query = "SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem";
            var searcher = new ManagementObjectSearcher(query);
            var sysInfo = new SystemInfo();

            //CPU calculations
            sysInfo.TotalCPU = cpuCounter.NextValue();
            Thread.Sleep(1000);
            // call .NextValue() twice if first read is 0.00
            if (Math.Abs(sysInfo.TotalCPU) <= 0.00)
                sysInfo.TotalCPU = cpuCounter.NextValue();

            //if threshold is reached, put CPU load alert in alerts queue
            if (sysInfo.TotalCPU > _configuration.GetValue<double>("Thresholds:CPU"))
            {
                _alertsQueue.Enqueue(new AlertInfo
                {
                    AlertMessage = _configuration.GetValue<string>("Alerts:CPUAlertMessage"),
                    AlertDate = DateTime.Now
                }) ;
            }

            //Memory calculations
            //converting from megabytes to gigabytes
            sysInfo.AvailableMemory = ConvertMBToGB(totalMemory.NextValue());
            //converting from bytes to gigabytes
            sysInfo.CommittedMemory = ConvertBytesToGB(memoryInUse.NextValue());
            //total memory in the system
            foreach (ManagementObject managementObject in searcher.Get())
            {
                //convert kilobytes to gigabytes
                sysInfo.TotalMemory = ConvertKBToGB((UInt64)managementObject.Properties["TotalVisibleMemorySize"].Value);
            }

            //calculate memory load percentage
            sysInfo.MemoryLoadPercentage = CalculateMemoryUtilizationPercentage(sysInfo.AvailableMemory, sysInfo.TotalMemory);

            //if threshold is reached, put memory load alert in alerts queue
            if (sysInfo.MemoryLoadPercentage > _configuration.GetValue<double>("Thresholds:Memory"))
            {
                _alertsQueue.Enqueue(new AlertInfo
                {
                    AlertMessage = _configuration.GetValue<string>("Alerts:MemoryAlertMessage"),
                    AlertDate = DateTime.Now
                });
            }

            return sysInfo;
        }

        public double GetCPUUtilizationForProcess(string processName)
        {
            var cpuCounter = new PerformanceCounter("Process", "% Processor Time", processName);
            var processCPUUtilization = cpuCounter.NextValue();

            // call .NextValue() twice if first read is 0.00
            if (Math.Abs(processCPUUtilization) <= 0.00)
            {
                //wait 200 milliseconds to make calculation more accurate
                Thread.Sleep(200);
                processCPUUtilization = cpuCounter.NextValue();
            }

            return processCPUUtilization;
        }

        public double CalculateMemoryUtilizationPercentage(double availableMemory, double totalMemory)
        { 
            return (100 * (totalMemory - availableMemory)) / totalMemory;
        }

        public double ConvertBytesToGB(double bytes)
        {
            return bytes / (1024 * 1024 * 1024);
        }

        public double ConvertKBToGB(UInt64 kilobytes)
        {
            return kilobytes / (double)(1024 * 1024);
        }

        public double ConvertMBToGB(double megabytes)
        {
            return megabytes / 1024;
        }

        public double ConvertBytesToMB(double bytes)
        {
            return bytes / (1024*1024);
        }
    }
}
