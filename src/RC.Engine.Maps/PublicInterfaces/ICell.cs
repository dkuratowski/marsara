using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Defines the interface of a cell on the map.
    /// </summary>
    public interface ICell
    {
        /// <summary>
        /// Gets the walkability flag of this cell.
        /// </summary>
        bool IsWalkable { get; }

        /// <summary>
        /// Gets the buildability flag of this cell.
        /// </summary>
        bool IsBuildable { get; }

        /// <summary>
        /// Gets the ground level of this cell.
        /// </summary>
        int GroundLevel { get; }

        /// <summary>
        /// Locks the data fields of the cell. Writing data after lock is not possible. If the cell data has already been locked,
        /// this function has no effect.
        /// </summary>
        void Lock();

        /// <summary>
        /// Changes the walkability flag of this cell.
        /// </summary>
        /// <param name="newVal">The new value of the walkability flag.</param>
        void ChangeWalkability(bool newVal);

        /// <summary>
        /// Changes the buildability flag of this cell.
        /// </summary>
        /// <param name="newVal">The new value of the buildability flag.</param>
        void ChangeBuildability(bool newVal);

        /// <summary>
        /// Changes the ground level of this cell.
        /// </summary>
        /// <param name="newVal">The new value of the ground level.</param>
        void ChangeGroundLevel(int newVal);

        /// <summary>
        /// Undos the last modification of the walkability flag.
        /// </summary>
        void UndoWalkabilityChange();

        /// <summary>
        /// Undos the last modification of the buildability flag.
        /// </summary>
        void UndoBuildabilityChange();

        /// <summary>
        /// Undos the last modification of the ground level.
        /// </summary>
        void UndoGroundLevelChange();

        /// <summary>
        /// Gets the quadratic tile that this cell belongs to.
        /// </summary>
        IQuadTile ParentQuadTile { get; }

        /// <summary>
        /// Gets the isometric tile that this cell belongs to.
        /// </summary>
        IIsoTile ParentIsoTile { get; }

        /// <summary>
        /// Gets the map coordinates of this cell.
        /// </summary>
        RCIntVector MapCoords { get; }
    }
}
