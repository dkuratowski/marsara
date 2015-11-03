using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Configuration;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;
using RC.Engine.Simulator.Metadata;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    /// <summary>
    /// Represents a command input listener that receives target position from the presentation layer.
    /// </summary>
    class TargetPositionListener : CommandInputListener, ITargetPositionListener
    {
        /// <see cref="CommandInputListener.TryComplete"/>
        public override CommandInputListener.CompletionResultEnum TryComplete()
        {
            if (this.CommandBuilder.TargetPosition == RCNumVector.Undefined)
            {
                /// No target position selected -> completion failed!
                return CompletionResultEnum.FailedAndCancel;
            }

            if (this.placeSelectedBuilding)
            {
                /// Target position selected for the selected building -> validate the selected position.
                int[] currentSelection = this.selectionManagerBC.CurrentSelection.ToArray();
                if (currentSelection.Length != 1) { throw new InvalidOperationException("The number of the currently selected entities must be 1!"); }

                Building selectedBuilding = this.scenarioManagerBC.ActiveScenario.GetElement<Building>(currentSelection[0]);
                if (selectedBuilding == null) { throw new InvalidOperationException("The currently selected entity doesn't exist or is not a building!"); }

                if (this.addonTypeName == null)
                {
                    /// There is no additional addon type.
                    if (this.fogOfWarBC.CheckPlacementConstraints(selectedBuilding, (RCIntVector)this.CommandBuilder.TargetPosition).Count == 0)
                    {
                        /// Selected position is OK.
                        return CommandInputListener.CompletionResultEnum.Succeeded;
                    }
                    else
                    {
                        /// Selected position is invalid -> completion failed!
                        this.CommandBuilder.TargetPosition = RCNumVector.Undefined;
                        return CommandInputListener.CompletionResultEnum.FailedButContinue;
                    }
                }
                else
                {
                    /// Additional addon type has to be checked as well.
                    IAddonType addonType = this.scenarioManagerBC.Metadata.GetAddonType(this.addonTypeName);
                    if (addonType == null) { throw new InvalidOperationException(string.Format("Addon type '{0}' is not defined in the metadata!", this.addonTypeName)); }
                    if (this.fogOfWarBC.CheckPlacementConstraints(selectedBuilding, (RCIntVector)this.CommandBuilder.TargetPosition, addonType).Count == 0)
                    {
                        /// Selected position is OK.
                        return CommandInputListener.CompletionResultEnum.Succeeded;
                    }
                    else
                    {
                        /// Selected position is invalid -> completion failed!
                        this.CommandBuilder.TargetPosition = RCNumVector.Undefined;
                        return CommandInputListener.CompletionResultEnum.FailedButContinue;
                    }
                }
            }
            else if (this.buildingTypeName != null)
            {
                /// Target position selected for a given building type -> validate the selected position.
                IBuildingType buildingType = this.scenarioManagerBC.Metadata.GetBuildingType(this.buildingTypeName);
                if (buildingType == null) { throw new InvalidOperationException(string.Format("Building type '{0}' is not defined in the metadata!", this.buildingTypeName)); }

                if (this.addonTypeName == null)
                {
                    if (this.fogOfWarBC.CheckPlacementConstraints(buildingType, (RCIntVector) this.CommandBuilder.TargetPosition).Count == 0)
                    {
                        /// Selected position is OK.
                        return CompletionResultEnum.Succeeded;
                    }
                    else
                    {
                        /// Selected position is invalid -> completion failed!
                        this.CommandBuilder.TargetPosition = RCNumVector.Undefined;
                        return CompletionResultEnum.FailedButContinue;
                    }
                }
                else
                {
                    /// Additional addon type has to be checked as well.
                    IAddonType addonType = this.scenarioManagerBC.Metadata.GetAddonType(this.addonTypeName);
                    if (addonType == null) { throw new InvalidOperationException(string.Format("Addon type '{0}' is not defined in the metadata!", this.addonTypeName)); }
                    if (this.fogOfWarBC.CheckPlacementConstraints(buildingType, (RCIntVector)this.CommandBuilder.TargetPosition, addonType).Count == 0)
                    {
                        /// Selected position is OK.
                        return CommandInputListener.CompletionResultEnum.Succeeded;
                    }
                    else
                    {
                        /// Selected position is invalid -> completion failed!
                        this.CommandBuilder.TargetPosition = RCNumVector.Undefined;
                        return CommandInputListener.CompletionResultEnum.FailedButContinue;
                    }
                }
            }
            else
            {
                /// A point selected on the map -> validate the selected position.
                return CommandInputListener.CompletionResultEnum.Succeeded;
            }
        }

        /// <see cref="CommandInputListener.Init"/>
        protected override void Init(XElement listenerElem)
        {
            base.Init(listenerElem);

            XElement currentSelectionElem = listenerElem.Element(CURRENT_SELECTION_ELEM);
            if (currentSelectionElem != null) { this.placeSelectedBuilding = true; }

            XElement buildingTypeElem = listenerElem.Element(BUILDING_TYPE_ELEM);
            if (buildingTypeElem != null)
            {
                if (this.placeSelectedBuilding) { throw new InvalidOperationException("Placing a given building type and the selected building is not possible at the same time!"); }
                XAttribute typeNameAttr = buildingTypeElem.Attribute(TYPE_NAME_ATTR);
                if (typeNameAttr == null) { throw new InvalidOperationException("Building type name not defined for a target position listener used for placing a building type!"); }
                this.buildingTypeName = typeNameAttr.Value;
            }

            XElement addonTypeElem = listenerElem.Element(ADDON_TYPE_ELEM);
            if (addonTypeElem != null)
            {
                if (!this.placeSelectedBuilding && this.buildingTypeName == null) { throw new InvalidOperationException("Addon type can only be placed together with a building!"); }
                XAttribute typeNameAttr = addonTypeElem.Attribute(TYPE_NAME_ATTR);
                if (typeNameAttr == null) { throw new InvalidOperationException("Addon type name not defined for a target position listener used for placing an addon type!"); }
                this.addonTypeName = typeNameAttr.Value;
            }

            this.selectionManagerBC = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.scenarioManagerBC = ComponentManager.GetInterface<IScenarioManagerBC>();
            this.fogOfWarBC = ComponentManager.GetInterface<IFogOfWarBC>();
        }

        #region ITargetPositionListener members

        /// <see cref="ITargetPositionListener.PlaceSelectedBuilding"/>
        public bool PlaceSelectedBuilding { get { return this.placeSelectedBuilding; } }

        /// <see cref="ITargetPositionListener.BuildingType"/>
        public string BuildingType { get { return this.buildingTypeName; } }

        /// <see cref="ITargetPositionListener.AddonType"/>
        public string AddonType { get { return this.addonTypeName; } }

        /// <see cref="ITargetPositionListener.SelectTargetPosition"/>
        public void SelectTargetPosition(RCNumVector targetPosition)
        {
            if (this.placeSelectedBuilding)
            {
                /// A position for the current selection is being selected.
                int[] currentSelection = this.selectionManagerBC.CurrentSelection.ToArray();
                if (currentSelection.Length != 1) { throw new InvalidOperationException("The number of the currently selected entities must be 1!"); }

                Building selectedBuilding = this.scenarioManagerBC.ActiveScenario.GetElement<Building>(currentSelection[0]);
                if (selectedBuilding == null) { throw new InvalidOperationException("The currently selected entity doesn't exist or is not a building!"); }

                IQuadTile quadTileAtPos = this.scenarioManagerBC.ActiveScenario.Map.GetCell(targetPosition.Round()).ParentQuadTile;
                RCIntVector objQuadSize = this.scenarioManagerBC.ActiveScenario.Map.CellToQuadSize(selectedBuilding.ElementType.Area.Read());
                RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - objQuadSize / 2;
                this.CommandBuilder.TargetPosition = topLeftQuadCoords;
            }
            else if (this.buildingTypeName != null)
            {
                /// A position for a building type is being selected.
                IBuildingType buildingType = this.scenarioManagerBC.Metadata.GetBuildingType(this.buildingTypeName);
                if (buildingType == null) { throw new InvalidOperationException(string.Format("Building type '{0}' is not defined in the metadata!", this.buildingTypeName)); }

                IQuadTile quadTileAtPos = this.scenarioManagerBC.ActiveScenario.Map.GetCell(targetPosition.Round()).ParentQuadTile;
                RCIntVector objQuadSize = this.scenarioManagerBC.ActiveScenario.Map.CellToQuadSize(buildingType.Area.Read());
                RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - objQuadSize / 2;
                this.CommandBuilder.TargetPosition = topLeftQuadCoords;
            }
            else
            {
                /// A point on the map is being selected.
                this.CommandBuilder.TargetPosition = targetPosition;
            }
        }

        #endregion ITargetPositionListener members

        /// <summary>
        /// True if the selected building has to be placed, false otherwise.
        /// </summary>
        private bool placeSelectedBuilding;

        /// <summary>
        /// The name of the building type to be placed or null if there is no building type to be placed or if the selected building has to be placed.
        /// </summary>
        private string buildingTypeName;

        /// <summary>
        /// The name of the addon type to be placed together with the appropriate building or null if there is no addon type to be placed.
        /// </summary>
        private string addonTypeName;

        /// <summary>
        /// Reference to the selection manager business component.
        /// </summary>
        private ISelectionManagerBC selectionManagerBC;

        /// <summary>
        /// Reference to the scenario manager business component.
        /// </summary>
        private IScenarioManagerBC scenarioManagerBC;

        /// <summary>
        /// Reference to the Fog Of War business component.
        /// </summary>
        private IFogOfWarBC fogOfWarBC;

        /// <summary>
        /// The supported XML-nodes and attributes.
        /// </summary>
        private const string CURRENT_SELECTION_ELEM = "currentSelection";
        private const string BUILDING_TYPE_ELEM = "buildingType";
        private const string ADDON_TYPE_ELEM = "addonType";
        private const string TYPE_NAME_ATTR = "name";
    }
}
