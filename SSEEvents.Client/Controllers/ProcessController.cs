using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SSEEvents.Client.Models;
using Microsoft.Extensions.Configuration;
using SSEEvents.Client.Services;

namespace SSEEvents.Client.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessController : Controller
    {
        private readonly ILogger<ProcessController> _logger;
        public IConfiguration _configuration { get; set; }
        public ProcessMonitorService _processMonitorService { get; set; }

        public ProcessController(ILogger<ProcessController> logger, IConfiguration configuration, ProcessMonitorService processMonitorService)
        {
            _logger = logger;
            _configuration = configuration;
            _processMonitorService = processMonitorService;
        }

        /// <summary>
        /// This GET method returns an event stream with a collection of ProcessInfo objects
        /// </summary>
        /// <returns>A stream with events</returns>
        [HttpGet]
        public async Task Get()
        {
            List<ProcessInfo> procInfoList = new List<ProcessInfo>();
            
            var procList = Process.GetProcesses()
                .Where(p => (long)p.MainWindowHandle != 0)
                .ToList();

            procList.ForEach(p => procInfoList.Add(new ProcessInfo
            {
                ProcessId = p.Id,
                ProcessName = p.ProcessName,
                //convert from bytes to MB
                Memory = _processMonitorService.ConvertBytesToMB(p.WorkingSet64),
                //if total processor time is 0 there is no need to do further calculations
                CPU = p.TotalProcessorTime > TimeSpan.Zero ? _processMonitorService.GetCPUUtilizationForProcess(p.ProcessName):0.0
            }));
            //serialize process list as json object
            var jsonObj = JsonConvert.SerializeObject(procInfoList);

            Response.Headers.Add("Content-Type", "text/event-stream");
            
            var msgJson = $"data:{jsonObj}\n\n";
            byte[] dataBytes = ASCIIEncoding.ASCII.GetBytes(msgJson);
            await Response.Body.WriteAsync(dataBytes, 0, dataBytes.Length);
            await Response.Body.FlushAsync();
            
            //wait before sending next set of data
            await Task.Delay(TimeSpan.FromSeconds(_configuration.GetValue<int>("Intervals:RefreshInterval")));
        }
    }
}
