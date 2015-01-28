using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RC.App.BizLogic.BusinessComponents;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.App.PresLogic.Controls;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Configuration;
using RC.UI;

namespace RC.UnitTests
{
    [TestClass]
    public class MinimapRenderTest
    {
        public const string OUTPUT_DIR = ".\\MinimapRenderTest_out";

        /// <summary>
        /// This initializer method creates the output directory if it doesn't exist.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ConstantsTable.Add("RC.App.BizLogic.TileSetDir", "../../../../tilesets", "STRING");
            ConstantsTable.Add("RC.Engine.Simulator.MetadataDir", "../../../../metadata", "STRING");
            ConstantsTable.Add("RC.App.BizLogic.CommandDir", "../../../../commands", "STRING");
            
            UIRoot root = new UIRoot();
            Assembly xnaPlugin = Assembly.Load("RC.UI.XnaPlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            root.LoadPlugins(xnaPlugin);
            root.InstallPlugins();
            UIWorkspace workspace = new UIWorkspace(new RCIntVector(1024, 768), new RCIntVector(1024, 768));
            Directory.CreateDirectory(OUTPUT_DIR);

            StartComponents();
        }

        /// <summary>
        /// Cleanup method.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            StopComponents();

            UIWorkspace.Instance.Dispose();
            UIRoot.Instance.Dispose();
        }

        /// <summary>
        /// </summary>
        [TestMethod]
        public void RenderMap4_Test()
        {
            IScenarioManagerBC scenarioManager = ComponentManager.GetInterface<IScenarioManagerBC>();
            IMapWindowBC mapWindowBC = ComponentManager.GetInterface<IMapWindowBC>();

            scenarioManager.OpenScenario("../../../../maps/testmap4.rcm");

            RCIntVector mapImageSize = mapWindowBC.FullWindow.PixelWindow.Size;
            UISprite mapImage = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Black, mapImageSize);
            IUIRenderContext renderContext = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateRenderContext(mapImage);

            RCMapDisplayBasic mapDisplayControl =
                new RCMapDisplayBasic(new RCIntVector(0, 0), mapImageSize);
            bool connectorOpFinished = false;
            mapDisplayControl.ConnectorOperationFinished += (sender) =>
                {
                    connectorOpFinished = true;
                };
            mapDisplayControl.Connect();
            while (!connectorOpFinished)
            {
                RCThread.Sleep(500);
                UITaskManager.OnUpdate();
            }
            mapDisplayControl.Render(renderContext);

            connectorOpFinished = false;
            mapDisplayControl.Disconnect();
            while (!connectorOpFinished)
            {
                RCThread.Sleep(500);
                UITaskManager.OnUpdate();
            }

            UIRoot.Instance.GraphicsPlatform.SpriteManager.CloseRenderContext(mapImage);
            string outputPath = System.IO.Path.Combine(OUTPUT_DIR, "testmap4.png");
            mapImage.Save(outputPath);
            UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(mapImage);
        }

        /// <summary>
        /// Starts the necessary components of the system.
        /// </summary>
        private static void StartComponents()
        {
            ComponentManager.RegisterComponents("RC.Engine.Maps, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                new string[]
                                                {
                                                    "RC.Engine.Maps.TileSetLoader",
                                                    "RC.Engine.Maps.MapLoader",
                                                    //"RC.Engine.Maps.MapEditor",
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
                                                    //"RC.App.BizLogic.SelectionManagerBC",
                                                    //"RC.App.BizLogic.CommandManagerBC",
                                                    "RC.App.BizLogic.FogOfWarBC",
                                                    "RC.App.BizLogic.MapWindowBC",
                                                    //"RC.App.BizLogic.MultiplayerService",
                                                    //"RC.App.BizLogic.CommandService",
                                                    //"RC.App.BizLogic.MapEditorService",
                                                    "RC.App.BizLogic.ViewService"
                                                });
            ComponentManager.StartComponents();
        }
        
        /// <summary>
        /// Stops the started components of the system.
        /// </summary>
        private static void StopComponents()
        {
            ComponentManager.StopComponents();
        }
    }
}
