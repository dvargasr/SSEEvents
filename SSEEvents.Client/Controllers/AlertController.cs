using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using SSEEvents.Client.Services;

namespace SSEEvents.Client.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlertController : Controller
    {
        private readonly ILogger<AlertController> _logger;
        private readonly IConfiguration _configuration;
        public ProcessMonitorService _processService { get; set; }

        public AlertController(ILogger<AlertController> logger, IConfiguration configuration, ProcessMonitorService processService)
        {
            _logger = logger;
            _configuration = configuration;
            _processService = processService;
        }

        /// <summary>
        /// This GET method returns an event stream with AlertInfo objects
        /// </summary>
        /// <returns>A stream with events</returns>
        [HttpGet]
        public async Task Get()
        {
            Response.Headers.Add("Content-Type", "text/event-stream");

            //wait before checking for alerts
            await Task.Delay(TimeSpan.FromSeconds(_configuration.GetValue<int>("Intervals:AlertCheckInterval")));
            
            //traverse the queue until all alerts are sent to client
            while (_processService._alertsQueue.Count > 0)
            {
                var alertInfo = _processService._alertsQueue.Dequeue();
                //serialize alertinfo object as json object
                var jsonObj = JsonConvert.SerializeObject(alertInfo);
                var msgJson = $"data:{jsonObj}\n\n";
                byte[] dataBytes = ASCIIEncoding.ASCII.GetBytes(msgJson);
                await Response.Body.WriteAsync(dataBytes, 0, dataBytes.Length);
                await Response.Body.FlushAsync();
            }
        }
    }
}
