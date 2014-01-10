using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.PublicInterfaces;
using RC.Engine.Simulator.Scenarios;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.App.BizLogic.Core
{
    /// <summary>
    /// Implementation of object placement views for map objects.
    /// </summary>
    class MapObjectPlacementView : ObjectPlacementView, IObjectPlacementView
    {
        /// <summary>
        /// Constructs a MapObjectPlacementView instance.
        /// </summary>
        /// <param name="objectType">Reference to the type of the map object being placed.</param>
        /// <param name="scenario">Reference to the scenario.</param>
        /// <param name="scheduler">
        /// Reference to a scheduler that will step the preview animation of the map object being placed.
        /// </param>
        public MapObjectPlacementView(IScenarioElementType objectType, Scenario scenario, Scheduler scheduler)
            : base(scenario.Map)
        {
            if (objectType == null) { throw new ArgumentNullException("objectType"); }
            if (scenario == null) { throw new ArgumentNullException("scenario"); }
            if (scheduler == null) { throw new ArgumentNullException("scheduler"); }

            this.scenario = scenario;
            this.objectType = objectType;
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
            return this.objectType.CheckConstraints(this.scenario, topLeftCoords);
        }

        /// <see cref="ObjectPlacementView.GetObjectQuadraticSize"/>
        protected override RCIntVector GetObjectQuadraticSize()
        {
            return this.scenario.Map.CellToQuadSize(this.objectType.Area.Read());
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
        /// Reference to the scenario.
        /// </summary>
        private Scenario scenario;

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
