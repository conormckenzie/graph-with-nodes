using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.IO;
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

        [OneTimeSetUp]
        public void Setup()
        {
            var dataFolderPath = Path.Combine(Path.GetTempPath(), "ReasoningEngineTests");
            Directory.CreateDirectory(dataFolderPath);

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
            var content = await response.Content.ReadAsStringAsync();

            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(content, Is.EqualTo("OK"));
        }

        [Test]
        public async Task EndToEndTest_AddNodeAndQuery()
        {
            var addResponse = await client.GetAsync("/api/command/add_node/1|TestNode");
            var addContent = await addResponse.Content.ReadAsStringAsync();
            Assert.That(addResponse.IsSuccessStatusCode, Is.True);
            Assert.That(addContent, Does.Contain("added successfully"));

            var queryResponse = await client.GetAsync("/api/command/node_query/1");
            var queryContent = await queryResponse.Content.ReadAsStringAsync();
            Assert.That(queryResponse.IsSuccessStatusCode, Is.True);
            Assert.That(queryContent, Does.Contain("TestNode"));
        }
    }
}