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
                    app.UseRouting();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/api/command/{command}/{payload}", async context =>
                        {
                            try 
                            {
                                var command = context.Request.RouteValues["command"]?.ToString();
                                var payload = context.Request.RouteValues["payload"]?.ToString();

                                if (string.IsNullOrEmpty(command))
                                {
                                    context.Response.StatusCode = 400;
                                    await context.Response.WriteAsync("Command is required");
                                    return;
                                }

                                if (payload == null)
                                {
                                    context.Response.StatusCode = 400;
                                    await context.Response.WriteAsync("Payload is required");
                                    return;
                                }

                                var result = await commandProcessor.ProcessCommandAsync(command, payload);
                                await context.Response.WriteAsync(result);
                            }
                            catch (Exception ex)
                            {
                                context.Response.StatusCode = 500;
                                await context.Response.WriteAsync($"Internal server error: {ex.Message}");
                            }
                        });

                        endpoints.MapGet("/api/health", async context =>
                        {
                            await context.Response.WriteAsync("OK");
                        });
                    });
                })
                .UseUrls("http://localhost:5000")
                .Build();

            DebugWriter.DebugWriteLine("#WEB001#", "Starting web server on http://localhost:5000");
            host.Run();
        }
    }
}