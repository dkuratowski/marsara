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

        /// <see cref="IMetadataView.GetMapObjectSpriteData"/>
        public List<SpriteData> GetMapObjectSpriteData()
        {
            List<SpriteData> retList = new List<SpriteData>();
            foreach (IScenarioElementType objType in this.metadata.AllTypes)
            {
                if (objType.SpritePalette != null)
                {
                    byte[] imageData = new byte[objType.SpritePalette.ImageData.Length];
                    Array.Copy(objType.SpritePalette.ImageData, imageData, objType.SpritePalette.ImageData.Length);
                    SpriteData info = new SpriteData();
                    info.ImageData = imageData;
                    info.TransparentColor = objType.SpritePalette.TransparentColor;
                    info.MaskColor = objType.SpritePalette.MaskColor;
                    info.IsMaskableSprite = objType.HasOwner;
                    retList.Add(info);
                }
            }
            return retList;
        }

        /// <see cref="IMetadataView.GetMapObjectHPIconData"/>
        public List<SpriteData> GetMapObjectHPIconData()
        {
            List<SpriteData> retList = new List<SpriteData>();
            foreach (IScenarioElementType objType in this.metadata.AllTypes)
            {
                if (objType.HPIconPalette != null)
                {
                    byte[] imageData = new byte[objType.HPIconPalette.ImageData.Length];
                    Array.Copy(objType.HPIconPalette.ImageData, imageData, objType.HPIconPalette.ImageData.Length);
                    SpriteData info = new SpriteData();
                    info.ImageData = imageData;
                    info.TransparentColor = objType.HPIconPalette.TransparentColor;
                    info.MaskColor = objType.HPIconPalette.MaskColor;
                    info.IsMaskableSprite = objType.MaxHP != null;
                    retList.Add(info);
                }
            }
            return retList;
        }

        /// <see cref="IMetadataView.GetShadowSpriteData"/>
        public List<SpriteData> GetShadowSpriteData()
        {
            List<SpriteData> retList = new List<SpriteData>();
            if (this.metadata.ShadowPalette != null)
            {
                byte[] imageData = new byte[this.metadata.ShadowPalette.ImageData.Length];
                Array.Copy(this.metadata.ShadowPalette.ImageData, imageData, this.metadata.ShadowPalette.ImageData.Length);
                SpriteData info = new SpriteData();
                info.ImageData = imageData;
                info.TransparentColor = this.metadata.ShadowPalette.TransparentColor;
                info.MaskColor = this.metadata.ShadowPalette.MaskColor;
                info.IsMaskableSprite = false;
                retList.Add(info);
            }
            return retList;
        }

        /// <see cref="IMetadataView.GetMapObjectDisplayedTypeNames"/>
        public Dictionary<int, string> GetMapObjectDisplayedTypeNames()
        {
            Dictionary<int, string> retList = new Dictionary<int, string>();
            foreach (IScenarioElementType elementType in this.metadata.AllTypes)
            {
                if (elementType.DisplayedName != null) { retList.Add(elementType.ID, elementType.DisplayedName); }
            }
            return retList;
        }

        /// <see cref="IMetadataView.GetWeaponDisplayedNames"/>
        public Dictionary<string, string> GetWeaponDisplayedNames()
        {
            Dictionary<string, string> retList = new Dictionary<string, string>();
            foreach (IScenarioElementType elementType in this.metadata.AllTypes)
            {
                foreach (IWeaponData weaponData in elementType.StandardWeapons)
                {
                    if (weaponData.DisplayedName != null) { retList.Add(weaponData.Name, weaponData.DisplayedName); }
                }
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
