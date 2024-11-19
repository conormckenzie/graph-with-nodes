using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ReasoningEngine.GraphAccess;
using ReasoningEngine.GraphFileHandling;

namespace ReasoningEngine.Tests
{
    [TestFixture]
    public class WebServerIntegrationTests
    {
        private HttpClient client;
        private WebServer webServer;
        private Task serverTask;
        private CancellationTokenSource cancellationTokenSource;

        // Define serializer options to match the response format
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        [OneTimeSetUp]
        public void Setup()
        {
            var dataFolderPath = Path.Combine(Path.GetTempPath(), "ReasoningEngineTests");
            Directory.CreateDirectory(dataFolderPath);

            Environment.SetEnvironmentVariable("DATA_FOLDER_PATH", dataFolderPath);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            var graphFileManager = new GraphFileManager(dataFolderPath);
            var commandProcessor = new CommandProcessor(graphFileManager);

            webServer = new WebServer(commandProcessor);
            cancellationTokenSource = new CancellationTokenSource();
            serverTask = Task.Run(() => webServer.Start(), cancellationTokenSource.Token);

            Thread.Sleep(2000); // Give the server time to start

            client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5000");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            client?.Dispose();
            
            var dataFolderPath = Path.Combine(Path.GetTempPath(), "ReasoningEngineTests");
            if (Directory.Exists(dataFolderPath))
            {
                Directory.Delete(dataFolderPath, true);
            }
        }

        [Test]
        public async Task HealthCheck_ReturnsOK()
        {
            var response = await client.GetAsync("/api/health");
            Assert.That(response.IsSuccessStatusCode, Is.True, "HTTP request failed");
            
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response content: {content}");

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(content, SerializerOptions);
            Assert.That(apiResponse, Is.Not.Null, "Failed to deserialize response");
            Assert.That(apiResponse!.Success, Is.True, $"API response indicated failure: {apiResponse.Error}");
            Assert.That(apiResponse.Data, Is.EqualTo("OK"));
        }

        [Test]
        public async Task EndToEndTest_AddNodeAndQuery()
        {
            // Create node
            var createPayload = new CommandRequest { Payload = "1|TestNode" };
            var createContent = new StringContent(
                JsonSerializer.Serialize(createPayload, SerializerOptions),
                Encoding.UTF8,
                "application/json");

            // First run setup
            await client.GetAsync("/api/commands/setup");

            var addResponse = await client.PostAsync("/api/nodes/create", createContent);
            Assert.That(addResponse.IsSuccessStatusCode, Is.True, "HTTP request failed");
            
            var addContent = await addResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Add node response: {addContent}");
            
            var addResult = JsonSerializer.Deserialize<ApiResponse<string>>(addContent, SerializerOptions);
            Assert.That(addResult, Is.Not.Null, "Failed to deserialize response");
            Assert.That(addResult!.Success, Is.True, $"API response indicated failure: {addResult.Error}");
            Assert.That(addResult.Data, Does.Contain("added successfully"));

            // Query node
            var queryResponse = await client.GetAsync("/api/nodes/1/get");
            Assert.That(queryResponse.IsSuccessStatusCode, Is.True, "HTTP request failed");
            
            var queryContent = await queryResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Query node response: {queryContent}");
            
            var queryResult = JsonSerializer.Deserialize<ApiResponse<string>>(queryContent, SerializerOptions);
            Assert.That(queryResult, Is.Not.Null, "Failed to deserialize response");
            Assert.That(queryResult!.Success, Is.True, $"API response indicated failure: {queryResult.Error}");
            Assert.That(queryResult.Data, Does.Contain("TestNode"));
        }

        [Test]
        public async Task EndToEndTest_EdgeOperations()
        {
            // First run setup
            await client.GetAsync("/api/commands/setup");

            // Create two nodes
            var createNode1 = new CommandRequest { Payload = "1|Node1" };
            var createNode2 = new CommandRequest { Payload = "2|Node2" };
            
            await client.PostAsync("/api/nodes/create", 
                new StringContent(JsonSerializer.Serialize(createNode1, SerializerOptions), 
                    Encoding.UTF8, "application/json"));
            await client.PostAsync("/api/nodes/create", 
                new StringContent(JsonSerializer.Serialize(createNode2, SerializerOptions), 
                    Encoding.UTF8, "application/json"));

            // Create edge between nodes
            var createEdge = new CommandRequest { Payload = "1|2|1.5|TestEdge" };
            var createEdgeResponse = await client.PostAsync("/api/edges/create",
                new StringContent(JsonSerializer.Serialize(createEdge, SerializerOptions), 
                    Encoding.UTF8, "application/json"));
            
            Assert.That(createEdgeResponse.IsSuccessStatusCode, Is.True, "HTTP request failed");
            
            var createEdgeContent = await createEdgeResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Create edge response: {createEdgeContent}");
            
            var createEdgeResult = JsonSerializer.Deserialize<ApiResponse<string>>(createEdgeContent, SerializerOptions);
            Assert.That(createEdgeResult, Is.Not.Null, "Failed to deserialize response");
            Assert.That(createEdgeResult!.Success, Is.True, $"API response indicated failure: {createEdgeResult.Error}");
            Assert.That(createEdgeResult.Data, Does.Contain("added successfully"));

            // Query outgoing edges
            var outgoingResponse = await client.GetAsync("/api/nodes/1/edges/outgoing/list");
            Assert.That(outgoingResponse.IsSuccessStatusCode, Is.True, "HTTP request failed");
            
            var outgoingContent = await outgoingResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Query edges response: {outgoingContent}");
            
            var outgoingResult = JsonSerializer.Deserialize<ApiResponse<string>>(outgoingContent, SerializerOptions);
            Assert.That(outgoingResult, Is.Not.Null, "Failed to deserialize response");
            Assert.That(outgoingResult!.Success, Is.True, $"API response indicated failure: {outgoingResult.Error}");
            Assert.That(outgoingResult.Data, Does.Contain("TestEdge"));
        }
    }
}