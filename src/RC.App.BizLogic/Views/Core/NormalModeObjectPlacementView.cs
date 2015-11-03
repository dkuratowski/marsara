using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.BusinessComponents;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.App.BizLogic.Views.Core
{
    /// <summary>
    /// Implementation of object placement views for map objects in normal gameplay.
    /// </summary>
    class NormalModeObjectPlacementView : ObjectPlacementView, INormalModeMapObjectPlacementView
    {
        /// <summary>
        /// Constructs a NormalModeObjectPlacementView instance.
        /// </summary>
        public NormalModeObjectPlacementView()
        {
            this.fogOfWarBC = ComponentManager.GetInterface<IFogOfWarBC>();
            this.scenarioManagerBC = ComponentManager.GetInterface<IScenarioManagerBC>();
            this.commandManagerBC = ComponentManager.GetInterface<ICommandManagerBC>();
            this.selectionManagerBC = ComponentManager.GetInterface<ISelectionManagerBC>();

            this.buildingToBePlaced = null;
            this.buildingTypeToBePlaced = null;
            this.addonTypeToBePlaced = null;
            this.buildingPreviewAnimation = null;
            this.addonPreviewAnimation = null;
        }

        #region ObjectPlacementView overrides

        /// <see cref="ObjectPlacementView.StepPreviewAnimation"/>
        public override void StepPreviewAnimation()
        {
            this.UpdatePlacementData();

            if (this.buildingPreviewAnimation != null) { this.buildingPreviewAnimation.Step(); }
            if (this.addonPreviewAnimation != null) { this.addonPreviewAnimation.Step(); }
        }

        /// <see cref="ObjectPlacementView.CheckObjectConstraints"/>
        protected override RCSet<RCIntVector> CheckObjectConstraints(RCIntVector topLeftQuadCoords)
        {
            this.UpdatePlacementData();

            if (this.buildingToBePlaced != null)
            {
                return this.addonTypeToBePlaced != null
                    ? this.fogOfWarBC.CheckPlacementConstraints(this.buildingToBePlaced, topLeftQuadCoords, this.addonTypeToBePlaced)
                    : this.fogOfWarBC.CheckPlacementConstraints(this.buildingToBePlaced, topLeftQuadCoords);
            }
            else if (this.buildingTypeToBePlaced != null)
            {
                return this.addonTypeToBePlaced != null
                    ? this.fogOfWarBC.CheckPlacementConstraints(this.buildingTypeToBePlaced, topLeftQuadCoords, this.addonTypeToBePlaced)
                    : this.fogOfWarBC.CheckPlacementConstraints(this.buildingTypeToBePlaced, topLeftQuadCoords);
            }
            else
            {
                return new RCSet<RCIntVector>();
            }
        }

        /// <see cref="ObjectPlacementView.GetObjectRelativeQuadRectangles"/>
        protected override RCSet<Tuple<RCIntRectangle, SpriteRenderInfo[]>> GetObjectRelativeQuadRectangles()
        {
            this.UpdatePlacementData();

            RCSet<Tuple<RCIntRectangle, SpriteRenderInfo[]>> retList = new RCSet<Tuple<RCIntRectangle, SpriteRenderInfo[]>>();

            IBuildingType buildingType = this.buildingToBePlaced != null
                ? this.buildingToBePlaced.BuildingType
                : this.buildingTypeToBePlaced;
            if (buildingType == null) { return retList; }

            /// Calculate the building rectangle and get sprites from the building preview animation if exists.
            RCIntVector buildingQuadSize = this.Scenario.Map.CellToQuadSize(buildingType.Area.Read());
            RCIntRectangle buildingRelativeRect = new RCIntRectangle((-1) * buildingQuadSize / 2, buildingQuadSize);
            SpriteRenderInfo[] buildingSprites = new SpriteRenderInfo[0];
            if (this.buildingPreviewAnimation != null)
            {
                buildingSprites = new SpriteRenderInfo[this.buildingPreviewAnimation.CurrentFrame.Length];
                for (int i = 0; i < this.buildingPreviewAnimation.CurrentFrame.Length; i++)
                {
                    buildingSprites[i] = new SpriteRenderInfo()
                    {
                        SpriteGroup = SpriteGroupEnum.MapObjectSpriteGroup,
                        Index = buildingType.SpritePalette.Index,
                        DisplayCoords = buildingType.SpritePalette.GetOffset(this.buildingPreviewAnimation.CurrentFrame[i]),
                        Section = buildingType.SpritePalette.GetSection(this.buildingPreviewAnimation.CurrentFrame[i])
                    };
                }
            }
            retList.Add(Tuple.Create(buildingRelativeRect, buildingSprites));

            if (this.addonTypeToBePlaced != null)
            {
                /// Calculate the addon rectangle and get sprites from the addon preview animation if exists.
                RCIntVector addonQuadSize = this.Scenario.Map.CellToQuadSize(this.addonTypeToBePlaced.Area.Read());
                RCIntRectangle addonRelativeRect = new RCIntRectangle(
                    buildingRelativeRect.Location +
                    buildingType.GetRelativeAddonPosition(this.Scenario.Map, this.addonTypeToBePlaced),
                    addonQuadSize);
                SpriteRenderInfo[] addonSprites = new SpriteRenderInfo[0];
                if (this.addonPreviewAnimation != null)
                {
                    addonSprites = new SpriteRenderInfo[this.addonPreviewAnimation.CurrentFrame.Length];
                    for (int i = 0; i < this.addonPreviewAnimation.CurrentFrame.Length; i++)
                    {
                        addonSprites[i] = new SpriteRenderInfo()
                        {
                            SpriteGroup = SpriteGroupEnum.MapObjectSpriteGroup,
                            Index = this.addonTypeToBePlaced.SpritePalette.Index,
                            DisplayCoords = this.addonTypeToBePlaced.SpritePalette.GetOffset(this.addonPreviewAnimation.CurrentFrame[i]),
                            Section = this.addonTypeToBePlaced.SpritePalette.GetSection(this.addonPreviewAnimation.CurrentFrame[i])
                        };
                    }
                }
                retList.Add(Tuple.Create(addonRelativeRect, addonSprites));
            }
            return retList;
        }

        #endregion ObjectPlacementView overrides

        /// <summary>
        /// Gets the currently selected building.
        /// </summary>
        /// <returns>The currently selected building.</returns>
        private Building GetSelectedBuilding()
        {
            int[] currentSelection = this.selectionManagerBC.CurrentSelection.ToArray();
            if (currentSelection.Length != 1) { throw new InvalidOperationException("The number of the currently selected entities must be 1!"); }

            return this.scenarioManagerBC.ActiveScenario.GetElement<Building>(currentSelection[0]);
        }

        /// <summary>
        /// Updates the placement data based on the current state of the game engine.
        /// </summary>
        private void UpdatePlacementData()
        {
            Building newBuildingToBePlaced = null;
            IBuildingType newBuildingTypeToBePlaced = null;
            IAddonType newAddonTypeToBePlaced = null;
            if (this.commandManagerBC.IsWaitingForTargetPosition)
            {
                if (this.commandManagerBC.PlaceSelectedBuilding)
                {
                    newBuildingToBePlaced = this.GetSelectedBuilding();
                }
                else if (this.commandManagerBC.BuildingType != null)
                {
                    newBuildingTypeToBePlaced = this.scenarioManagerBC.Metadata.GetBuildingType(this.commandManagerBC.BuildingType);
                    if (newBuildingTypeToBePlaced == null) { throw new InvalidOperationException(string.Format("Building type '{0}' is not defined in the metadata!", this.commandManagerBC.BuildingType)); }
                }

                if (this.commandManagerBC.AddonType != null)
                {
                    newAddonTypeToBePlaced = this.scenarioManagerBC.Metadata.GetAddonType(this.commandManagerBC.AddonType);
                    if (newAddonTypeToBePlaced == null) { throw new InvalidOperationException(string.Format("Addon type '{0}' is not defined in the metadata!", this.commandManagerBC.AddonType)); }
                }
            }

            /// Check if new preview animations have to be created.
            bool createPreviewAnimations = false;
            if (newBuildingToBePlaced != this.buildingToBePlaced ||
                newBuildingTypeToBePlaced != this.buildingTypeToBePlaced ||
                newAddonTypeToBePlaced != this.addonTypeToBePlaced)
            {
                this.buildingToBePlaced = newBuildingToBePlaced;
                this.buildingTypeToBePlaced = newBuildingTypeToBePlaced;
                this.addonTypeToBePlaced = newAddonTypeToBePlaced;
                createPreviewAnimations = true;
            }

            /// Create the building and addon preview animations if necessary.
            if (createPreviewAnimations)
            {
                this.buildingPreviewAnimation = null;
                this.addonPreviewAnimation = null;

                if (this.buildingToBePlaced != null && this.buildingToBePlaced.ElementType.AnimationPalette != null)
                {
                    Animation previewAnimDef = this.buildingToBePlaced.ElementType.AnimationPalette.PreviewAnimation;
                    if (previewAnimDef != null) { this.buildingPreviewAnimation = new AnimationPlayer(previewAnimDef, new ConstValue<MapDirection>(MapDirection.NorthEast)); }
                }
                else if (this.buildingTypeToBePlaced != null && this.buildingTypeToBePlaced.AnimationPalette != null)
                {
                    Animation previewAnimDef = this.buildingTypeToBePlaced.AnimationPalette.PreviewAnimation;
                    if (previewAnimDef != null) { this.buildingPreviewAnimation = new AnimationPlayer(previewAnimDef, new ConstValue<MapDirection>(MapDirection.NorthEast)); }
                }

                if (this.addonTypeToBePlaced != null && this.addonTypeToBePlaced.AnimationPalette != null)
                {
                    Animation previewAnimDef = this.addonTypeToBePlaced.AnimationPalette.PreviewAnimation;
                    if (previewAnimDef != null) { this.addonPreviewAnimation = new AnimationPlayer(previewAnimDef, new ConstValue<MapDirection>(MapDirection.NorthEast)); }
                }
            }
        }

        /// <summary>
        /// Reference to the building to be placed.
        /// </summary>
        private Building buildingToBePlaced;

        /// <summary>
        /// Reference to the building type to be placed.
        /// </summary>
        private IBuildingType buildingTypeToBePlaced;

        /// <summary>
        /// Reference to the addon type to be placed.
        /// </summary>
        private IAddonType addonTypeToBePlaced;

        /// <summary>
        /// The preview animation of the building to be placed.
        /// </summary>
        private AnimationPlayer buildingPreviewAnimation;

        /// <summary>
        /// The preview animation of the addon to be placed.
        /// </summary>
        private AnimationPlayer addonPreviewAnimation;

        /// <summary>
        /// Reference to the Fog Of War business component.
        /// </summary>
        private readonly IFogOfWarBC fogOfWarBC;

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private readonly IScenarioManagerBC scenarioManagerBC;

        /// <summary>
        /// Reference to the command manager business component.
        /// </summary>
        private readonly ICommandManagerBC commandManagerBC;

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private readonly ISelectionManagerBC selectionManagerBC;
    }
}
