using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// This static class defines special methods used in the RC.Engine.Maps component but logically not part of any
    /// other classes or structures.
    /// </summary>
    static class MapHelper
    {
        /// <summary>This method is used to rotate the terrain combination of an isometric tile.</summary>
        /// <param name="combination">The original terrain combination to be rotated.</param>
        /// <param name="origDir">
        /// The original direction vector of the isometric tile.
        /// Can be one of the followings: MapDirection.NorthEast, MapDirection.SouthEast, MapDirection.SouthWest,
        /// MapDirection.NorthWest.
        /// </param>
        /// <param name="newDir">
        /// The new direction vector of the isometric tile.
        /// Can be one of the followings: MapDirection.NorthEast, MapDirection.SouthEast, MapDirection.SouthWest,
        /// MapDirection.NorthWest.
        /// </param>
        /// <returns>The rotated terrain combination of the tile.</returns>
        public static TerrainCombination RotateTerrainCombination(TerrainCombination combination, MapDirection origDir, MapDirection newDir)
        {
            /// No rotation has to be performed in case of simple terrain combination.
            if (combination == TerrainCombination.Simple) { return combination; }

            if (newDir != MapDirection.NorthEast)
            {
                int modifiedOrigDir = ((int)MapDirection.NorthEast - (int)newDir + (int)origDir) % 8;
                return MapHelper.RotateTerrainCombination(combination,
                                                               (MapDirection)(modifiedOrigDir >= 0 ? modifiedOrigDir : modifiedOrigDir + 8),
                                                               MapDirection.NorthEast);
            }

            if (origDir == MapDirection.NorthEast)
            {
                /// No rotation has to be performed as the direction vector already points to MapDirection.NorthEast.
                return combination;
            }
            else if (origDir == MapDirection.SouthEast)
            {
                /// Rotate by +90 degrees.
                if (combination == TerrainCombination.AAAB) { return TerrainCombination.AABA; }
                if (combination == TerrainCombination.AABA) { return TerrainCombination.ABAA; }
                if (combination == TerrainCombination.AABB) { return TerrainCombination.ABBA; }
                if (combination == TerrainCombination.ABAA) { return TerrainCombination.BAAA; }
                if (combination == TerrainCombination.ABAB) { return TerrainCombination.BABA; }
                if (combination == TerrainCombination.ABBA) { return TerrainCombination.BBAA; }
                if (combination == TerrainCombination.ABBB) { return TerrainCombination.BBBA; }
                if (combination == TerrainCombination.BAAA) { return TerrainCombination.AAAB; }
                if (combination == TerrainCombination.BAAB) { return TerrainCombination.AABB; }
                if (combination == TerrainCombination.BABA) { return TerrainCombination.ABAB; }
                if (combination == TerrainCombination.BABB) { return TerrainCombination.ABBB; }
                if (combination == TerrainCombination.BBAA) { return TerrainCombination.BAAB; }
                if (combination == TerrainCombination.BBAB) { return TerrainCombination.BABB; }
                if (combination == TerrainCombination.BBBA) { return TerrainCombination.BBAB; }
                throw new ArgumentException("Invalid terrain combination!", "combination");
            }
            else if (origDir == MapDirection.SouthWest)
            {
                /// Rotate by +180 degrees.
                if (combination == TerrainCombination.AAAB) { return TerrainCombination.ABAA; }
                if (combination == TerrainCombination.AABA) { return TerrainCombination.BAAA; }
                if (combination == TerrainCombination.AABB) { return TerrainCombination.BBAA; }
                if (combination == TerrainCombination.ABAA) { return TerrainCombination.AAAB; }
                if (combination == TerrainCombination.ABAB) { return TerrainCombination.ABAB; }
                if (combination == TerrainCombination.ABBA) { return TerrainCombination.BAAB; }
                if (combination == TerrainCombination.ABBB) { return TerrainCombination.BBAB; }
                if (combination == TerrainCombination.BAAA) { return TerrainCombination.AABA; }
                if (combination == TerrainCombination.BAAB) { return TerrainCombination.ABBA; }
                if (combination == TerrainCombination.BABA) { return TerrainCombination.BABA; }
                if (combination == TerrainCombination.BABB) { return TerrainCombination.BBBA; }
                if (combination == TerrainCombination.BBAA) { return TerrainCombination.AABB; }
                if (combination == TerrainCombination.BBAB) { return TerrainCombination.ABBB; }
                if (combination == TerrainCombination.BBBA) { return TerrainCombination.BABB; }
                throw new ArgumentException("Invalid terrain combination!", "combination");
            }
            else if (origDir == MapDirection.NorthWest)
            {
                /// Rotate by +270 degrees.
                if (combination == TerrainCombination.AAAB) { return TerrainCombination.BAAA; }
                if (combination == TerrainCombination.AABA) { return TerrainCombination.AAAB; }
                if (combination == TerrainCombination.AABB) { return TerrainCombination.BAAB; }
                if (combination == TerrainCombination.ABAA) { return TerrainCombination.AABA; }
                if (combination == TerrainCombination.ABAB) { return TerrainCombination.BABA; }
                if (combination == TerrainCombination.ABBA) { return TerrainCombination.AABB; }
                if (combination == TerrainCombination.ABBB) { return TerrainCombination.BABB; }
                if (combination == TerrainCombination.BAAA) { return TerrainCombination.ABAA; }
                if (combination == TerrainCombination.BAAB) { return TerrainCombination.BBAA; }
                if (combination == TerrainCombination.BABA) { return TerrainCombination.ABAB; }
                if (combination == TerrainCombination.BABB) { return TerrainCombination.BBAB; }
                if (combination == TerrainCombination.BBAA) { return TerrainCombination.ABBA; }
                if (combination == TerrainCombination.BBAB) { return TerrainCombination.BBBA; }
                if (combination == TerrainCombination.BBBA) { return TerrainCombination.ABBB; }
                throw new ArgumentException("Invalid terrain combination!", "combination");
            }
            else
            {
                throw new ArgumentException("Invalid direction vector!", "direction");
            }
        }

        /// <summary>
        /// Generates the terrain-NESW array for the given terrain types and their combination.
        /// </summary>
        /// <param name="a">The first terrain type.</param>
        /// <param name="b">The second terrain type or null in case of simple combination.</param>
        /// <param name="combination">The combination of the terrains.</param>
        /// <returns>
        /// A 4-long array that contains the terrain types in the north, east, south and west quarter (in this order).
        /// </returns>
        public static ITerrainType[] GetTerrainNESW(ITerrainType a, ITerrainType b, TerrainCombination combination)
        {
            if (a == null) { throw new ArgumentNullException("a"); }
            if (combination == TerrainCombination.Simple && b != null) { throw new ArgumentException("TerrainB must be null in case of simple tile type!", "b"); }
            if (combination != TerrainCombination.Simple && b == null) { throw new ArgumentException("TerrainB cannot be null in case of mixed tile type!", "b"); }

            if (combination == TerrainCombination.Simple) { return new ITerrainType[4] { a, a, a, a }; }
            if (combination == TerrainCombination.AAAB) { return new ITerrainType[4] { a, a, a, b }; }
            if (combination == TerrainCombination.AABA) { return new ITerrainType[4] { a, a, b, a }; }
            if (combination == TerrainCombination.AABB) { return new ITerrainType[4] { a, a, b, b }; }
            if (combination == TerrainCombination.ABAA) { return new ITerrainType[4] { a, b, a, a }; }
            if (combination == TerrainCombination.ABAB) { return new ITerrainType[4] { a, b, a, b }; }
            if (combination == TerrainCombination.ABBA) { return new ITerrainType[4] { a, b, b, a }; }
            if (combination == TerrainCombination.ABBB) { return new ITerrainType[4] { a, b, b, b }; }
            if (combination == TerrainCombination.BAAA) { return new ITerrainType[4] { b, a, a, a }; }
            if (combination == TerrainCombination.BAAB) { return new ITerrainType[4] { b, a, a, b }; }
            if (combination == TerrainCombination.BABA) { return new ITerrainType[4] { b, a, b, a }; }
            if (combination == TerrainCombination.BABB) { return new ITerrainType[4] { b, a, b, b }; }
            if (combination == TerrainCombination.BBAA) { return new ITerrainType[4] { b, b, a, a }; }
            if (combination == TerrainCombination.BBAB) { return new ITerrainType[4] { b, b, a, b }; }
            if (combination == TerrainCombination.BBBA) { return new ITerrainType[4] { b, b, b, a }; }
            throw new ArgumentException("Invalid terrain combination!", "combination");
        }
    }
}
