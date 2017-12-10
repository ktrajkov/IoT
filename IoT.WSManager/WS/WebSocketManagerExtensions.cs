using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using IoT.CM.Managers;

namespace IoT.CM.WS
{
    public static class WebSocketManagerExtensions
    {
        public static IServiceCollection AddWebSocketManager(this IServiceCollection services)
        {
            foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
            {
                if (type.GetTypeInfo().BaseType == typeof(WebSocketHandler))
                {
                    services.AddSingleton(type);
                }
            }

            return services;
        }

        public static IApplicationBuilder MapWebSocketManager(this IApplicationBuilder app,
                                                              PathString path,
                                                              WebSocketHandler handler)
        {
            return app.Map(path, (_app) => _app.UseMiddleware<WebSocketManagerMiddleware>(handler));
        }
    }
}
