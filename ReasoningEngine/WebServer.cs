using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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

                        endpoints.MapGet("/api/debug", async context =>
                        {
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "ReasoningEngine", "debug.html");
                            context.Response.ContentType = "text/html";
                            try
                            {
                                await context.Response.SendFileAsync(filePath);
                            }
                            catch (Exception ex)
                            {
                                context.Response.StatusCode = 500;
                                await context.Response.WriteAsync($"Error serving debug.html: {ex.Message}");
                            }
                        });
                    });
                })
                .UseUrls("http://localhost:5000")
                .Build();

            DebugWriter.DebugWriteLine("#WEB001#", "Starting web server on http://localhost:5000");
            OpenBrowser("http://localhost:5000/api/debug");
            host.Run();
        }

        private void OpenBrowser(string url)
        {
            try
            {
                ProcessStartInfo startInfo;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    startInfo = new ProcessStartInfo("cmd.exe", $"/c start {url}") { CreateNoWindow = true };
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    startInfo = new ProcessStartInfo("open", url) { CreateNoWindow = true };
                }
                else
                {
                    throw new NotSupportedException("Unsupported OS");
                }

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open browser. Please open the following URL manually: {url}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}