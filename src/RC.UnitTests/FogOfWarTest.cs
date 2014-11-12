using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RC.App.BizLogic.Views;
using RC.App.PresLogic;
using RC.Common;
using RC.Common.Configuration;
using RC.UI;

namespace RC.UnitTests
{
    /// <summary>
    /// Implements test cases for testing Fog Of War.
    /// </summary>    
    [TestClass]
    public class FogOfWarTest
    {
        /// <summary>
        /// The input and output directories.
        /// </summary>
        public const string OUTPUT_DIR = ".\\FogOfWarTest_out";
        public const string FOW_SPRITEPALETTE_DIR = "..\\..\\..\\..\\sprites\\fow";
        public const string FOW_SPRITEPALETTE_FILE = "fow.xml";

        /// <summary>
        /// This initializer method creates the output directory if it doesn't exist.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            UIRoot root = new UIRoot();
            Assembly xnaPlugin = Assembly.Load("RC.UI.XnaPlugin, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            root.LoadPlugins(xnaPlugin);
            root.InstallPlugins();
            UIWorkspace workspace = new UIWorkspace(new RCIntVector(1024, 768), new RCIntVector(1024, 768));
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
        /// Tests whether the FOWSpriteGroup for drawing full Fog Of War is generated properly.
        /// </summary>
        [TestMethod]
        public void FullFOWSpriteGroupTest() { this.FOWSpriteGroupTest(FOWTypeEnum.Full); }

        /// <summary>
        /// Tests whether the FOWSpriteGroup for drawing partial Fog Of War is generated properly.
        /// </summary>
        [TestMethod]
        public void PartialFOWSpriteGroupTest() { this.FOWSpriteGroupTest(FOWTypeEnum.Partial); }

        /// <summary>
        /// Tests whether the FOWSpriteGroup for drawing the given type of Fog Of War is generated properly.
        /// </summary>
        private void FOWSpriteGroupTest(FOWTypeEnum fowType)
        {
            /// Load the Fog Of War sprite palette and create a FOWSpriteGroup out of it.
            string fowSpritePalettePath = System.IO.Path.Combine(FOW_SPRITEPALETTE_DIR, FOW_SPRITEPALETTE_FILE);
            XDocument fowSpritePaletteXml = XDocument.Load(fowSpritePalettePath);
            ISpritePalette<FOWTypeEnum> spritePalette = XmlHelper.LoadSpritePalette(fowSpritePaletteXml.Root, fowType, FOW_SPRITEPALETTE_DIR);
            FOWSpriteGroup spriteGroup = new FOWSpriteGroup(spritePalette, fowType);
            spriteGroup.Load();

            /// Check that 2 UISprite instances are the same if and only if they images have the same bytes.
            PrivateObject spriteGroupObj = new PrivateObject(spriteGroup, new PrivateType(typeof(SpriteGroup)));
            List<UISprite> fowSprites = (List<UISprite>)spriteGroupObj.GetField("spriteList");
            HashSet<UISprite> savedFowSprites = new HashSet<UISprite>();
            for (int indexA = 0; indexA < fowSprites.Count - 1; indexA++)
            {
                if (fowSprites[indexA] == null) { continue; }
                byte[] bytesOfSpriteA = fowSprites[indexA].Save();
                for (int indexB = indexA + 1; indexB < fowSprites.Count; indexB++)
                {
                    if (fowSprites[indexB] == null) { continue; }
                    byte[] bytesOfSpriteB = fowSprites[indexB].Save();

                    if (StructuralComparisons.StructuralEqualityComparer.Equals(bytesOfSpriteA, bytesOfSpriteB))
                    {
                        Assert.AreEqual(fowSprites[indexA], fowSprites[indexB]);
                    }
                    else
                    {
                        Assert.AreNotEqual(fowSprites[indexA], fowSprites[indexB]);
                    }
                }

                if (savedFowSprites.Add(fowSprites[indexA]))
                {
                    string outputPath = System.IO.Path.Combine(OUTPUT_DIR, string.Format("{0}_{1}.png", fowType, Convert.ToString(indexA, 2).PadLeft(9, '0')));
                    File.WriteAllBytes(outputPath, bytesOfSpriteA);
                }
            }

            /// Destroy the FOWSpriteGroup object.
            spriteGroup.Unload();
        }
    }
}
