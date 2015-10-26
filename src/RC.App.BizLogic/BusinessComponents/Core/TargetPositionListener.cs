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
                return CompletionResultEnum.FailedAndCancel;
            }

            if (this.sourceOfTypeToBePlaced == SourceOfTypeToBePlacedEnum.FromParameter)
            {
                /// TODO: Handle this case during the implementation of the Build command of SCV!
                return CompletionResultEnum.Succeeded;
            }
            else if (this.sourceOfTypeToBePlaced == SourceOfTypeToBePlacedEnum.CurrentSelection)
            {
                int[] currentSelection = this.selectionManagerBC.CurrentSelection.ToArray();
                if (currentSelection.Length != 1) { throw new InvalidOperationException("The number of the currently selected entities must be 1!"); }

                Entity selectedEntity = this.scenarioManagerBC.ActiveScenario.GetElement<Entity>(currentSelection[0]);
                if (selectedEntity == null) { throw new InvalidOperationException("The currently selected entity doesn't exist!"); }

                if (this.fogOfWarBC.CheckPlacementConstraints(selectedEntity, (RCIntVector)this.CommandBuilder.TargetPosition).Count == 0)
                {
                    return CommandInputListener.CompletionResultEnum.Succeeded;
                }
                else
                {
                    this.CommandBuilder.TargetPosition = RCNumVector.Undefined;
                    return CommandInputListener.CompletionResultEnum.FailedButContinue;
                }
            }
            else
            {
                /// TODO: check if the selected target position is valid!
                return CommandInputListener.CompletionResultEnum.Succeeded;
            }
        }

        /// <see cref="CommandInputListener.Init"/>
        protected override void Init(XElement listenerElem)
        {
            base.Init(listenerElem);

            XAttribute sourceOfTypeToBePlacedAttr = listenerElem.Attribute(SOURCE_OF_TYPE_TO_BE_PLACED_ATTR);
            this.sourceOfTypeToBePlaced = sourceOfTypeToBePlacedAttr != null
                                        ? EnumMap<SourceOfTypeToBePlacedEnum, string>.Demap(sourceOfTypeToBePlacedAttr.Value)
                                        : SourceOfTypeToBePlacedEnum.None;

            this.selectionManagerBC = ComponentManager.GetInterface<ISelectionManagerBC>();
            this.scenarioManagerBC = ComponentManager.GetInterface<IScenarioManagerBC>();
            this.fogOfWarBC = ComponentManager.GetInterface<IFogOfWarBC>();
        }

        #region ITargetPositionListener members

        /// <see cref="ITargetPositionListener.TypeToBePlaced"/>
        public string TypeToBePlaced
        {
            get
            {
                if (this.sourceOfTypeToBePlaced == SourceOfTypeToBePlacedEnum.CurrentSelection)
                {
                    int[] currentSelection = this.selectionManagerBC.CurrentSelection.ToArray();
                    if (currentSelection.Length != 1) { throw new InvalidOperationException( "The number of the currently selected entities must be 1!"); }

                    Entity selectedEntity = this.scenarioManagerBC.ActiveScenario.GetElement<Entity>(currentSelection[0]);
                    if (selectedEntity == null) { throw new InvalidOperationException( "The currently selected entity doesn't exist!"); }

                    return selectedEntity.ElementType.Name;
                }
                else if (this.sourceOfTypeToBePlaced == SourceOfTypeToBePlacedEnum.FromParameter)
                {
                    return this.CommandBuilder.Parameter;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <see cref="ITargetPositionListener.SelectTargetPosition"/>
        public void SelectTargetPosition(RCNumVector targetPosition)
        {
            if (this.sourceOfTypeToBePlaced == SourceOfTypeToBePlacedEnum.FromParameter)
            {
                /// TODO: Handle this case during the implementation of the Build command of SCV!
            }
            else if (this.sourceOfTypeToBePlaced == SourceOfTypeToBePlacedEnum.CurrentSelection)
            {
                int[] currentSelection = this.selectionManagerBC.CurrentSelection.ToArray();
                if (currentSelection.Length != 1) { throw new InvalidOperationException("The number of the currently selected entities must be 1!"); }

                Entity selectedEntity = this.scenarioManagerBC.ActiveScenario.GetElement<Entity>(currentSelection[0]);
                if (selectedEntity == null) { throw new InvalidOperationException("The currently selected entity doesn't exist!"); }

                IQuadTile quadTileAtPos = this.scenarioManagerBC.ActiveScenario.Map.GetCell(targetPosition.Round()).ParentQuadTile;
                RCIntVector objQuadSize = this.scenarioManagerBC.ActiveScenario.Map.CellToQuadSize(selectedEntity.ElementType.Area.Read());
                RCIntVector topLeftQuadCoords = quadTileAtPos.MapCoords - objQuadSize / 2;
                this.CommandBuilder.TargetPosition = topLeftQuadCoords;
            }
            else
            {
                this.CommandBuilder.TargetPosition = targetPosition;
            }
        }

        #endregion ITargetPositionListener members

        /// <summary>
        /// Enumerates the possible sources of the type to be placed by a target input listener.
        /// </summary>
        private enum SourceOfTypeToBePlacedEnum
        {
            [EnumMapping("None")]
            None = 0,               /// There is no type to be placed.
            [EnumMapping("FromParameter")]
            FromParameter = 1,      /// The type to be placed is coming from the parameter of the command being built.
            [EnumMapping("CurrentSelection")]
            CurrentSelection = 2    /// The type to be placed is the type of the currently selected entity.
        }

        /// <summary>
        /// The source of the type to be placed by this target position listener.
        /// </summary>
        private SourceOfTypeToBePlacedEnum sourceOfTypeToBePlaced;

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
        private const string SOURCE_OF_TYPE_TO_BE_PLACED_ATTR = "sourceOfTypeToBePlaced";
    }
}
