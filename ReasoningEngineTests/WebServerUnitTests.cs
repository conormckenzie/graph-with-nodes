using NUnit.Framework;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using ReasoningEngine.GraphAccess;
using ReasoningEngine.GraphFileHandling;
using Microsoft.Extensions.DependencyInjection;

namespace ReasoningEngine.Tests
{
    [TestFixture]
    public class WebServerUnitTests
    {
        private TestServer? testServer;
        private HttpClient? client;
        private Mock<CommandProcessor>? mockCommandProcessor;

        [OneTimeSetUp]
        public void Setup()
        {
            // Create mock CommandProcessor directly
            mockCommandProcessor = new Mock<CommandProcessor>(new GraphFileManager("test-path"));

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddCors();
                    services.AddSingleton(mockCommandProcessor.Object);
                })
                .Configure(app =>
                {
                    app.UseCors(builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());

                    app.UseRouting();

                    app.UseEndpoints(endpoints =>
                    {
                        // Add a more general route that catches invalid formats
                        endpoints.MapGet("/api/command/{command?}/{payload?}", async context =>
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

                                if (string.IsNullOrEmpty(payload))
                                {
                                    context.Response.StatusCode = 400;
                                    await context.Response.WriteAsync("Payload is required");
                                    return;
                                }

                                var processor = context.RequestServices.GetRequiredService<CommandProcessor>();
                                var result = await processor.ProcessCommandAsync(command, payload);
                                await context.Response.WriteAsync(result);
                            }
                            catch (Exception ex)
                            {
                                context.Response.StatusCode = 500;
                                await context.Response.WriteAsync($"Error: {ex.Message}");
                            }
                        });

                        endpoints.MapGet("/api/health", async context =>
                        {
                            await context.Response.WriteAsync("OK");
                        });
                    });
                });

            testServer = new TestServer(webHostBuilder);
            client = testServer.CreateClient();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            client?.Dispose();
            testServer?.Dispose();
        }

        [Test]
        public async Task HealthCheck_ReturnsOK()
        {
            Assert.That(client, Is.Not.Null, "HTTP client should be initialized");
            
            var response = await client!.GetAsync("/api/health");
            var content = await response.Content.ReadAsStringAsync();

            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(content, Is.EqualTo("OK"));
        }

        [Test]
        public async Task NodeQuery_ValidInput_ReturnsExpectedResult()
        {
            Assert.That(client, Is.Not.Null, "HTTP client should be initialized");
            Assert.That(mockCommandProcessor, Is.Not.Null, "Command processor mock should be initialized");
            
            string expectedResult = "Node 1: Test Content";
            mockCommandProcessor!
                .Setup(x => x.ProcessCommandAsync("node_query", "1"))
                .ReturnsAsync(expectedResult);

            var response = await client!.GetAsync("/api/command/node_query/1");
            var content = await response.Content.ReadAsStringAsync();

            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(content, Is.EqualTo(expectedResult));
        }

        [Test]
        public async Task InvalidCommand_ReturnsBadRequest()
        {
            Assert.That(client, Is.Not.Null, "HTTP client should be initialized");
            
            // Test with missing command
            var response = await client!.GetAsync("/api/command/");
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Is.EqualTo("Command is required"));
        }

        [Test]
        public async Task EmptyPayload_ReturnsBadRequest()
        {
            Assert.That(client, Is.Not.Null, "HTTP client should be initialized");
            
            var response = await client!.GetAsync("/api/command/node_query");
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Is.EqualTo("Payload is required"));
        }

        [Test]
        public async Task ExceptionInProcessor_ReturnsInternalServerError()
        {
            Assert.That(client, Is.Not.Null, "HTTP client should be initialized");
            Assert.That(mockCommandProcessor, Is.Not.Null, "Command processor mock should be initialized");
            
            mockCommandProcessor!
                .Setup(x => x.ProcessCommandAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test error"));

            var response = await client!.GetAsync("/api/command/node_query/1");
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.InternalServerError));
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Is.EqualTo("Error: Test error"));
        }
    }
}