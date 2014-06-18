using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.Scenarios;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;
using RC.App.BizLogic.BusinessComponents.Core;
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
        /// <param name="scheduler">
        /// Reference to a scheduler that will step the preview animation of the map object being placed.
        /// </param>
        public MapObjectPlacementView(string objectTypeName, Scheduler scheduler)
        {
            if (objectTypeName == null) { throw new ArgumentNullException("objectTypeName"); }
            if (scheduler == null) { throw new ArgumentNullException("scheduler"); }

            this.objectType = ComponentManager.GetInterface<IScenarioManagerBC>().Metadata.GetElementType(objectTypeName);
            this.scheduler = scheduler;
            if (this.objectType.AnimationPalette != null)
            {
                Animation previewAnimDef = this.objectType.AnimationPalette.PreviewAnimation;
                if (previewAnimDef != null)
                {
                    this.previewAnimation = new AnimationPlayer(previewAnimDef, MapDirection.Undefined);
                    this.scheduler.AddScheduledFunction(this.previewAnimation.Step);
                }
            }
        }

        #region ObjectPlacementView overrides

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

        /// <see cref="IDisposable.Dispose"/>
        public override void Dispose()
        {
            if (this.previewAnimation != null)
            {
                this.scheduler.RemoveScheduledFunction(this.previewAnimation.Step);
            }
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

        /// <summary>
        /// Reference to a scheduler that will step the preview animation of the map object being placed. 
        /// </summary>
        private Scheduler scheduler;
    }
}
