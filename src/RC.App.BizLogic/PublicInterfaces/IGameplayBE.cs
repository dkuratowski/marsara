using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.PublicInterfaces
{
    /// <summary>
    /// Interface to the gameplay backend component. The Gameplay page of the UI communicates directly with this interface.
    /// </summary>
    [ComponentInterface]
    public interface IGameplayBE
    {
        /// <summary>
        /// Creates a view on the terrain of the map of the currently running game.
        /// </summary>
        /// <returns>The view on the terrain of the map of the currently running game.</returns>
        /// <exception cref="InvalidOperationException">If there is no running game.</exception>
        IMapTerrainView CreateMapTerrainView();

        /// <summary>
        /// Creates a view on the tileset of the map of the currently running game.
        /// </summary>
        /// <returns>The view on the tileset of the map of the currently running game.</returns>
        /// <exception cref="InvalidOperationException">If there is no running game.</exception>
        ITileSetView CreateTileSetView();

        /// <summary>
        /// Creates a view on the objects of the map of the currently running game.
        /// </summary>
        /// <returns>The view on the objects of the map of the currently running game.</returns>
        /// <exception cref="InvalidOperationException">If there is no running game.</exception>
        IMapObjectView CreateMapObjectView();

        /// <summary>
        /// Creates a debug view on the map of the currently running game.
        /// </summary>
        /// <returns>The debug view on the map of the currently running game.</returns>
        /// <exception cref="InvalidOperationException">If there is no running game.</exception>
        IMapDebugView CreateMapDebugView();

        /// <summary>
        /// Temporary method for testing.
        /// </summary>
        /// TODO: remove this method when no longer necessary.
        void StartTestScenario();

        /// <summary>
        /// PROTOTYPE CODE
        /// Updates the simulation and all of its components. Later the simulation shall be executed from the DSS-thread.
        /// </summary>
        void UpdateSimulation();
    }
}
