using RC.App.BizLogic.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// Implementation of views on the RC engine metadata.
    /// </summary>
    class MetadataView : IMetadataView
    {
        /// <summary>
        /// Constructs a MetadataView instance.
        /// </summary>
        /// <param name="metadata">The subject of this view.</param>
        public MetadataView(IScenarioMetadata metadata)
        {
            if (metadata == null) { throw new ArgumentNullException("metadata"); }
            this.metadata = metadata;
        }

        #region IMetadataView members

        /// <see cref="IMetadataView.GetMapObjectTypes"/>
        public List<MapSpriteType> GetMapObjectTypes()
        {
            List<MapSpriteType> retList = new List<MapSpriteType>();
            foreach (IUnitType unitType in this.metadata.UnitTypes)
            {
                if (unitType.SpritePalette != null)
                {
                    MapSpriteType info = this.CreateMapSpriteType(unitType);
                    info.HasPlayer = true;
                    retList.Add(info);
                }
            }

            foreach (IBuildingType buildingType in this.metadata.BuildingTypes)
            {
                if (buildingType.SpritePalette != null)
                {
                    MapSpriteType info = this.CreateMapSpriteType(buildingType);
                    info.HasPlayer = true;
                    retList.Add(info);
                }
            }

            foreach (IAddonType addonType in this.metadata.AddonTypes)
            {
                if (addonType.SpritePalette != null)
                {
                    MapSpriteType info = this.CreateMapSpriteType(addonType);
                    info.HasPlayer = true;
                    retList.Add(info);
                }
            }

            foreach (IScenarioElementType customType in this.metadata.CustomTypes)
            {
                if (customType.SpritePalette != null)
                {
                    MapSpriteType info = this.CreateMapSpriteType(customType);
                    info.HasPlayer = true;
                    retList.Add(info);
                }
            }

            foreach (IUpgradeType upgradeType in this.metadata.UpgradeTypes)
            {
                if (upgradeType.SpritePalette != null)
                {
                    MapSpriteType info = this.CreateMapSpriteType(upgradeType);
                    info.HasPlayer = false;
                    retList.Add(info);
                }
            }
            return retList;
        }

        #endregion IMetadataView members

        /// <summary>
        /// Creates the map sprite type out of the given object type.
        /// </summary>
        /// <param name="objType">The object type.</param>
        /// <returns>The created map sprite type.</returns>
        private MapSpriteType CreateMapSpriteType(IScenarioElementType objType)
        {
            byte[] imageData = new byte[objType.SpritePalette.ImageData.Length];
            Array.Copy(objType.SpritePalette.ImageData, imageData, objType.SpritePalette.ImageData.Length);
            MapSpriteType info = new MapSpriteType();
            info.ImageData = imageData;
            info.TransparentColorStr = objType.SpritePalette.TransparentColorStr;
            info.OwnerMaskColorStr = objType.SpritePalette.OwnerMaskColorStr;
            return info;
        }

        /// <summary>
        /// Reference to the RC engine metadata.
        /// </summary>
        private IScenarioMetadata metadata;
    }
}
