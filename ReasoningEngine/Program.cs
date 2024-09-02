using System;
using System.Collections.Generic;
using DotNetEnv;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine.Communication;
using Newtonsoft.Json;
using DebugUtils;

namespace ReasoningEngine
{
    class Program
    {
        // List to store nodes
        public static List<Node> nodes = new List<Node>();

        // List to store edges
        public static List<Edge> edges = new List<Edge>();

        // Lists to track changes in nodes and edges
        public static List<Node> nodeChanges = new List<Node>();
        public static List<Edge> edgeChanges = new List<Edge>();

        static void Main(string[] args)
        {
            // Load environment variables from the .env file
            Env.Load();

            // Get the data folder path from environment variables, or throw an exception if not set
            string dataFolderPath = Environment.GetEnvironmentVariable("DATA_FOLDER_PATH")
                                    ?? throw new Exception("DATA_FOLDER_PATH is not set in the environment variables.");

            // Create an instance of GraphFileManager with the data folder path
            var manager = new GraphFileManager(dataFolderPath);

            // Create an instance of CommandProcessor
            var commandProcessor = new CommandProcessor(manager);

            // Display the main menu and handle user input
            ShowMainMenu(manager, commandProcessor);
        }

        /// <summary>
        /// Displays the main menu and handles user input.
        /// </summary>
        /// <param name="manager">The GraphFileManager instance to handle save/load operations.</param>
        /// <param name="commandProcessor">The CommandProcessor instance to handle user commands.</param>
        static void ShowMainMenu(GraphFileManager manager, CommandProcessor commandProcessor)
        {
            while (true)
            {
                // Display menu options
                DebugWriter.DebugWriteLine("#D7D1#", "Main Menu:");
                DebugWriter.DebugWriteLine("#D7D2#", "1. Run Setup");
                DebugWriter.DebugWriteLine("#D7D3#", "2. Save Node");
                DebugWriter.DebugWriteLine("#D7D4#", "3. Load Node");
                DebugWriter.DebugWriteLine("#D7D5#", "4. Set Debug Mode");
                DebugWriter.DebugWriteLine("#D7D6#", "5. Execute Command Menu");
                DebugWriter.DebugWriteLine("#D7D7#", "6. Exit");
                DebugWriter.DebugWrite("#D7D8#", "Enter option: ");

                // Read user input
                var option = Console.ReadLine();

                // Handle user input
                switch (option)
                {
                    case "1":
                        OneTimeSetup.Initialize();
                        break;
                    case "2":
                        // manager.SaveNodeWithUserInput();
                        DebugWriter.DebugWriteLine("#SOR1#", "Sorry, this has been disabled for now");
                        break;
                    case "3":
                        // manager.LoadNodeWithUserInput();
                        DebugWriter.DebugWriteLine("#SOR2#", "Sorry, this has been disabled for now");
                        break;
                    case "4":
                        DebugOptions.SetDebugMode();
                        break;
                    case "5":
                        commandProcessor.ShowCommandMenu();
                        break;
                    case "6":
                        return; // Exit the loop and end the program
                    default:
                        DebugWriter.DebugWriteLine("#D7D9#", "Invalid option. Please try again.");
                        break;
                }
            }
        }
    }
}
