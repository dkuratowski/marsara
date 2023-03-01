using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RC.Common;
using RC.Common.Configuration;
using RC.UI;

namespace RC.UnitTests
{
    /// <summary>
    /// Collects the text widths for the info panel in logical pixels.
    /// </summary>
    [TestClass]
    public class InfoPanelTextWidthTest
    {
        /// <summary>
        /// The input and output directories.
        /// </summary>
        public const string INPUT_DIR = "./InfoPanelTextWidthTest_in";
        public const string OUTPUT_DIR = "./InfoPanelTextWidthTest_out";
        public const string ENTITY_TYPE_NAMES_FILE = "entity_type_names.txt";
        public const string ENTITY_TYPE_NAME_WIDTHS_OUT_FILE = "entity_type_name_widths.txt";

        /// <summary>
        /// This initializer method creates the output directory if it doesn't exist.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ConfigurationManager.Initialize("../../../../config/RC.UI.Test/RC.UI.Test.root");
            //ConstantsTable.Add("RC.App.Version", "1.0.0.0", "STRING");

            UIRoot root = new UIRoot();
            Assembly xnaPlugin = Assembly.Load("RC.UI.XnaPlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            root.LoadPlugins(xnaPlugin);
            root.InstallPlugins();
            UIWorkspace workspace = new UIWorkspace(new RCIntVector(1024, 768), new RCIntVector(1024, 768));
            UIResourceManager.LoadResourceGroup("RC.App.SplashScreen");
            UIResourceManager.LoadResourceGroup("RC.App.CommonResources");
            Directory.CreateDirectory(OUTPUT_DIR);
        }

        /// <summary>
        /// Cleanup method.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            UIWorkspace.Instance.Dispose();
            UIRoot.Instance.Dispose();
        }

        /// <summary>
        /// Calculates the text width in logical pixels for each entity type names.
        /// </summary>
        [TestMethod]
        public void EntityTypeNameTextWidthTest()
        {
            string entityTypeNamesPath = System.IO.Path.Combine(INPUT_DIR, ENTITY_TYPE_NAMES_FILE);
            string[] entityTypeNames = File.ReadAllLines(entityTypeNamesPath);
            string[] entityTypeNameWidths = new string[entityTypeNames.Length];
            for (int i = 0; i < entityTypeNames.Length; i++)
            {
                UIString typeNameString = new UIString(entityTypeNames[i], UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5"), new RCIntVector(1, 1), RCColor.White);

                //UISprite typeNameSprite = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.Black, new RCIntVector(typeNameString.Width, typeNameString.Font.MinimumLineHeight));
                //IUIRenderContext typeNameSpriteContext = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateRenderContext(typeNameSprite);
                //typeNameSpriteContext.RenderString(typeNameString);

                entityTypeNameWidths[i] = string.Format("{0} - Width: {1}, Height: {2}", entityTypeNames[i], typeNameString.Width, typeNameString.Font.MinimumLineHeight);
                typeNameString.Dispose();
            }
            string outputPath = System.IO.Path.Combine(OUTPUT_DIR, ENTITY_TYPE_NAME_WIDTHS_OUT_FILE);
            File.WriteAllLines(outputPath, entityTypeNameWidths);
        }
    }
}
