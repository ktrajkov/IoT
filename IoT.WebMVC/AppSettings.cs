using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoT.WebMVC
{
    public class AppSettings
    {
        public string WSServerUrl { get; set; }

        public string Broker { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Key { get; set; }
        public string Feeds { get; set; }
    }
}
