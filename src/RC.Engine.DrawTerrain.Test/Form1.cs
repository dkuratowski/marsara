using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Maps.ComponentInterfaces;

namespace RC.Engine.DrawTerrain.Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ComponentManager.RegisterComponents("RC.Engine.Maps, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                new string[3] { "RC.Engine.Maps.TileSetLoader", "RC.Engine.Maps.MapLoader", "RC.Engine.Maps.MapEditor" });
            ComponentManager.StartComponents();

            this.tilesetLoader = ComponentManager.GetInterface<ITileSetLoader>();
            this.mapLoader = ComponentManager.GetInterface<IMapLoader>();
            this.mapEditor = ComponentManager.GetInterface<IMapEditor>();

            /// TODO: this is a hack!
            FileInfo tilesetFile = new FileInfo("../../../../tilesets/test/test.xml");
            string xmlStr = File.ReadAllText(tilesetFile.FullName);
            string imageDir = tilesetFile.DirectoryName;
            RCPackage tilesetPackage = RCPackage.CreateCustomDataPackage(PackageFormats.TILESET_FORMAT);
            tilesetPackage.WriteString(0, xmlStr);
            tilesetPackage.WriteString(1, imageDir);

            byte[] buffer = new byte[tilesetPackage.PackageLength];
            tilesetPackage.WritePackageToBuffer(buffer, 0);
            ITileSet tileset = this.tilesetLoader.LoadTileSet(buffer);

            this.map = this.mapLoader.NewMap("TestMap", tileset, "Yellow", new RCIntVector(64, 32));

            this.draw = new IsoDraw();

            this.replacedTiles = new HashSet<IIsoTile>();
            this.terrainTypes = new List<string>();
            foreach (ITerrainType terrainType in tileset.TerrainTypes)
            {
                this.terrainTypes.Add(terrainType.Name);
            }
            this.selectedTerrain = 0;
            this.Text = this.terrainTypes[this.selectedTerrain];
        }

        private IMapLoader mapLoader;

        private ITileSetLoader tilesetLoader;

        private IMapAccess map;

        private IMapEditor mapEditor;

        private IsoDraw draw;

        private HashSet<IIsoTile> replacedTiles;

        private List<string> terrainTypes;

        private int selectedTerrain;

        protected override void OnPaint(PaintEventArgs e)
        {
            foreach (IIsoTile tile in this.map.IsometricTiles)
            {
                this.draw.DrawTile(tile, this.replacedTiles.Contains(tile), e.Graphics);
            }
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                RCIntVector screenCoords = new RCIntVector(e.X, e.Y);
                RCIntVector mapCoords = (screenCoords / 16) - new RCIntVector(1, 16);
                IIsoTile target = map.GetIsoTile(mapCoords);
                if (target != null)
                {
                    this.replacedTiles.Clear();
                    IEnumerable<IIsoTile> replacedTiles = this.mapEditor.DrawTerrain(this.map, target, map.Tileset.GetTerrainType(this.terrainTypes[this.selectedTerrain]));
                    foreach (IIsoTile item in replacedTiles)
                    {
                        this.replacedTiles.Add(item);
                    }
                }
                this.Invalidate();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                this.selectedTerrain++;
                if (this.selectedTerrain == this.terrainTypes.Count) { this.selectedTerrain = 0; }
                this.Text = this.terrainTypes[this.selectedTerrain];
            }
        }

        //protected override void OnPaintBackground(PaintEventArgs e)
        //{
        //    return;
        //}
    }
}
