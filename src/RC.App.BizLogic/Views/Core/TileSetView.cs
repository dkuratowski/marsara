using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.App.BizLogic.Views.Core
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
        public List<SpriteDef> GetIsoTileTypes()
        {
            List<SpriteDef> retList = new List<SpriteDef>();
            foreach (IIsoTileVariant tile in this.tileset.TileVariants)
            {
                byte[] imageData = new byte[tile.ImageData.Length];
                Array.Copy(tile.ImageData, imageData, tile.ImageData.Length);
                SpriteDef info = new SpriteDef();
                info.ImageData = imageData;
                info.TransparentColorStr = tile.GetProperty(BizLogicConstants.TILEPROP_TRANSPARENTCOLOR);
                retList.Add(info);
            }
            return retList;
        }

        /// <see cref="ITileSetView.GetTerrainObjectTypes"/>
        public List<SpriteDef> GetTerrainObjectTypes()
        {
            List<SpriteDef> retList = new List<SpriteDef>();
            foreach (ITerrainObjectType terrainObjectType in this.tileset.TerrainObjectTypes)
            {
                byte[] imageData = new byte[terrainObjectType.ImageData.Length];
                Array.Copy(terrainObjectType.ImageData, imageData, terrainObjectType.ImageData.Length);
                SpriteDef info = new SpriteDef();
                info.ImageData = imageData;
                info.TransparentColorStr = terrainObjectType.GetProperty(BizLogicConstants.TERRAINOBJPROP_TRANSPARENTCOLOR);
                retList.Add(info);
            }
            return retList;
        }

        /// <see cref="ITileSetView.GetTerrainObjectTypeNames"/>
        public List<string> GetTerrainObjectTypeNames()
        {
            List<string> retList = new List<string>();
            foreach (ITerrainObjectType terrainObject in this.tileset.TerrainObjectTypes)
            {
                retList.Add(terrainObject.Name);
            }
            return retList;
        }

        /// <see cref="ITileSetView.GetTerrainTypeNames"/>
        public List<string> GetTerrainTypeNames()
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
