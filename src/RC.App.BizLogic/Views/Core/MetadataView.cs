using RC.Engine.Simulator.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// <param name="metadata">The subject of this view.</param>
        public MetadataView(IScenarioMetadata metadata)
        {
            if (metadata == null) { throw new ArgumentNullException("metadata"); }
            this.metadata = metadata;
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
                    info.TransparentColorStr = objType.SpritePalette.TransparentColorStr;
                    info.MaskColorStr = objType.SpritePalette.OwnerMaskColorStr;
                    info.IsMaskableSprite = objType.HasOwner;
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
        private SpriteDef CreateMapSpriteType(IScenarioElementType objType)
        {
            byte[] imageData = new byte[objType.SpritePalette.ImageData.Length];
            Array.Copy(objType.SpritePalette.ImageData, imageData, objType.SpritePalette.ImageData.Length);
            SpriteDef info = new SpriteDef();
            info.ImageData = imageData;
            info.TransparentColorStr = objType.SpritePalette.TransparentColorStr;
            info.MaskColorStr = objType.SpritePalette.OwnerMaskColorStr;
            return info;
        }

        /// <summary>
        /// Reference to the RC engine metadata.
        /// </summary>
        private IScenarioMetadata metadata;
    }
}
