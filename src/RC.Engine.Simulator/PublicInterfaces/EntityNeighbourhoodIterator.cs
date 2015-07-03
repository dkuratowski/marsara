using System;
using System.Collections.Generic;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;
using RC.Engine.Simulator.Engine;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// This iterator iterates
    /// </summary>
    public class EntityNeighbourhoodIterator : CellIteratorBase
    {
        /// <summary>
        /// Constructs an EntityNeighbourhoodIterator instance.
        /// </summary>
        /// <param name="entity">The entity at the center of the iteration.</param>
        public EntityNeighbourhoodIterator(Entity entity)
            : base(entity.Scenario.Map)
        {
            this.centralEntity = entity;
        }

        #region Overrides from CellIteratorBase

        /// <see cref="CellIteratorBase.GetEnumeratorImpl"/>
        protected override IEnumerable<ScanOperation> ScanOperations
        {
            get
            {
                if (!this.centralEntity.HasMapObject) { throw new InvalidOperationException("The given entity is not placed on the map!"); }
                
                /// Calculate the initial processed area.
                RCIntVector bbTopLeft = this.centralEntity.Position.Location.Round();
                RCIntVector bbBottomRight = (this.centralEntity.Position.Location + this.centralEntity.Position.Size).Round();
                RCIntRectangle processedArea = new RCIntRectangle(bbTopLeft, (bbBottomRight - bbTopLeft) + new RCIntVector(1, 1));
                for (int layerIdx = 0; layerIdx < LAYER_COUNT; layerIdx++)
                {
                    /// Calculate the parameters of the areas to be scanned.
                    int agLeft = (processedArea.Left + processedArea.Right) / 2;
                    int cdTop = (processedArea.Top + processedArea.Bottom) / 2;
                    int agWidth = processedArea.Right - agLeft;
                    int bhWidth = agLeft - processedArea.Left;
                    int cdHeight = processedArea.Bottom + LAYER_THICKNESS - cdTop;
                    int efHeight = cdTop - (processedArea.Top - LAYER_THICKNESS);

                    /// Construct the scan operations for scanning the areas.
                    if (agWidth > 0)
                    {
                        yield return new ScanOperation
                        {
                            AffectedArea = new RCIntRectangle(agLeft, processedArea.Bottom, agWidth, LAYER_THICKNESS),
                            ScanMethod = CellIteratorBase.LeftRightTopDownScan
                        };
                    }
                    if (bhWidth > 0)
                    {
                        yield return new ScanOperation
                        {
                            AffectedArea = new RCIntRectangle(processedArea.Left, processedArea.Bottom, bhWidth, LAYER_THICKNESS),
                            ScanMethod = CellIteratorBase.RightLeftTopDownScan
                        };
                    }
                    if (cdHeight > 0)
                    {
                        yield return new ScanOperation
                        {
                            AffectedArea = new RCIntRectangle(processedArea.Right, cdTop, LAYER_THICKNESS, cdHeight),
                            ScanMethod = CellIteratorBase.BottomUpLeftRightScan
                        };
                        yield return new ScanOperation
                        {
                            AffectedArea = new RCIntRectangle(processedArea.Left - LAYER_THICKNESS, cdTop, LAYER_THICKNESS, cdHeight),
                            ScanMethod = CellIteratorBase.BottomUpRightLeftScan
                        };
                    }
                    if (efHeight > 0)
                    {
                        yield return new ScanOperation
                        {
                            AffectedArea = new RCIntRectangle(processedArea.Right, processedArea.Top - LAYER_THICKNESS, LAYER_THICKNESS, efHeight),
                            ScanMethod = CellIteratorBase.BottomUpLeftRightScan
                        };
                        yield return new ScanOperation
                        {
                            AffectedArea = new RCIntRectangle(processedArea.Left - LAYER_THICKNESS, processedArea.Top - LAYER_THICKNESS, LAYER_THICKNESS, efHeight),
                            ScanMethod = CellIteratorBase.BottomUpRightLeftScan
                        };
                    }
                    if (agWidth > 0)
                    {
                        yield return new ScanOperation
                        {
                            AffectedArea = new RCIntRectangle(agLeft, processedArea.Top - LAYER_THICKNESS, agWidth, LAYER_THICKNESS),
                            ScanMethod = CellIteratorBase.LeftRightBottomUpScan
                        };
                    }
                    if (bhWidth > 0)
                    {
                        yield return new ScanOperation
                        {
                            AffectedArea = new RCIntRectangle(processedArea.Left, processedArea.Top - LAYER_THICKNESS, bhWidth, LAYER_THICKNESS),
                            ScanMethod = CellIteratorBase.RightLeftBottomUpScan
                        };
                    }

                    /// Enlarge the processed area.
                    processedArea = new RCIntRectangle(processedArea.Left - LAYER_THICKNESS,
                                                       processedArea.Top - LAYER_THICKNESS,
                                                       processedArea.Width + 2 * LAYER_THICKNESS,
                                                       processedArea.Height + 2 * LAYER_THICKNESS);
                }
            }
        }

        #endregion Overrides from CellIteratorBase

        /// <summary>
        /// Reference to the entity at the center of the iteration.
        /// </summary>
        private Entity centralEntity;

        /// <summary>
        /// Constants to determine the scan operation list.
        /// </summary>
        private const int LAYER_THICKNESS = 3;
        private const int LAYER_COUNT = 2;
    }
}
