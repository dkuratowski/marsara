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

        /// <see cref="IMetadataView.GetMapObjectTypes"/>
        public List<SpriteDef> GetMapObjectTypes()
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

        #endregion IMetadataView members

        /// <summary>
        /// Reference to the RC engine metadata.
        /// </summary>
        private IScenarioMetadata metadata;
    }
}
