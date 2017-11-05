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

        public string WSServerUrl { get; set; }
        public string EnvironmentName { get; set; }

        public IndexModel(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
        }        

        public void OnGet()
        {
            WSServerUrl = _settings.WSServerUrl;
            var envVariable = Environment.GetEnvironmentVariables();
            StringBuilder v = new StringBuilder();
            foreach (DictionaryEntry item in envVariable)
            {
                v.AppendFormat("key: {0} - value: {1}", item.Key, item.Value);
            }
            EnvironmentName = v.ToString();
        }
    }
}
