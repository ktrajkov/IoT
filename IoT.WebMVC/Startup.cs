using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IoT.WebMVC.IoTHandler;
using IoT.CM;
using System.Text;
using System.Collections;
using System.Reflection;
using IoT.Broker;
using IoT.CM.WS;
using IoT.CM.Managers;
using IoT.CM.Settings;

namespace IoT.WebMVC
{
    public static class EnvVariable
    {
        //   public static List<>
    }


    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddWebSocketManager();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<CMSettings>(Configuration.GetSection("ClientSettings"));

            services.AddSingleton<WebSocketConnectionManager>();
            services.AddSingleton<ClientConnectionManager>();
            services.AddSingleton<ClientHandler>();
            services.AddSingleton<BaseClientManager, AdafruitClientManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();         
            app.UseWebSockets();

            app.MapWebSocketManager("/iot", serviceProvider.GetService<IoTMessageHandler>());
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}
