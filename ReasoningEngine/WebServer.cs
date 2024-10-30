using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using DotNetEnv;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine.GraphAccess;
using DebugUtils;

namespace ReasoningEngine
{
    public class WebServer
    {
        private readonly CommandProcessor commandProcessor;

        public WebServer(CommandProcessor commandProcessor)
        {
            this.commandProcessor = commandProcessor;
        }

        public void Start()
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .ConfigureServices(services => services.AddRouting())
                .Configure(app =>
                {
                    app.UseRouting(); // Enable routing

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/api/command/{command}/{payload}", async context =>
                        {
                            var command = context.Request.RouteValues["command"]?.ToString();
                            var payload = context.Request.RouteValues["payload"]?.ToString();
                            var result = await commandProcessor.ProcessCommandAsync(command, payload);
                            await context.Response.WriteAsync(result);
                        });
                    });
                })
                .UseUrls("http://localhost:5000")
                .Build();

            host.Run();
        }
    }
}
