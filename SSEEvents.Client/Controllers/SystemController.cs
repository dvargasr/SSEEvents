using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
    public class SystemController : Controller
    {
        private readonly ILogger<SystemController> _logger;
        public IConfiguration _configuration { get; set; }
        public ProcessMonitorService _processService { get; set; }

        public SystemController(ILogger<SystemController> logger, IConfiguration configuration, ProcessMonitorService processService)
        {
            _logger = logger;
            _configuration = configuration;
            _processService = processService;
        }

        /// <summary>
        /// This GET method returns an event stream with a SystemInfo object
        /// </summary>
        /// <returns>A stream with events</returns>
        [HttpGet]
        public async Task Get()
        {
            SystemInfo sysInfo = _processService.GetSystemInfo();

            //serialize system info as a json object
            var jsonObj = JsonConvert.SerializeObject(sysInfo);

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
