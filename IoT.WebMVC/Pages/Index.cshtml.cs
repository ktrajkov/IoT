using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace IoT.WebMVC.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppSettings _settings;

        public string WSServerUrl { get; set; }

        public IndexModel(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }        

        public void OnGet()
        {
            WSServerUrl = _settings.WSServerUrl;
        }
    }
}
