using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Maps.PublicInterfaces
{
    /// <summary>
    /// Defines the interface of a tileset.
    /// </summary>
    public interface ITileSet
    {
        /// <summary>
        /// Gets the index of the given field in the cell-data declaration of this tileset.
        /// </summary>
        /// <param name="nameOfField">The name of the field.</param>
        /// <returns>The index of the given field.</returns>
        /// <exception cref="TileSetException">If no field was declared with the given name.</exception>
        int GetCellDataFieldIndex(string nameOfField);

        /// <summary>
        /// Gets the name of the given field in the cell-data declaration of this tileset.
        /// </summary>
        /// <param name="index">The index of the field.</param>
        /// <returns>The name of the field.</returns>
        /// <exception cref="TileSetException">If no field was declared with the given index.</exception>
        string GetCellDataFieldName(int index);

        /// <summary>
        /// Gets the type of the given field in the cell-data declaration of this tileset.
        /// </summary>
        /// <param name="index">The index of the field.</param>
        /// <returns>The type of the field.</returns>
        /// <exception cref="TileSetException">If no field was declared with the given index.</exception>
        CellDataType GetCellDataFieldType(int index);

        /// <summary>
        /// Gets the terrain type of this tileset with the given name.
        /// </summary>
        /// <param name="name">The name of the terrain type.</param>
        /// <returns>The terrain type with the given name.</returns>
        ITerrainType GetTerrainType(string name);

        /// <summary>
        /// Gets the simple isometric tile type defined for the given terrain type.
        /// </summary>
        /// <param name="terrainType">The name of the terrain type.</param>
        /// <returns>The isometric tile type defined for the given terrain type.</returns>
        IIsoTileType GetIsoTileType(string terrainType);

        /// <summary>
        /// Gets the mixed isometric tile type defined for the given terrain types and combination.
        /// </summary>
        /// <param name="terrainTypeA">The first terrain type.</param>
        /// <param name="terrainTypeB">The second terrain type.</param>
        /// <param name="combination">The combination of the terrain types.</param>
        /// <returns>The ismetric tile type defined for the given terrain types and combination.</returns>
        /// <remarks>Terrain type A must be the parent of terrain type B.</remarks>
        IIsoTileType GetIsoTileType(string terrainTypeA, string terrainTypeB, TerrainCombination combination);

        /// <summary>
        /// Gets the terrain object type of this tileset with the given name.
        /// </summary>
        /// <param name="name">The name of the terrain object type.</param>
        /// <returns>The terrain object type with the given name.</returns>
        ITerrainObjectType GetTerrainObjectType(string name);

        /// <summary>
        /// Gets the list of all terrain types defined in this tileset.
        /// </summary>
        IEnumerable<ITerrainType> TerrainTypes { get; }

        /// <summary>
        /// Gets the list of all terrain object types defined in this tileset.
        /// </summary>
        IEnumerable<ITerrainObjectType> TerrainObjectTypes { get; }

        /// <summary>
        /// Gets the list of all tile variants defined in this tileset.
        /// </summary>
        IEnumerable<IIsoTileVariant> TileVariants { get; }

        /// <summary>
        /// Gets the default values of the declared cell data fields.
        /// </summary>
        ICellData DefaultCellData { get; }

        /// <summary>
        /// Gets the name of this tileset.
        /// </summary>
        string Name { get; }
    }
}
