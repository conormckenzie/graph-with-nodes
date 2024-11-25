using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using DebugUtils;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine.GraphAccess;

namespace ReasoningEngine
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
    }

    public class CommandRequest
    {
        public string Payload { get; set; } = string.Empty;
    }

    public class WebServer
    {
        private readonly CommandProcessor commandProcessor;
        private static readonly string[] SupportedCommands = new[]
        {
            "node_query",
            "outgoing_edge_query", 
            "incoming_edge_query",
            "add_node",
            "delete_node",
            "edit_node",
            "add_edge",
            "delete_edge",
            "edit_edge"
        };

        private static readonly Dictionary<string, string> CommandDescriptions = new()
        {
            ["node_query"] = "Retrieves detailed node information by its ID.",
            ["outgoing_edge_query"] = "Lists all edges where this node is the source.",
            ["incoming_edge_query"] = "Lists all edges where this node is the destination.",
            ["add_node"] = "Creates a new node with specified content.",
            ["delete_node"] = "Permanently removes a node and all its edges.",
            ["edit_node"] = "Updates an existing node's content.",
            ["add_edge"] = "Creates a directed edge between two nodes.",
            ["delete_edge"] = "Removes a directed edge between two nodes.",
            ["edit_edge"] = "Updates an existing edge's properties."
        };

        public WebServer(CommandProcessor commandProcessor)
        {
            this.commandProcessor = commandProcessor;
        }

        public void Start()
        {
            var builder = WebApplication.CreateBuilder();

            // Enable services
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Reasoning Engine API",
                    Version = "v1",
                    Description = "API for managing the Reasoning Engine graph database.",
                    Contact = new OpenApiContact
                    {
                        Name = "Development Team",
                        Email = "conor.mckenzie314@protonmail.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "CC BY-NC 4.0",
                        Url = new Uri("http://creativecommons.org/licenses/by-nc/4.0/")
                    }
                });
            });

            var app = builder.Build();

            // Configure middleware
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reasoning Engine API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();
            app.UseAuthorization();

            // Error handling middleware
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    DebugWriter.DebugWriteLine("#WEB500#", $"Internal server error: {ex.Message}");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    
                    var response = new ApiResponse<object>
                    {
                        Success = false,
                        Error = "An internal server error occurred"
                    };
                    
                    await context.Response.WriteAsJsonAsync(response);
                }
            });

            // List available commands
            app.MapGet("/api/commands", () =>
            {
                DebugWriter.DebugWriteLine("#CMD000#", "Listing available commands");
                return new ApiResponse<string[]> { Success = true, Data = SupportedCommands };
            })
            .WithMetadata(new SwaggerOperationAttribute("List Commands", "Lists all available API commands and operations"));

            // Generic command endpoint (legacy support)
            app.MapPost("/api/commands/{command}", async (string command, [FromBody] CommandRequest request) =>
            {
                if (!SupportedCommands.Contains(command))
                {
                    return Results.BadRequest(new ApiResponse<string> 
                    { 
                        Success = false, 
                        Error = $"Unsupported command. Supported commands are: {string.Join(", ", SupportedCommands)}" 
                    });
                }

                DebugWriter.DebugWriteLine("#CMD001#", $"Processing command: {command}, payload: {request.Payload}");
                var result = await Task.FromResult(commandProcessor.ProcessCommand(command, request.Payload));
                return Results.Ok(new ApiResponse<string> { Success = true, Data = result });
            })
            .WithMetadata(new SwaggerOperationAttribute("Execute Generic Command", 
                "Legacy endpoint that supports all commands. Prefer using the specific endpoints below."));

            // RESTful endpoints
            // Node operations
            app.MapGet("/api/nodes/{id}/get", (long id) => 
                ProcessCommand("node_query", id.ToString()))
                .WithMetadata(new SwaggerOperationAttribute("Get Node", 
                    "Retrieves detailed information about a specific node"));

            app.MapPost("/api/nodes/create", async ([FromBody] CommandRequest request) => 
                await ProcessCommand("add_node", request.Payload))
                .WithMetadata(new SwaggerOperationAttribute("Create Node", 
                    "Creates a new node. Payload format: \"nodeId|content\""));

            app.MapPut("/api/nodes/{id}/update", async (long id, [FromBody] CommandRequest request) => 
                await ProcessCommand("edit_node", $"{id}|{request.Payload}"))
                .WithMetadata(new SwaggerOperationAttribute("Update Node", 
                    "Updates an existing node's content"));

            app.MapDelete("/api/nodes/{id}/delete", (long id) => 
                ProcessCommand("delete_node", id.ToString()))
                .WithMetadata(new SwaggerOperationAttribute("Delete Node", 
                    "Permanently removes a node and all its associated edges"));

            // Edge operations
            app.MapGet("/api/nodes/{id}/edges/outgoing/list", (long id) => 
                ProcessCommand("outgoing_edge_query", id.ToString()))
                .WithMetadata(new SwaggerOperationAttribute("List Outgoing Edges", 
                    "Lists all edges where this node is the source"));

            app.MapGet("/api/nodes/{id}/edges/incoming/list", (long id) => 
                ProcessCommand("incoming_edge_query", id.ToString()))
                .WithMetadata(new SwaggerOperationAttribute("List Incoming Edges", 
                    "Lists all edges where this node is the destination"));

            app.MapPost("/api/edges/create", async ([FromBody] CommandRequest request) => 
                await ProcessCommand("add_edge", request.Payload))
                .WithMetadata(new SwaggerOperationAttribute("Create Edge", 
                    "Creates a directed edge. Payload format: \"sourceId|destId|weight|content\""));

            app.MapPut("/api/edges/update", async ([FromBody] CommandRequest request) => 
                await ProcessCommand("edit_edge", request.Payload))
                .WithMetadata(new SwaggerOperationAttribute("Update Edge", 
                    "Updates an existing edge. Payload format: \"sourceId|destId|weight|content\""));

            app.MapDelete("/api/edges/delete", async ([FromBody] CommandRequest request) => 
                await ProcessCommand("delete_edge", request.Payload))
                .WithMetadata(new SwaggerOperationAttribute("Delete Edge", 
                    "Removes an edge. Payload format: \"sourceId|destId\""));

            // Health check endpoint
            app.MapGet("/api/health", () =>
            {
                DebugWriter.DebugWriteLine("#HC001#", "Health check requested");
                return Results.Ok(new ApiResponse<string> { Success = true, Data = "OK" });
            })
            .WithMetadata(new SwaggerOperationAttribute("Health Check", "Returns OK if the service is healthy"));

            DebugWriter.DebugWriteLine("#WEB001#", "Starting web server on http://localhost:5000");
            app.Run("http://localhost:5000");
        }

        private async Task<IResult> ProcessCommand(string command, string payload)
        {
            try
            {
                var result = await Task.FromResult(commandProcessor.ProcessCommand(command, payload));
                return Results.Ok(new ApiResponse<string> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new ApiResponse<string> { Success = false, Error = ex.Message });
            }
        }
    }
}