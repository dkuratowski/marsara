using RC.Common;
using RC.Common.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Engine.Pathfinder.PublicInterfaces
{
    /// <summary>
    /// The public interface of the pathfinder component.
    /// </summary>
    [ComponentInterface]
    public interface IPathfinder
    {
        /// <summary>
        /// Initializes the pathfinder component with the given walkability informations.
        /// </summary>
        /// <param name="walkabilityReader">The reader that provides the walkability informations.</param>
        /// <param name="maxMovingObstacleSize">The maximum size of moving obstacles.</param>
        void Initialize(IWalkabilityReader walkabilityReader, int maxMovingObstacleSize);

        /// <summary>
        /// Places a moving obstacle onto the given layer of the pathfinding grid to the given position.
        /// </summary>
        /// <param name="layer">The layer of the pathfinding grid on which to place the moving obstacle.</param>
        /// <param name="position">The position of the top-left corner of the moving obstacle.</param>
        /// <param name="size">The size of the moving obstacle.</param>
        /// <param name="client">The client of the moving obstacle that will provide additional informations for the pathfinder component.</param>
        /// <returns>A reference to the moving obstacle or null if the placement failed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the given size is not greater than 0.
        /// If the given size exceeds the maximum size of moving obstacles with which the pathfinder component was initialized.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If the pathfinder component is not initialized.
        /// </exception>
        IMovingObstacle PlaceMovingObstacle(PathfindingLayerEnum layer, RCIntVector position, int size, IObstacleClient client);

        /// <summary>
        /// Places an obstacle onto the given layer of the pathfinding grid to the given area.
        /// </summary>
        /// <param name="layer">The layer of the pathfinding grid on which to place the obstacle.</param>
        /// <param name="area">The rectangular area of the obstacle.</param>
        /// <param name="client">The client of the placed obstacle that will provide additional informations for the pathfinder component.</param>
        /// <returns>A reference to the obstacle or null if the placement failed.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the pathfinder component is not initialized.
        /// </exception>
        IObstacle PlaceObstacle(PathfindingLayerEnum layer, RCIntRectangle area, IObstacleClient client);

        /// <summary>
        /// Removes the given obstacle from the pathfinding grid.
        /// </summary>
        /// <param name="obstacle">The obstacle to be removed.</param>
        /// <exception cref="InvalidOperationException">
        /// If the obstacle is not placed on the pathfinding grid.
        /// If the pathfinder component is not initialized.
        /// </exception>
        void RemoveObstacle(IObstacle obstacle);

        /// <summary>
        /// Updates the state of the pathfinder component.
        /// </summary>
        void Update();
    }
}
