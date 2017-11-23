using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace IoT.WebMVC.Controllers
{
    public class FirmwareController : Controller
    {
        public IActionResult Update(string clientId, string currentVersion)
        {           
            var path = @"C:\Projects\IoT\IoT.Controller\ESP8266\Debug\ESP8266.bin";            
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            var contentType = "APPLICATION/octet-stream";
            var fileName = "ESP8266.bin";
            return File(fileBytes, contentType, fileName);            
        }
    }
}