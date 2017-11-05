using IoT.WebMVC.IoTHandler;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoT.WebMVC.Controllers
{
    public class MessageController
    {
        public class MessagesController : Controller
        {
            private IoTMessageHandler _ioTMessageHandler { get; set; }

            public MessagesController(IoTMessageHandler ioTMessageHandler)
            {
                _ioTMessageHandler = ioTMessageHandler;
            }

            [HttpGet]
            public async Task SendMessage([FromQueryAttribute]string message)
            {
                await _ioTMessageHandler.InvokeClientMethodToAllAsync("receiveMessage", message);
            }
        }
    }
}
