using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.ComponentInterfaces
{
    /// <summary>
    /// Component interface for creating, loading and saving navigation meshes.
    /// </summary>
    [ComponentInterface]
    public interface INavMeshLoader
    {
        /// <summary>
        /// Creates a new navigation mesh for the given walkability grid.
        /// </summary>
        /// <param name="walkabilityGrid">The walkability grid that the created navmesh is based on.</param>
        /// <returns>The created navmesh.</returns>
        /// <remarks>This is a long running process that might block the UI if called from the UI thread.</remarks>
        INavMesh NewNavMesh(IWalkabilityGrid walkabilityGrid);

        /// <summary>
        /// Loads a navigation mesh from the given byte array.
        /// </summary>
        /// <param name="data">The byte array to load from.</param>
        /// <returns>The loaded navmesh or null if the given byte array doesn't contain navmesh.</returns>
        INavMesh LoadNavMesh(byte[] data);

        /// <summary>
        /// Saves the given navigation mesh to a byte array.
        /// </summary>
        /// <param name="navmesh">The navmesh to be saved.</param>
        /// <returns>The byte array that contains the serialized navmesh.</returns>
        byte[] SaveNavMesh(INavMesh navmesh);

        /// <summary>
        /// Checks whether the given navmesh is based on the given walkability grid.
        /// </summary>
        /// <param name="gridToCheck">The walkability grid to check.</param>
        /// <param name="navmeshToCheck">The navmesh to check.</param>
        /// <returns>
        /// True if the given navmesh is based on the given walkability grid; otherwise false. See remarks for more informations!
        /// </returns>
        /// <remarks>
        /// As this integrity check is based on comparison of hash values, the following restrictions are present:
        ///     - If this method returns false then it is 100% sure that the given navmesh is not based on the given walkability grid.
        ///     - If this method returns true then it is VERY probable - but not 100% sure - that the given navemsh is based on the
        ///       given walkability grid.
        /// </remarks>
        bool CheckNavmeshIntegrity(IWalkabilityGrid gridToCheck, INavMesh navmeshToCheck);
    }
}
