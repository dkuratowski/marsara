using System;
using System.Collections.Generic;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;
using RC.Common.ComponentModel;
using RC.App.BizLogic.BusinessComponents;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Implementation of object placement views for map objects in map editor.
    /// </summary>
    class MapEditorModeObjectPlacementView : ObjectPlacementView, IMapEditorModeObjectPlacementView
    {
        /// <summary>
        /// Constructs a MapEditorModeObjectPlacementView instance.
        /// </summary>
        /// <param name="objectTypeName">The name of the type of the map object being placed.</param>
        public MapEditorModeObjectPlacementView(string objectTypeName)
        {
            if (objectTypeName == null) { throw new ArgumentNullException("objectTypeName"); }

            this.fogOfWarBC = ComponentManager.GetInterface<IFogOfWarBC>();
            this.objectType = ComponentManager.GetInterface<IScenarioManagerBC>().Metadata.GetElementType(objectTypeName);
            if (this.objectType.AnimationPalette != null)
            {
                Animation previewAnimDef = this.objectType.AnimationPalette.PreviewAnimation;
                if (previewAnimDef != null) { this.previewAnimation = new AnimationPlayer(previewAnimDef, new ConstValue<MapDirection>(MapDirection.NorthEast)); }
            }
        }

        #region ObjectPlacementView overrides

        /// <see cref="ObjectPlacementView.StepPreviewAnimation"/>
        public override void StepPreviewAnimation()
        {
            if (this.previewAnimation != null) { this.previewAnimation.Step(); }
        }

        /// <see cref="ObjectPlacementView.CheckObjectConstraints"/>
        protected override RCSet<RCIntVector> CheckObjectConstraints(RCIntVector topLeftQuadCoords)
        {
            return this.fogOfWarBC.CheckPlacementConstraints(this.objectType, topLeftQuadCoords);
        }

        /// <see cref="ObjectPlacementView.GetObjectRelativeQuadRectangles"/>
        protected override RCSet<Tuple<RCIntRectangle, SpriteRenderInfo[]>> GetObjectRelativeQuadRectangles()
        {
            SpriteRenderInfo[] objectSprites = new SpriteRenderInfo[0];
            if (this.previewAnimation != null)
            {
                objectSprites = new SpriteRenderInfo[this.previewAnimation.CurrentFrame.Length];
                for (int i = 0; i < objectSprites.Length; i++)
                {
                    objectSprites[i] = new SpriteRenderInfo()
                    {
                        SpriteGroup = SpriteGroupEnum.MapObjectSpriteGroup,
                        Index = this.objectType.SpritePalette.Index,
                        DisplayCoords = this.objectType.SpritePalette.GetOffset(this.previewAnimation.CurrentFrame[i]),
                        Section = this.objectType.SpritePalette.GetSection(this.previewAnimation.CurrentFrame[i])
                    };
                }
            }

            RCIntVector objectQuadSize = this.Scenario.Map.CellToQuadSize(this.objectType.Area.Read());
            return new RCSet<Tuple<RCIntRectangle, SpriteRenderInfo[]>>
            {
                Tuple.Create(new RCIntRectangle(-1 * objectQuadSize / 2, objectQuadSize), objectSprites)
            };
        }

        #endregion ObjectPlacementView overrides

        /// <summary>
        /// Reference to the type of the map object being placed.
        /// </summary>
        private readonly IScenarioElementType objectType;

        /// <summary>
        /// Reference to the preview animation of the map object being placed.
        /// </summary>
        private readonly AnimationPlayer previewAnimation;

        /// <summary>
        /// Reference to the Fog Of War business component.
        /// </summary>
        private readonly IFogOfWarBC fogOfWarBC;
    }
}
