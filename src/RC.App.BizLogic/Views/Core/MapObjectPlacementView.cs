using System;
using System.Collections.Generic;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;
using RC.Common.ComponentModel;
using RC.App.BizLogic.BusinessComponents;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Implementation of object placement views for map objects.
    /// </summary>
    class MapObjectPlacementView : ObjectPlacementView, IMapObjectPlacementView
    {
        /// <summary>
        /// Constructs a MapObjectPlacementView instance.
        /// </summary>
        /// <param name="objectTypeName">The name of the type of the map object being placed.</param>
        public MapObjectPlacementView(string objectTypeName)
        {
            if (objectTypeName == null) { throw new ArgumentNullException("objectTypeName"); }

            this.objectType = ComponentManager.GetInterface<IScenarioManagerBC>().Metadata.GetElementType(objectTypeName);
            if (this.objectType.AnimationPalette != null)
            {
                Animation previewAnimDef = this.objectType.AnimationPalette.PreviewAnimation;
                if (previewAnimDef != null) { this.previewAnimation = new AnimationPlayer(previewAnimDef, MapDirection.Undefined); }
            }
        }

        #region ObjectPlacementView overrides

        /// <see cref="ObjectPlacementView.StepPreviewAnimation"/>
        public override void StepPreviewAnimation()
        {
            if (this.previewAnimation != null) { this.previewAnimation.Step(); }
        }

        /// <see cref="ObjectPlacementView.CheckObjectConstraints"/>
        protected override HashSet<RCIntVector> CheckObjectConstraints(RCIntVector topLeftCoords)
        {
            return this.objectType.CheckConstraints(this.Scenario, topLeftCoords);
        }

        /// <see cref="ObjectPlacementView.GetObjectQuadraticSize"/>
        protected override RCIntVector GetObjectQuadraticSize()
        {
            return this.Scenario.Map.CellToQuadSize(this.objectType.Area.Read());
        }

        /// <see cref="ObjectPlacementView.GetObjectSprites"/>
        protected override List<SpriteInst> GetObjectSprites()
        {
            if (this.previewAnimation == null) { return new List<SpriteInst>(); }

            List<SpriteInst> retList = new List<SpriteInst>();
            foreach (int spriteIdx in this.previewAnimation.CurrentFrame)
            {
                retList.Add(new SpriteInst()
                {
                    Index = this.objectType.SpritePalette.Index,
                    DisplayCoords = this.objectType.SpritePalette.GetOffset(spriteIdx),
                    Section = this.objectType.SpritePalette.GetSection(spriteIdx)
                });
            }
            return retList;
        }

        #endregion ObjectPlacementView overrides

        /// <summary>
        /// Reference to the type of the map object being placed.
        /// </summary>
        private IScenarioElementType objectType;

        /// <summary>
        /// Reference to the preview animation of the map object being placed.
        /// </summary>
        private AnimationPlayer previewAnimation;
    }
}
