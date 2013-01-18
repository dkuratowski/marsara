using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using RC.Common;

namespace RC.Engine.DrawTerrain.Test
{
    /// <summary>
    /// Helper class for drawing on the picture box of the test application.
    /// </summary>
    class IsoDraw
    {
        public IsoDraw()
        {
            this.gridImage = this.LoadBitmap("grid.png");
            this.gridImage.MakeTransparent(GRID_IMAGE_MASK);
            this.gridChangedImage = this.LoadBitmap("grid_changed.png");
            this.gridChangedImage.MakeTransparent(GRID_IMAGE_MASK);

            this.maskImages = new Dictionary<TerrainCombination, Bitmap>();
            this.maskImages.Add(TerrainCombination.AAAB, this.LoadBitmap("aaab.png"));
            this.maskImages.Add(TerrainCombination.AABA, this.LoadBitmap("aaba.png"));
            this.maskImages.Add(TerrainCombination.AABB, this.LoadBitmap("aabb.png"));
            this.maskImages.Add(TerrainCombination.ABAA, this.LoadBitmap("abaa.png"));
            this.maskImages.Add(TerrainCombination.ABAB, this.LoadBitmap("abab.png"));
            this.maskImages.Add(TerrainCombination.ABBA, this.LoadBitmap("abba.png"));
            this.maskImages.Add(TerrainCombination.ABBB, this.LoadBitmap("abbb.png"));
            this.maskImages.Add(TerrainCombination.BAAA, this.LoadBitmap("baaa.png"));
            this.maskImages.Add(TerrainCombination.BAAB, this.LoadBitmap("baab.png"));
            this.maskImages.Add(TerrainCombination.BABA, this.LoadBitmap("baba.png"));
            this.maskImages.Add(TerrainCombination.BABB, this.LoadBitmap("babb.png"));
            this.maskImages.Add(TerrainCombination.BBAA, this.LoadBitmap("bbaa.png"));
            this.maskImages.Add(TerrainCombination.BBAB, this.LoadBitmap("bbab.png"));
            this.maskImages.Add(TerrainCombination.BBBA, this.LoadBitmap("bbba.png"));
            this.maskImages.Add(TerrainCombination.Simple, this.LoadBitmap("simple.png"));

            this.tileImages = new Dictionary<TileType, Bitmap>();

            this.terrainColors = new Dictionary<string, Color>();
            this.terrainColors.Add("Blue", Color.Blue);
            this.terrainColors.Add("Green", Color.Green);
            this.terrainColors.Add("Cyan", Color.FromArgb(0, 255, 254));
            this.terrainColors.Add("Red", Color.Red);
            this.terrainColors.Add("Magenta", Color.FromArgb(255, 0, 255));
            this.terrainColors.Add("Brown", Color.Brown);
            this.terrainColors.Add("Gray", Color.Gray);
            this.terrainColors.Add("LightBlue", Color.LightBlue);
            this.terrainColors.Add("Yellow", Color.Yellow);
            this.terrainColors.Add("LightGreen", Color.LightGreen);
        }

        /// <summary>
        /// Draws the given isometric tile to the given device context.
        /// </summary>
        public void DrawTile(IIsoTile tile, bool isReplaced, Graphics g)
        {
            RCIntVector screenCoords = TILE_SIZE * (tile.MapCoords + new RCIntVector(1, 16));
            if (!this.tileImages.ContainsKey(tile.Type))
            {
                this.CreateTileImage(tile.Type);
            }

            g.DrawImageUnscaled(this.tileImages[tile.Type], screenCoords.X, screenCoords.Y);
            g.DrawImageUnscaled(isReplaced ? this.gridChangedImage : this.gridImage, screenCoords.X, screenCoords.Y);
        }

        /// <summary>
        /// Loads a bitmap from the given file.
        /// </summary>
        private Bitmap LoadBitmap(string filename)
        {
            Bitmap loadedBitmap = (Bitmap)Image.FromFile(filename);
            if (loadedBitmap.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new ArgumentException("Pixel format of the given Bitmap must be PixelFormat.Format24bppRgb",
                                            "filename");
            }
            if (loadedBitmap.Size.Width != TILE_SIZE || loadedBitmap.Size.Height != TILE_SIZE)
            {
                throw new ArgumentException(string.Format("Size of the given Bitmap must be {0}x{0}", TILE_SIZE),
                                            "filename");
            }
            return loadedBitmap;
        }

        /// <summary>
        /// Creates the image for the given type.
        /// </summary>
        private void CreateTileImage(TileType type)
        {
            Bitmap baseTerrainA = new Bitmap(TILE_SIZE, TILE_SIZE);
            Graphics baseTerrainAGraphics = Graphics.FromImage(baseTerrainA);
            baseTerrainAGraphics.Clear(this.terrainColors[type.TerrainA.Name]);
            this.maskImages[type.Combination].MakeTransparent(TERRAIN_A_MASK);
            baseTerrainAGraphics.DrawImageUnscaled(this.maskImages[type.Combination], 0, 0);

            if (type.Combination == TerrainCombination.Simple)
            {
                baseTerrainAGraphics.Dispose();
                this.tileImages.Add(type, baseTerrainA);
                return;
            }

            Bitmap baseTerrainB = new Bitmap(TILE_SIZE, TILE_SIZE);
            Graphics baseTerrainBGraphics = Graphics.FromImage(baseTerrainB);
            baseTerrainBGraphics.Clear(this.terrainColors[type.TerrainB.Name]);
            baseTerrainA.MakeTransparent(TERRAIN_B_MASK);
            baseTerrainBGraphics.DrawImageUnscaled(baseTerrainA, 0, 0);

            baseTerrainAGraphics.Dispose();
            baseTerrainBGraphics.Dispose();
            baseTerrainA.Dispose();

            this.tileImages.Add(type, baseTerrainB);
        }

        /// <summary>
        /// The mask images mapped by their combinations.
        /// </summary>
        private Dictionary<TerrainCombination, Bitmap> maskImages;

        /// <summary>
        /// The tile images mapped by the types.
        /// </summary>
        private Dictionary<TileType, Bitmap> tileImages;

        /// <summary>
        /// List of the terrain colors mapped by their names.
        /// </summary>
        private Dictionary<string, Color> terrainColors;

        /// <summary>
        /// The image that is used to draw the grid.
        /// </summary>
        private Bitmap gridImage;

        /// <summary>
        /// The image that is used to draw the grid in case of changed tiles.
        /// </summary>
        private Bitmap gridChangedImage;

        /// <summary>
        /// Mask colors.
        /// </summary>
        private readonly Color TERRAIN_A_MASK = Color.FromArgb(255, 0, 255);
        private readonly Color TERRAIN_B_MASK = Color.FromArgb(0, 255, 255);
        private readonly Color GRID_IMAGE_MASK = Color.FromArgb(255, 0, 255);

        /// <summary>
        /// Size of the tile images.
        /// </summary>
        private const int TILE_SIZE = 16;
    }
}
