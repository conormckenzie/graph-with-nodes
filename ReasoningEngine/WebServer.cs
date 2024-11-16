using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
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
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.MapPost("/api/node_query", async (NodeIdPayload payload) => 
            {
                DebugWriter.DebugWriteLine("#NQ001#", $"Received request to query node with ID: {payload.Id}");
                var result = await Task.FromResult(commandProcessor.ProcessCommand("node_query", payload.Id.ToString()));
                DebugWriter.DebugWriteLine("#NQ002#", $"Result of node query command: {result}");
                return result;
            });

            app.MapPost("/api/outgoing_edge_query", async (NodeIdPayload payload) => 
            {
                DebugWriter.DebugWriteLine("#OEQ001#", $"Received request to query outgoing edges for node ID: {payload.Id}");
                var result = await Task.FromResult(commandProcessor.ProcessCommand("outgoing_edge_query", payload.Id.ToString()));
                DebugWriter.DebugWriteLine("#OEQ002#", $"Result of outgoing edge query command: {result}");
                return result;
            });

            app.MapPost("/api/incoming_edge_query", async (NodeIdPayload payload) => 
            {
                DebugWriter.DebugWriteLine("#IEQ001#", $"Received request to query incoming edges for node ID: {payload.Id}");
                var result = await Task.FromResult(commandProcessor.ProcessCommand("incoming_edge_query", payload.Id.ToString()));
                DebugWriter.DebugWriteLine("#IEQ002#", $"Result of incoming edge query command: {result}");
                return result;
            });

            app.MapPost("/api/add_node", async (NodePayload payload) => 
            {
                DebugWriter.DebugWriteLine("#AN001#", $"Received request to add node with ID: {payload.Id} and Content: {payload.Content}");
                var result = await Task.FromResult(commandProcessor.ProcessCommand("add_node", $"{payload.Id.ToString()}|{payload.Content}"));
                DebugWriter.DebugWriteLine("#AN002#", $"Result of add node command: {result}");
                return result;
            });

            app.MapPost("/api/delete_node", async (NodeIdPayload payload) => 
            {
                DebugWriter.DebugWriteLine("#DN001#", $"Received request to delete node with ID: {payload.Id}");
                var result = await Task.FromResult(commandProcessor.ProcessCommand("delete_node", payload.Id.ToString()));
                DebugWriter.DebugWriteLine("#DN002#", $"Result of delete node command: {result}");
                return result;
            });

            app.MapPost("/api/edit_node", async (NodePayload payload) => 
            {
                DebugWriter.DebugWriteLine("#EN001#", $"Received request to edit node with ID: {payload.Id} and Content: {payload.Content}");
                var result = await Task.FromResult(commandProcessor.ProcessCommand("edit_node", $"{payload.Id.ToString()}|{payload.Content}"));
                DebugWriter.DebugWriteLine("#EN002#", $"Result of edit node command: {result}");
                return result;
            });

            app.MapPost("/api/add_edge", async (EdgePayload payload) => 
            {
                DebugWriter.DebugWriteLine("#AE001#", $"Received request to add edge from Source ID: {payload.SourceId} to Dest ID: {payload.DestId} with Weight: {payload.Weight} and Content: {payload.Content}");
                var result = await Task.FromResult(commandProcessor.ProcessCommand("add_edge", $"{payload.SourceId.ToString()}|{payload.DestId.ToString()}|{payload.Weight}|{payload.Content}"));
                DebugWriter.DebugWriteLine("#AE002#", $"Result of add edge command: {result}");
                return result;
            });

            app.MapPost("/api/delete_edge", async (EdgeIdPayload payload) => 
            {
                DebugWriter.DebugWriteLine("#DE001#", $"Received request to delete edge from Source ID: {payload.SourceId} to Dest ID: {payload.DestId}");
                var result = await Task.FromResult(commandProcessor.ProcessCommand("delete_edge", $"{payload.SourceId.ToString()}|{payload.DestId.ToString()}"));
                DebugWriter.DebugWriteLine("#DE002#", $"Result of delete edge command: {result}");
                return result;
            });

            app.MapPost("/api/edit_edge", async (EdgePayload payload) => 
            {
                DebugWriter.DebugWriteLine("#EE001#", $"Received request to edit edge from Source ID: {payload.SourceId} to Dest ID: {payload.DestId} with Weight: {payload.Weight} and Content: {payload.Content}");
                var result = await Task.FromResult(commandProcessor.ProcessCommand("edit_edge", $"{payload.SourceId.ToString()}|{payload.DestId.ToString()}|{payload.Weight}|{payload.Content}"));
                DebugWriter.DebugWriteLine("#EE002#", $"Result of edit edge command: {result}");
                return result;
            });

            app.MapGet("/api/health", () => 
            {
                DebugWriter.DebugWriteLine("#HC001#", "Health check requested.");
                return Results.Ok(new { status = "Healthy" });
            });

            DebugWriter.DebugWriteLine("#WEB001#", "Starting web server on http://localhost:5000");
            OpenBrowser("http://localhost:5000");
            app.Run("http://localhost:5000");
        }

        private static void OpenBrowser(string url)
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

    public class NodeIdPayload
    {
        public int Id { get; set; }
    }

    public class NodePayload
    {
        public int Id { get; set; }
        public string Content { get; set; }
    }

    public class EdgePayload
    {
        public int SourceId { get; set; }
        public int DestId { get; set; }
        public string Weight { get; set; }
        public string Content { get; set; }
    }

    public class EdgeIdPayload
    {
        public int SourceId { get; set; }
        public int DestId { get; set; }
    }
}