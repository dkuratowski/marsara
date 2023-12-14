using System;
// using System.Windows.Forms;
using RC.Common.Configuration;
using RC.UI;
using System.Reflection;
using RC.Common;
using RC.Common.Diagnostics;
using RC.Common.ComponentModel;
using RC.App.PresLogic.Pages;
using System.IO;

namespace RC.App.Starter
{
    class Program
    {
        /// <summary>
        /// This is the starting point of the RC application. Command line switches for RC.exe:
        /// /cfg filename --> Name of the root configuration file to initialize with (RC.root by default).
        /// /c --> Run RC.exe with console window (console is hidden by default). You can also toggle the console
        ///        window by pressing the CTRL + ALT + SHIFT + C combination during runtime.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                /// Read and execute the command line switches
                //ConsoleHelper.HideConsole();
                CmdLineSwitch.ParseCommandLine(args);
                CmdLineSwitch.ExecuteSwitches();

                /// Initialize the configuration sub-system
                if (!ConfigurationManager.IsInitialized)
                {
                    if (RCAppSetup.Mode == RCAppMode.Normal || RCAppSetup.Mode == RCAppMode.MultiplayerHost || RCAppSetup.Mode == RCAppMode.MultiplayerGuest)
                    {
                        ConfigurationManager.Initialize("RC.App.root");
                    }
                    else if (RCAppSetup.Mode == RCAppMode.NewMap || RCAppSetup.Mode == RCAppMode.LoadMap)
                    {
                        ConfigurationManager.Initialize("RC.MapEditor.root");
                    }
                }

                /// Start the components of the system
                StartComponents();

                /// Initialize the UI-core and install the MonoGame plugin (TODO: make it configurable)
                UIRoot root = new UIRoot(RCAppSetup.WorkspacePosition);
                Assembly monogamePlugin = Assembly.Load("RC.UI.MonoGamePlugin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
                root.LoadPlugins(monogamePlugin);
                root.InstallPlugins();

                /// Create the UIWorkspace (TODO: make it configurable)
                UIWorkspace workspace = null;
                if (RCAppSetup.Mode == RCAppMode.Normal || RCAppSetup.Mode == RCAppMode.MultiplayerHost || RCAppSetup.Mode == RCAppMode.MultiplayerGuest)
                {
                    workspace = new UIWorkspace(new RCIntVector(1024, 768), new RCIntVector(320, 200));
                }
                else if (RCAppSetup.Mode == RCAppMode.NewMap || RCAppSetup.Mode == RCAppMode.LoadMap)
                {
                    workspace = new UIWorkspace(new RCIntVector(1024, 768), new RCIntVector(1024, 768));
                }

                if (RCAppSetup.Mode == RCAppMode.Normal)
                {
                    TraceManager.WriteAllTrace("NORMAL STARTUP...", TraceManager.GetTraceFilterID("RC.App.Info"));

                    /// Load the resource group for displaying the splash screen (TODO: make it configurable?)
                    UIResourceManager.LoadResourceGroup("RC.App.SplashScreen");

                    /// Initialize the pages of the RC application for normal mode.
                    root.GraphicsPlatform.RenderLoop.FrameUpdate += InitPages;

                    /// Start and run the render loop
                    root.GraphicsPlatform.RenderLoop.Start(workspace.DisplaySize);
                }
                else if (RCAppSetup.Mode == RCAppMode.MultiplayerHost || RCAppSetup.Mode == RCAppMode.MultiplayerGuest)
                {
                    TraceManager.WriteAllTrace("MULTIPLAYER STARTUP...", TraceManager.GetTraceFilterID("RC.App.Info"));

                    /// Load the common resource group (TODO: make it configurable?)
                    UIResourceManager.LoadResourceGroup("RC.App.CommonResources");

                    /// Set the default mouse pointer.
                    workspace.SetDefaultMousePointer(UIResourceManager.GetResource<UIPointer>("RC.App.Pointers.NormalPointer"));

                    /// Initialize the pages of the RC application for multiplayer mode.
                    root.GraphicsPlatform.RenderLoop.FrameUpdate += InitMultiplayerPages;

                    /// Start and run the render loop
                    root.GraphicsPlatform.RenderLoop.Start(workspace.DisplaySize);
                }
                else
                {
                    //ConsoleHelper.ShowConsole();
                    TraceManager.WriteAllTrace("STARTING MAP EDITOR...", TraceManager.GetTraceFilterID("RC.MapEditor.Info"));

                    /// Read the parameters from the command line
                    if (RCAppSetup.Mode == RCAppMode.NewMap)
                    {
                        Console.Write("Name of the new map file: ");
                        RCAppSetup.MapFile = Console.ReadLine();
                        if (File.Exists(RCAppSetup.MapFile)) { throw new IOException(string.Format("The file '{0}' already exists!", RCAppSetup.MapFile)); }
                        Console.Write("Name of the new map: ");
                        RCAppSetup.MapName = Console.ReadLine();
                        Console.Write("Name of the tileset of the new map: ");
                        RCAppSetup.TilesetName = Console.ReadLine();
                        Console.Write("Name of the default terrain of the new map: ");
                        RCAppSetup.DefaultTerrain = Console.ReadLine();
                        Console.Write("Size of the new map: ");
                        RCAppSetup.MapSize = XmlHelper.LoadIntVector(Console.ReadLine());
                    }
                    else if (RCAppSetup.Mode == RCAppMode.LoadMap)
                    {
                        Console.Write("Name of the map file to load: ");
                        RCAppSetup.MapFile = Console.ReadLine();
                    }

                    TraceManager.WriteAllTrace(RCAppSetup.ToString(), TraceManager.GetTraceFilterID("RC.MapEditor.Info"));

                    /// Load the resources for the map editor.
                    UIResourceManager.LoadResourceGroup("RC.MapEditor.Resources");

                    /// Set the default mouse pointer.
                    workspace.SetDefaultMousePointer(UIResourceManager.GetResource<UIPointer>("RC.App.Pointers.NormalPointer"));

                    /// Initialize the page of the map editor.
                    root.GraphicsPlatform.RenderLoop.FrameUpdate += InitMapEditorPage;

                    /// Start and run the render loop
                    root.GraphicsPlatform.RenderLoop.Start(workspace.DisplaySize);
                }

                /// After the render loop has been stopped, release all resources of the UIRoot.
                root.Dispose();

                ComponentManager.StopComponents();
                ComponentManager.UnregisterComponentsAndPlugins();

                /// End of RC application
                // if (ConsoleHelper.IsConsoleHidden)
                // {
                //     Console.Clear();
                //     ConsoleHelper.ShowConsole();
                // }
            }
            catch (Exception ex)
            {
                /// Catch any exception from the UI-thread, write it to the console and show a "sorry" message-box
                Exception currException = ex;
                Console.WriteLine(currException.ToString());
                while (currException.InnerException != null)
                {
                    currException = currException.InnerException;
                    Console.WriteLine(currException.ToString());
                }

                // TODO: Removed this Windows Forms specific call.
                //MessageBox.Show("An internal error happened and the application will be closed.\nSee the contents of installed traces for more information!", "Sorry");
            }
        }

        /// <summary>
        /// Initializes the pages of the RC application in normal mode.
        /// </summary>
        private static void InitPages()
        {
            UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate -= InitPages;

            /// Create the main page
            RCMainMenuPage mainMenuPage = new RCMainMenuPage();
            UIWorkspace.Instance.RegisterPage(mainMenuPage);

            mainMenuPage.LoadFinished += delegate()
            {
                /// Create the pages
                RCCreditsPage creditsPage = new RCCreditsPage();
                RCRegistryPage registryPage = new RCRegistryPage();
                RCSelectGamePage selectGamePage = new RCSelectGamePage();
                RCCreateGamePage createGamePage = new RCCreateGamePage();
                RCMultiSetupPage multiSetupPage = new RCMultiSetupPage();
                RCGameplayPage gameplayPage = new RCGameplayPage();
                RCResultsPage resultsPage = new RCResultsPage();

                /// Register the pages
                UIWorkspace.Instance.RegisterPage(creditsPage);
                UIWorkspace.Instance.RegisterPage(registryPage);
                UIWorkspace.Instance.RegisterPage(selectGamePage);
                UIWorkspace.Instance.RegisterPage(createGamePage);
                UIWorkspace.Instance.RegisterPage(multiSetupPage);
                UIWorkspace.Instance.RegisterPage(gameplayPage);
                UIWorkspace.Instance.RegisterPage(resultsPage);

                /// Set the page references
                mainMenuPage.AddReference("Credits", creditsPage);
                mainMenuPage.AddReference("Registry", gameplayPage); // TODO: restore the line below!
                //mainMenuPage.AddReference("Registry", registryPage);
                creditsPage.AddReference("MainMenu", mainMenuPage);
                registryPage.AddReference("MainMenu", mainMenuPage);
                registryPage.AddReference("SelectGame", selectGamePage);
                selectGamePage.AddReference("Registry", registryPage);
                selectGamePage.AddReference("CreateGame", createGamePage);
                selectGamePage.AddReference("MultiSetup", multiSetupPage);
                createGamePage.AddReference("SelectGame", selectGamePage);
                createGamePage.AddReference("MultiSetup", multiSetupPage);
                multiSetupPage.AddReference("SelectGame", selectGamePage);
                multiSetupPage.AddReference("Gameplay", gameplayPage);
                gameplayPage.AddReference("Results", resultsPage);
                resultsPage.AddReference("SelectGame", selectGamePage);
            };

            /// Activate the main menu page
            mainMenuPage.Activate();
        }

        /// <summary>
        /// Initializes the pages of the RC application in multiplayer mode.
        /// </summary>
        private static void InitMultiplayerPages()
        {
            UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate -= InitMultiplayerPages;

            /// Create the pages.
            RCMultiSetupPage multiSetupPage = new RCMultiSetupPage();
            RCGameplayPage gameplayPage = new RCGameplayPage();

            /// Register the pages.
            UIWorkspace.Instance.RegisterPage(multiSetupPage);
            UIWorkspace.Instance.RegisterPage(gameplayPage);

            /// Set the page references
            multiSetupPage.AddReference("Gameplay", gameplayPage);

            /// Activate the multiplayer setup page.
            multiSetupPage.Activate();
        }

        /// <summary>
        /// Initializes the page of the map editor.
        /// </summary>
        private static void InitMapEditorPage()
        {
            UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate -= InitMapEditorPage;

            /// Create and activate the map editor page.
            RCMapEditorPage mapEditorPage = null;
            if (RCAppSetup.Mode == RCAppMode.LoadMap)
            {
                mapEditorPage = new RCMapEditorPage(RCAppSetup.MapFile);
            }
            else if (RCAppSetup.Mode == RCAppMode.NewMap)
            {
                mapEditorPage = new RCMapEditorPage(RCAppSetup.MapFile,
                                                    RCAppSetup.MapName,
                                                    RCAppSetup.TilesetName,
                                                    RCAppSetup.DefaultTerrain,
                                                    RCAppSetup.MapSize);
            }
            UIWorkspace.Instance.RegisterPage(mapEditorPage);
            mapEditorPage.Activate();
        }

        /// <summary>
        /// Starts the components of the system.
        /// </summary>
        private static void StartComponents()
        {
            ComponentManager.RegisterComponents("RC.Engine.Pathfinder, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                "RC.Engine.Pathfinder");

            ComponentManager.RegisterComponents("RC.Engine.Maps, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                "RC.Engine.Maps.TileSetLoader",
                                                "RC.Engine.Maps.MapLoader",
                                                "RC.Engine.Maps.MapEditor");

            ComponentManager.RegisterComponents("RC.Engine.Simulator, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                "RC.Engine.Simulator.ElementFactory",
                                                "RC.Engine.Simulator.CommandExecutor",
                                                "RC.Engine.Simulator.ScenarioLoader",
                                                "RC.Engine.Simulator.HeapManager");

            ComponentManager.RegisterComponents("RC.App.BizLogic, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                "RC.App.BizLogic.TilesetManagerBC",
                                                "RC.App.BizLogic.ScenarioManagerBC",
                                                "RC.App.BizLogic.SelectionManagerBC",
                                                "RC.App.BizLogic.CommandManagerBC",
                                                "RC.App.BizLogic.FogOfWarBC",
                                                "RC.App.BizLogic.MapWindowBC",
                                                "RC.App.BizLogic.MultiplayerService",
                                                "RC.App.BizLogic.CommandService",
                                                "RC.App.BizLogic.SelectionService",
                                                "RC.App.BizLogic.ScrollService",
                                                "RC.App.BizLogic.MapEditorService",
                                                "RC.App.BizLogic.ViewService");

            ComponentManager.RegisterComponents("RC.App.PresLogic, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                "RC.App.PresLogic.TaskManagerBC");

            ComponentManager.RegisterPluginAssembly("RC.Engine.Simulator.Terran, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

            ComponentManager.StartComponents();
        }
    }
}
