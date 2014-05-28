using System;
using System.Collections;
using System.Collections.Generic;
using RC.Common;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Base class of cell iterators. A cell iterator is used to visit a subset of cells of a map in an order
    /// determined by the derived classes.
    /// </summary>
    public abstract class CellIteratorBase : IEnumerable<ICell>
    {
        #region Predefined scan-methods

        /// <summary>
        /// LeftRightTopDown scan-method implementation.
        /// </summary>
        public static IEnumerable<RCIntVector> LeftRightTopDownScan(RCIntRectangle area)
        {
            for (int row = area.Top; row < area.Bottom; row++)
            {
                for (int col = area.Left; col < area.Right; col++)
                {
                    yield return new RCIntVector(col, row);
                }
            }
        }

        /// <summary>
        /// LeftRightBottomUp scan-method implementation.
        /// </summary>
        public static IEnumerable<RCIntVector> LeftRightBottomUpScan(RCIntRectangle area)
        {
            for (int row = area.Bottom - 1; row >= area.Top; row--)
            {
                for (int col = area.Left; col < area.Right; col++)
                {
                    yield return new RCIntVector(col, row);
                }
            }
        }

        /// <summary>
        /// RightLeftTopDown scan-method implementation.
        /// </summary>
        public static IEnumerable<RCIntVector> RightLeftTopDownScan(RCIntRectangle area)
        {
            for (int row = area.Top; row < area.Bottom; row++)
            {
                for (int col = area.Right - 1; col >= area.Left; col--)
                {
                    yield return new RCIntVector(col, row);
                }
            }
        }

        /// <summary>
        /// RightLeftBottomUp scan-method implementation.
        /// </summary>
        public static IEnumerable<RCIntVector> RightLeftBottomUpScan(RCIntRectangle area)
        {
            for (int row = area.Bottom - 1; row >= area.Top; row--)
            {
                for (int col = area.Right - 1; col >= area.Left; col--)
                {
                    yield return new RCIntVector(col, row);
                }
            }
        }

        /// <summary>
        /// TopDownLeftRight scan-method implementation.
        /// </summary>
        public static IEnumerable<RCIntVector> TopDownLeftRightScan(RCIntRectangle area)
        {
            for (int col = area.Left; col < area.Right; col++)
            {
                for (int row = area.Top; row < area.Bottom; row++)
                {
                    yield return new RCIntVector(col, row);
                }
            }
        }

        /// <summary>
        /// TopDownRightLeft scan-method implementation.
        /// </summary>
        public static IEnumerable<RCIntVector> TopDownRightLeftScan(RCIntRectangle area)
        {
            for (int col = area.Right - 1; col >= area.Left; col--)
            {
                for (int row = area.Top; row < area.Bottom; row++)
                {
                    yield return new RCIntVector(col, row);
                }
            }
        }

        /// <summary>
        /// BottomUpLeftRight scan-method implementation.
        /// </summary>
        public static IEnumerable<RCIntVector> BottomUpLeftRightScan(RCIntRectangle area)
        {
            for (int col = area.Left; col < area.Right; col++)
            {
                for (int row = area.Bottom - 1; row >= area.Top; row--)
                {
                    yield return new RCIntVector(col, row);
                }
            }
        }

        /// <summary>
        /// BottomUpRightLeft scan-method implementation.
        /// </summary>
        public static IEnumerable<RCIntVector> BottomUpRightLeftScan(RCIntRectangle area)
        {
            for (int col = area.Right - 1; col >= area.Left; col--)
            {
                for (int row = area.Bottom - 1; row >= area.Top; row--)
                {
                    yield return new RCIntVector(col, row);
                }
            }
        }

        #endregion Predefined scan-methods

        #region IEnumerable<T> members

        /// <see cref="IEnumerable<T>.GetEnumerator"/>
        public IEnumerator<ICell> GetEnumerator() { return this.GetEnumeratorImpl(); }

        /// <see cref="IEnumerable.GetEnumerator"/>
        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumeratorImpl(); }

        #endregion IEnumerable<T> members

        #region Protected members for the derived classes

        /// <summary>
        /// Defines a scan operation.
        /// </summary>
        protected struct ScanOperation
        {
            /// <summary>
            /// The rectangular area of cells on the map affected by the scan operation.
            /// </summary>
            public RCIntRectangle AffectedArea;

            /// <summary>
            /// The scan method that visits the cells of the affected area.
            /// </summary>
            public Func<RCIntRectangle, IEnumerable<RCIntVector>> ScanMethod;
        }

        /// <summary>
        /// Constructs a CellIteratorBase instance.
        /// </summary>
        /// <param name="map">The map whose cells need to be visited.</param>
        protected CellIteratorBase(IMapAccess map)
        {
            if (map == null) { throw new ArgumentNullException("map"); }

            this.map = map;
        }

        #endregion Protected members for the derived classes

        #region Overridable methods

        /// <summary>
        /// Gets the list of scan operations to be executed. This property must be overriden in the derived classes.
        /// </summary>
        protected abstract IEnumerable<ScanOperation> ScanOperations { get; }

        #endregion Overridable methods

        #region Private methods

        /// <summary>
        /// The method that actually implements the iteration strategy.
        /// </summary>
        private IEnumerator<ICell> GetEnumeratorImpl()
        {
            RCIntRectangle mapRect = new RCIntRectangle(new RCIntVector(0, 0), this.map.CellSize);
            foreach (ScanOperation scanOp in this.ScanOperations)
            {
                foreach (RCIntVector cellCoords in scanOp.ScanMethod(scanOp.AffectedArea))
                {
                    if (mapRect.Contains(cellCoords))
                    {
                        yield return this.map.GetCell(cellCoords);
                    }
                }
            }
        }

        #endregion Private methods

        /// <summary>
        /// Reference to the visited map.
        /// </summary>
        private IMapAccess map;
    }
}
