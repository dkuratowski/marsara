using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RC.Common.Configuration;
using System.IO;
using RC.Common;
using RC.Common.ComponentModel;

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
            ConfigurationManager.Initialize("../../../../config/RC.Engine.Test/RC.Engine.Test.root");
            ComponentManager.RegisterComponents("RC.Engine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                                                new string[2] { "RC.Engine.TileSetManager", "RC.Engine.MapManager" });
            ComponentManager.StartComponents();

            this.tilesetManager = ComponentManager.GetInterface<ITileSetManager>();
            this.mapManager = ComponentManager.GetInterface<IMapManager>();

            FileInfo tilesetFile = new FileInfo("../../../../tilesets_raw/test/test.xml");
            this.tilesetManager.LoadTileSet(tilesetFile.FullName);

            this.mapManager.Initialize();
            this.map = this.mapManager.CreateMap("Test", "Yellow", new RCIntVector(64, 32));
            TileSet tileset = this.tilesetManager.GetTileSet("Test");

            this.draw = new IsoDraw();

            this.replacedTiles = new HashSet<IIsoTile>();
            this.terrainTypes = new List<string>();
            foreach (string terrainType in tileset.TerrainTypes)
            {
                this.terrainTypes.Add(terrainType);
            }
            this.selectedTerrain = 0;
            this.Text = this.terrainTypes[this.selectedTerrain];
        }

        private IMapManager mapManager;

        private ITileSetManager tilesetManager;

        private IMapEdit map;

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
                    IEnumerable<IIsoTile> replacedTiles = map.DrawTerrain(target, map.Tileset.GetTerrainType(this.terrainTypes[this.selectedTerrain]));
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
