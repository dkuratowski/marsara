using System;
using System.Windows.Forms;
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
                ConsoleHelper.HideConsole();
                CmdLineSwitch.ParseCommandLine(args);
                CmdLineSwitch.ExecuteSwitches();

                /// Initialize the configuration sub-system
                if (!ConfigurationManager.IsInitialized) { ConfigurationManager.Initialize(MapEditorSetup.Mode == MapEditorMode.Off ? "RC.App.root" : "RC.MapEditor.root"); }

                /// Start the components of the system
                StartComponents();

                /// Initialize the UI-core and install the XNA-plugin (TODO: make it configurable)
                UIRoot root = new UIRoot();
                Assembly xnaPlugin = Assembly.Load("RC.UI.XnaPlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                root.LoadPlugins(xnaPlugin);
                root.InstallPlugins();

                /// Activate the event sources (TODO: make it configurable)
                root.GetEventSource("RC.UI.XnaPlugin.XnaMouseEventSource").Activate();
                root.GetEventSource("RC.UI.XnaPlugin.XnaKeyboardEventSource").Activate();

                /// Create the UIWorkspace (TODO: make it configurable)
                UIWorkspace workspace = new UIWorkspace(new RCIntVector(1024, 768), MapEditorSetup.Mode == MapEditorMode.Off ? new RCIntVector(320, 200) : new RCIntVector(1024, 768));

                if (MapEditorSetup.Mode == MapEditorMode.Off)
                {
                    TraceManager.WriteAllTrace("NORMAL STARTUP...", TraceManager.GetTraceFilterID("RC.App.Info"));

                    /// Load the common resource group (TODO: make it configurable?)
                    UIResourceManager.LoadResourceGroup("RC.App.SplashScreen");

                    /// Initialize the pages of the RC application.
                    root.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(InitPages);

                    /// Start and run the render loop
                    root.GraphicsPlatform.RenderLoop.Start(workspace.DisplaySize);
                }
                else
                {
                    ConsoleHelper.ShowConsole();
                    TraceManager.WriteAllTrace("STARTING MAP EDITOR...", TraceManager.GetTraceFilterID("RC.MapEditor.Info"));

                    /// Read the parameters from the command line
                    if (MapEditorSetup.Mode == MapEditorMode.NewMap)
                    {
                        Console.Write("Name of the new map file: ");
                        MapEditorSetup.MapFile = Console.ReadLine();
                        if (File.Exists(MapEditorSetup.MapFile)) { throw new IOException(string.Format("The file '{0}' already exists!", MapEditorSetup.MapFile)); }
                        Console.Write("Name of the new map: ");
                        MapEditorSetup.MapName = Console.ReadLine();
                        Console.Write("Name of the tileset of the new map: ");
                        MapEditorSetup.TilesetName = Console.ReadLine();
                        Console.Write("Name of the default terrain of the new map: ");
                        MapEditorSetup.DefaultTerrain = Console.ReadLine();
                        Console.Write("Size of the new map: ");
                        MapEditorSetup.MapSize = XmlHelper.LoadIntVector(Console.ReadLine());
                    }
                    else if (MapEditorSetup.Mode == MapEditorMode.LoadMap)
                    {
                        Console.Write("Name of the map file to load: ");
                        MapEditorSetup.MapFile = Console.ReadLine();
                    }

                    TraceManager.WriteAllTrace(MapEditorSetup.ToString(), TraceManager.GetTraceFilterID("RC.MapEditor.Info"));

                    /// Load the resources for the map editor.
                    UIResourceManager.LoadResourceGroup("RC.MapEditor.Resources");
                    UISprite mouseIcon = UIResourceManager.GetResource<UISprite>("RC.MapEditor.Sprites.MenuPointerSprite");
                    UIBasicPointer basicPtr = new UIBasicPointer(mouseIcon, new RCIntVector(0, 0));
                    UIWorkspace.Instance.SetMousePointer(basicPtr);

                    /// Initialize the page of the map editor.
                    root.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(InitMapEditorPage);

                    /// Start and run the render loop
                    root.GraphicsPlatform.RenderLoop.Start(workspace.DisplaySize);
                }

                /// After the render loop has been stopped, release all resources of the UIRoot.
                root.Dispose();

                ComponentManager.StopComponents();
                ComponentManager.UnregisterComponentsAndPlugins();

                /// End of RC application
                if (ConsoleHelper.IsConsoleHidden)
                {
                    Console.Clear();
                    ConsoleHelper.ShowConsole();
                }
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

                MessageBox.Show("An internal error happened and the application will be closed.\nSee the contents of installed traces for more information!", "Sorry");
            }
        }

        /// <summary>
        /// Initializes the pages of the RC application.
        /// </summary>
        private static void InitPages(UIUpdateSystemEventArgs evtArgs)
        {
            UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(InitPages);

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
                mainMenuPage.AddReference("Registry", gameplayPage/*registryPage*/); // TODO: restore
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
        /// Initializes the page of the map editor.
        /// </summary>
        private static void InitMapEditorPage(UIUpdateSystemEventArgs evtArgs)
        {
            UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(InitMapEditorPage);

            /// Create and activate the map editor page.
            RCMapEditorPage mapEditorPage = null;
            if (MapEditorSetup.Mode == MapEditorMode.LoadMap)
            {
                mapEditorPage = new RCMapEditorPage(MapEditorSetup.MapFile);
            }
            else if (MapEditorSetup.Mode == MapEditorMode.NewMap)
            {
                mapEditorPage = new RCMapEditorPage(MapEditorSetup.MapFile,
                                                    MapEditorSetup.MapName,
                                                    MapEditorSetup.TilesetName,
                                                    MapEditorSetup.DefaultTerrain,
                                                    MapEditorSetup.MapSize);
            }
            UIWorkspace.Instance.RegisterPage(mapEditorPage);
            mapEditorPage.Activate();
        }

        /// <summary>
        /// Starts the components of the system.
        /// </summary>
        private static void StartComponents()
        {
            ComponentManager.RegisterComponents("RC.Engine.Maps, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                new string[]
                                                {
                                                    "RC.Engine.Maps.TileSetLoader",
                                                    "RC.Engine.Maps.MapLoader",
                                                    "RC.Engine.Maps.MapEditor",
                                                    "RC.Engine.Maps.NavMeshLoader"
                                                });

            ComponentManager.RegisterComponents("RC.Engine.Simulator, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                new string[]
                                                {
                                                    "RC.Engine.Simulator.ScenarioLoader",
                                                    "RC.Engine.Simulator.HeapManager",
                                                    "RC.Engine.Simulator.PathFinder"
                                                });

            ComponentManager.RegisterComponents("RC.App.BizLogic, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                new string[]
                                                {
                                                    "RC.App.BizLogic.TilesetManagerBC",
                                                    "RC.App.BizLogic.ScenarioManagerBC",
                                                    "RC.App.BizLogic.SelectionManagerBC",
                                                    "RC.App.BizLogic.MultiplayerService",
                                                    "RC.App.BizLogic.CommandService",
                                                    "RC.App.BizLogic.MapEditorService",
                                                    "RC.App.BizLogic.ViewService"
                                                });
            ComponentManager.RegisterComponents("RC.App.PresLogic, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                new string[]
                                                {
                                                    "RC.App.PresLogic.TaskManagerBC"
                                                });
            ComponentManager.RegisterPluginAssembly("RC.Engine.Simulator.Terran, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            ComponentManager.StartComponents();
        }
    }
}
