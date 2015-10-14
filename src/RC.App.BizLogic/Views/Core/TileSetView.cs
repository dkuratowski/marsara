using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents;
using RC.App.BizLogic.BusinessComponents.Core;
using RC.Common.ComponentModel;
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
        public TileSetView()
        {
            this.tileset = ComponentManager.GetInterface<IScenarioManagerBC>().ActiveScenario.Map.Tileset;
        }

        #region ITileSetView methods

        /// <see cref="ITileSetView.GetIsoTileTypes"/>
        public List<SpriteData> GetIsoTileTypes()
        {
            List<SpriteData> retList = new List<SpriteData>();
            foreach (IIsoTileVariant tile in this.tileset.TileVariants)
            {
                byte[] imageData = new byte[tile.ImageData.Length];
                Array.Copy(tile.ImageData, imageData, tile.ImageData.Length);
                SpriteData info = new SpriteData();
                info.ImageData = imageData;
                info.TransparentColor = tile.TransparentColor;
                retList.Add(info);
            }
            return retList;
        }

        /// <see cref="ITileSetView.GetTerrainObjectTypes"/>
        public List<SpriteData> GetTerrainObjectTypes()
        {
            List<SpriteData> retList = new List<SpriteData>();
            foreach (ITerrainObjectType terrainObjectType in this.tileset.TerrainObjectTypes)
            {
                byte[] imageData = new byte[terrainObjectType.ImageData.Length];
                Array.Copy(terrainObjectType.ImageData, imageData, terrainObjectType.ImageData.Length);
                SpriteData info = new SpriteData();
                info.ImageData = imageData;
                info.TransparentColor = terrainObjectType.TransparentColor;
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
