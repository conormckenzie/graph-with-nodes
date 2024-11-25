using System;
using DotNetEnv;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine.GraphAccess;
using DebugUtils;
using System.IO;

namespace ReasoningEngine
{
    class Program
    {
        private static List<MenuItem> mainMenuItems = new List<MenuItem>
        {
            new MenuItem("Run Setup", "#RNKA1C#", "setup"),
            new MenuItem("Graph Operations", "#D7SFN1#", "graph_operations"),
            new MenuItem("Debug Options", "#E1QTUA#", "debug_options"),
            new MenuItem("Start Web Server", "#WEB000#", "start_web_server"),
        };

        static void Main(string[] args)
        {
            // Try current directory first
            string currentDirectory = Directory.GetCurrentDirectory();
            string envPath = Path.Combine(currentDirectory, ".env");
            
            // If not found, try looking up from executable location
            if (!File.Exists(envPath))
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string solutionDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "../../../../"));
                envPath = Path.Combine(solutionDirectory, ".env");
            }
            
            DebugWriter.DebugWriteLine("#ENV001#", $"Looking for .env file at: {envPath}");
            
            if (!File.Exists(envPath))
            {
                throw new Exception($".env file not found at {envPath}. Please ensure the .env file exists in the project root directory.");
            }

            Env.Load(envPath);
            DebugWriter.DebugWriteLine("#ENV002#", "Loaded .env file successfully");

            string dataFolderPath = Environment.GetEnvironmentVariable("DATA_FOLDER_PATH") 
                                    ?? throw new Exception("DATA_FOLDER_PATH is not set in the environment variables.");

            var graphFileManager = new GraphFileManager(dataFolderPath);
            var commandProcessor = new CommandProcessor(graphFileManager);
            var graphOperationsUserMenu = new GraphOperationsUserMenu(commandProcessor);

            ShowMenu(commandProcessor, graphOperationsUserMenu);
        }

        static void ShowMenu(CommandProcessor commandProcessor, GraphOperationsUserMenu graphOperationsUserMenu)
        {
            while (true)
            {
                DebugWriter.DebugWriteLine("#0D7D01#", "\nMain Menu:");

                for (int i = 0; i < mainMenuItems.Count; i++)
                {
                    DebugWriter.DebugWriteLine(mainMenuItems[i].DebugString, $"{i + 1}. {mainMenuItems[i].DisplayText}");
                }

                DebugWriter.DebugWriteLine("#0D7D00#", "0. Exit");

                DebugWriter.DebugWrite("#0D7E00#", "Enter option: ");
                var option = Console.ReadLine();

                if (option == "0")
                {
                    return;
                }
                else if (int.TryParse(option, out int selectedOption) && selectedOption > 0 && selectedOption <= mainMenuItems.Count)
                {
                    var selectedItem = mainMenuItems[selectedOption - 1];

                    switch (selectedItem.InternalText)
                    {
                        case "setup":
                            OneTimeSetup.Initialize();
                            break;
                        case "graph_operations":
                            graphOperationsUserMenu.ShowMenu();
                            break;
                        case "debug_options":
                            DebugOptions.ShowDebugOptionsMenu();
                            break;
                        case "start_web_server":
                            var webServer = new WebServer(commandProcessor);
                            webServer.Start();
                            break;
                        default:
                            DebugWriter.DebugWriteLine("#00INV1#", "Invalid option. Please try again.");
                            break;
                    }
                }
                else
                {
                    DebugWriter.DebugWriteLine("#00INV2#", "Invalid option. Please try again.");
                }
            }
        }
    }
}