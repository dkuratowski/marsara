using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.Engine.PublicInterfaces;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// Implementation of views on tilesets.
    /// </summary>
    class TileSetView : ITileSetView
    {
        /// <summary>
        /// Constructs a TileSetView instance.
        /// </summary>
        /// <param name="tileset">The subject of this view.</param>
        public TileSetView(ITileSet tileset)
        {
            if (tileset == null) { throw new ArgumentNullException("tileset"); }
            this.tileset = tileset;
        }

        #region ITileSetView methods

        /// <see cref="ITileSetView.GetIsoTileTypes"/>
        public List<IsoTileTypeInfo> GetIsoTileTypes()
        {
            List<IsoTileTypeInfo> retList = new List<IsoTileTypeInfo>();
            foreach (IIsoTileVariant tile in this.tileset.TileVariants)
            {
                byte[] imageData = new byte[tile.ImageData.Length];
                Array.Copy(tile.ImageData, imageData, tile.ImageData.Length);
                IsoTileTypeInfo info = new IsoTileTypeInfo();
                info.ImageData = imageData;
                info.TransparentColorStr = tile.GetProperty(BizLogicConstants.TILEPROP_TRANSPARENTCOLOR);
                retList.Add(info);
            }
            return retList;
        }

        /// <see cref="ITileSetView.GetTerrainTypes"/>
        public List<string> GetTerrainTypes()
        {
            List<string> retList = new List<string>();
            foreach (ITerrainType terrain in this.tileset.TerrainTypes)
            {
                retList.Add(terrain.Name);
            }
            return retList;
        }

        #endregion ITileSetView methods

        /// <summary>
        /// The subject of this view.
        /// </summary>
        private ITileSet tileset;
    }
}
