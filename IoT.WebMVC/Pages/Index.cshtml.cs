using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Text;
using System.Collections;

namespace IoT.WebMVC.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppSettings _settings;
        private string _accessToken = "?access_token=Kalin";

        public string WSServerUrl { get; set; }
        public string EnvironmentName { get; set; }

        public IndexModel(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }        

        public void OnGet()
        {
            WSServerUrl = _settings.WSServerUrl + _accessToken;          
        }
    }
}
