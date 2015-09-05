using RC.Engine.Simulator.Metadata;
using System;
using System.Collections.Generic;
using RC.Common.ComponentModel;
using RC.App.BizLogic.BusinessComponents;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Implementation of views on the RC engine metadata.
    /// </summary>
    class MetadataView : IMetadataView
    {
        /// <summary>
        /// Constructs a MetadataView instance.
        /// </summary>
        public MetadataView()
        {
            this.metadata = ComponentManager.GetInterface<IScenarioManagerBC>().Metadata;
        }

        #region IMetadataView members

        /// <see cref="IMetadataView.GetMapObjectSpriteDefs"/>
        public List<SpriteDef> GetMapObjectSpriteDefs()
        {
            List<SpriteDef> retList = new List<SpriteDef>();
            foreach (IScenarioElementType objType in this.metadata.AllTypes)
            {
                if (objType.SpritePalette != null)
                {
                    byte[] imageData = new byte[objType.SpritePalette.ImageData.Length];
                    Array.Copy(objType.SpritePalette.ImageData, imageData, objType.SpritePalette.ImageData.Length);
                    SpriteDef info = new SpriteDef();
                    info.ImageData = imageData;
                    info.TransparentColor = objType.SpritePalette.TransparentColor;
                    info.MaskColor = objType.SpritePalette.MaskColor;
                    info.IsMaskableSprite = objType.HasOwner;
                    retList.Add(info);
                }
            }
            return retList;
        }

        /// <see cref="IMetadataView.GetMapObjectHPIconDefs"/>
        public List<SpriteDef> GetMapObjectHPIconDefs()
        {
            List<SpriteDef> retList = new List<SpriteDef>();
            foreach (IScenarioElementType objType in this.metadata.AllTypes)
            {
                if (objType.HPIconPalette != null)
                {
                    byte[] imageData = new byte[objType.HPIconPalette.ImageData.Length];
                    Array.Copy(objType.HPIconPalette.ImageData, imageData, objType.HPIconPalette.ImageData.Length);
                    SpriteDef info = new SpriteDef();
                    info.ImageData = imageData;
                    info.TransparentColor = objType.HPIconPalette.TransparentColor;
                    info.MaskColor = objType.HPIconPalette.MaskColor;
                    info.IsMaskableSprite = objType.MaxHP != null;
                    retList.Add(info);
                }
            }
            return retList;
        }

        /// <see cref="IMetadataView.GetMapObjectDisplayedTypeNames"/>
        public Dictionary<int, string> GetMapObjectDisplayedTypeNames()
        {
            Dictionary<int, string> retList = new Dictionary<int, string>();

            foreach (IUnitType unitType in this.metadata.UnitTypes)
            {
                if (unitType.DisplayedName != null) { retList.Add(unitType.ID, unitType.DisplayedName); }
            }
            foreach (IBuildingType buildingType in this.metadata.BuildingTypes)
            {
                if (buildingType.DisplayedName != null) { retList.Add(buildingType.ID, buildingType.DisplayedName); }
            }
            foreach (IAddonType addonType in this.metadata.AddonTypes)
            {
                if (addonType.DisplayedName != null) { retList.Add(addonType.ID, addonType.DisplayedName); }
            }
            foreach (IScenarioElementType customType in this.metadata.CustomTypes)
            {
                if (customType.DisplayedName != null) { retList.Add(customType.ID, customType.DisplayedName); }
            }

            return retList;
        }

        #endregion IMetadataView members

        /// <summary>
        /// Reference to the RC engine metadata.
        /// </summary>
        private IScenarioMetadata metadata;
    }
}
