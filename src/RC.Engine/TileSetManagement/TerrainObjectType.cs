using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine
{
    /// <summary>
    /// Represents a terrain object type in the tileset. 
    /// </summary>
    public class TerrainObjectType
    {
        /// <summary>
        /// Constructs a TerrainObjectType instance.
        /// </summary>
        /// <param name="name">The name of the TerrainObjectType.</param>
        /// <param name="imageData">The byte sequence that contains the image data of the TerrainObjectType.</param>
        /// <param name="quadraticSize">The size of the TerrainObjectType in quadratic tiles.</param>
        /// <param name="offset">The offset of the top-left corner of the TerrainObjectType on the image in pixels.</param>
        public TerrainObjectType(string name, byte[] imageData, RCIntVector quadraticSize, RCIntVector offset, TileSet tileset)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (imageData == null || imageData.Length == 0) { throw new ArgumentNullException("imageData"); }
            if (quadraticSize == RCIntVector.Undefined) { throw new ArgumentNullException("quadraticSize"); }
            if (offset == RCIntVector.Undefined) { throw new ArgumentNullException("offset"); }
            if (tileset == null) { throw new ArgumentNullException("tileset"); }
            if (quadraticSize.X <= 0 || quadraticSize.Y <= 0) { throw new ArgumentOutOfRangeException("quadraticSize", "Quadratic size cannot be 0 in any direction!"); }

            this.name = name;
            this.imageData = imageData;
            this.quadraticSize = quadraticSize;
            this.offset = offset;
            this.tileset = tileset;
            this.areaCanBeExcluded = true;
            this.properties = new Dictionary<string, string>();
            this.constraints = new List<ITerrainObjectConstraint>();
            this.cellDataChangesets = new List<ICellDataChangeSet>();

            this.includedQuadCoords = new HashSet<RCIntVector>();
            for (int x = 0; x < quadraticSize.X; x++)
            {
                for (int y = 0; y < quadraticSize.Y; y++)
                {
                    this.includedQuadCoords.Add(new RCIntVector(x, y));
                }
            }
        }

        /// <summary>
        /// Adds a cell data changeset to this terrain object type.
        /// </summary>
        /// <param name="changeset">The changeset operation to add.</param>
        public void AddCellDataChangeset(ICellDataChangeSet changeset)
        {
            if (changeset == null) { throw new ArgumentNullException("changeset"); }
            if (changeset.Tileset != this.tileset) { throw new TileSetException("The given ICellDataChangeSet is in another TileSet!"); }
            if (this.tileset.IsFinalized) { throw new InvalidOperationException("TileSet already finalized!"); }

            this.cellDataChangesets.Add(changeset);
        }

        /// <summary>
        /// Excludes the given area from this TerrainObjectType. This method is only available if the tileset
        /// has not yet been finalized and constraints/data-changes have not yet been defined.
        /// </summary>
        /// <param name="area">The area that won't be part of this TerrainObjectType.</param>
        public void ExcludeArea(RCIntRectangle area)
        {
            if (area == RCIntRectangle.Undefined) { throw new ArgumentNullException("area"); }
            if (area.X < 0 || area.Y < 0 || area.Right > this.quadraticSize.X || area.Bottom > this.quadraticSize.Y) { throw new ArgumentOutOfRangeException("area", "The excluded area exceeds the borders of the TerrainObjectType!"); }
            if (this.tileset.IsFinalized) { throw new InvalidOperationException("TileSet already finalized!"); }
            if (!this.areaCanBeExcluded) { throw new InvalidOperationException("Excluding area is not allowed!"); }

            for (int x = area.X; x < area.Right; x++)
            {
                for (int y = area.Y; y < area.Bottom; y++)
                {
                    this.includedQuadCoords.Remove(new RCIntVector(x, y));
                }
            }
        }

        /// <summary>
        /// Adds a constraint to this TerrainObjectType. Area exclusion is not possible after calling this method.
        /// </summary>
        /// <param name="constraint">The constraint to add.</param>
        public void AddConstraint(ITerrainObjectConstraint constraint)
        {
            if (constraint == null) { throw new ArgumentNullException("constraint"); }
            if (constraint.Tileset != this.tileset) { throw new TileSetException("The given ITerrainObjectConstraint is in another TileSet!"); }
            if (this.tileset.IsFinalized) { throw new InvalidOperationException("TileSet already finalized!"); }

            this.constraints.Add(constraint);
            this.areaCanBeExcluded = false;
        }

        /// <summary>
        /// Adds a property to this TerrainObjectType.
        /// </summary>
        /// <param name="propName">The name of the property.</param>
        /// <param name="propValue">The value of the property.</param>
        public void AddProperty(string propName, string propValue)
        {
            if (propName == null) { throw new ArgumentNullException("propName"); }
            if (propValue == null) { throw new ArgumentNullException("propValue"); }
            if (this.tileset.IsFinalized) { throw new InvalidOperationException("TileSet already finalized!"); }

            if (this.properties.ContainsKey(propName)) { throw new TileSetException(string.Format("TerrainObjectType already contains a property with name '{0}'!", propName)); }
            this.properties.Add(propName, propValue);
        }

        /// <summary>
        /// Check and finalize this TerrainObjectType. Buildup methods will be unavailable after calling this method.
        /// </summary>
        public void CheckAndFinalize()
        {
            this.areaCanBeExcluded = false;
        }

        /// <summary>
        /// Checks whether the given quadratic position is excluded from this TerrainObjectType or not.
        /// </summary>
        /// <param name="position">The quadratic position to check.</param>
        /// <returns>True if the given quadratic position is excluded from this TerrainObjectType, false otherwise.</returns>
        public bool IsExcluded(RCIntVector position)
        {
            if (position == RCIntVector.Undefined) { throw new ArgumentNullException("position"); }
            return !this.includedQuadCoords.Contains(position);
        }

        /// <summary>
        /// Collects all the quadratic coordinates of the given terrain object violating the constraints of this
        /// terrain object type.
        /// </summary>
        /// <returns>
        /// The list of the quadratic coordinates (relative to the top-left corner) violating the constraints
        /// of this terrain object type.
        /// </returns>
        public HashSet<RCIntVector> CheckConstraints(ITerrainObject terrainObj)
        {
            if (terrainObj == null) { throw new ArgumentNullException("terrainObj"); }

            HashSet<RCIntVector> retList = new HashSet<RCIntVector>();
            foreach (ITerrainObjectConstraint contraint in this.constraints)
            {
                retList.UnionWith(contraint.Check(terrainObj));
            }
            return retList;
        }

        /// <summary>
        /// Gets the constraints of this TerrainObjectType.
        /// </summary>
        //public IEnumerable<ITerrainObjectConstraint> Constraints { get { return this.constraints; } }

        /// <summary>
        /// Gets the tileset of this TerrainObjectType.
        /// </summary>
        public TileSet Tileset { get { return this.tileset; } }

        /// <summary>
        /// Gets the image data of this TerrainObjectType.
        /// </summary>
        public byte[] ImageData { get { return this.imageData; } }

        /// <summary>
        /// Gets the value of a given property.
        /// </summary>
        /// <param name="propName">The name of the property to get.</param>
        /// <returns>The value of the property of null if the property doesn't exists.</returns>
        public string this[string propName]
        {
            get
            {
                if (propName == null) { throw new ArgumentNullException("propName"); }
                return this.properties.ContainsKey(propName) ? this.properties[propName] : null;
            }
        }

        /// <summary>
        /// Gets the name of this TerrainObjectType.
        /// </summary>
        public string Name { get { return this.name; } }

        /// <summary>
        /// Gets the size of the TerrainObjectType in quadratic tiles.
        /// </summary>
        public RCIntVector QuadraticSize { get { return this.quadraticSize; } }

        /// <summary>
        /// Gets the offset of the top-left corner of the TerrainObjectType on the image in pixels.
        /// </summary>
        public RCIntVector Offset { get { return this.offset; } }

        /// <summary>
        /// Gets the cell data changesets of this terrain object type.
        /// </summary>
        public IEnumerable<ICellDataChangeSet> CellDataChangesets { get { return this.cellDataChangesets; } }

        /// <summary>
        /// The size of the TerrainObjectType in quadratic tiles.
        /// </summary>
        private RCIntVector quadraticSize;

        /// <summary>
        /// The offset of the top-left corner of the TerrainObjectType on the image in pixels.
        /// </summary>
        private RCIntVector offset;

        /// <summary>
        /// The byte sequence that contains the image data of this TerrainObjectType.
        /// </summary>
        private byte[] imageData;

        /// <summary>
        /// List of the properties of this TerrainObjectType mapped by their name.
        /// </summary>
        private Dictionary<string, string> properties;

        /// <summary>
        /// The name of this TerrainObjectType.
        /// </summary>
        private string name;

        /// <summary>
        /// List of the constraints of this TerrainObjectType.
        /// </summary>
        private List<ITerrainObjectConstraint> constraints;

        /// <summary>
        /// List of the quadratic coordinates that are included in this TerrainObjectType.
        /// </summary>
        private HashSet<RCIntVector> includedQuadCoords;

        /// <summary>
        /// List of the cell data changesets of this terrain object type.
        /// </summary>
        private List<ICellDataChangeSet> cellDataChangesets;

        /// <summary>
        /// This flag indicates whether excluding area is allowed or not.
        /// </summary>
        private bool areaCanBeExcluded;

        /// <summary>
        /// The tileset that this TerrainObjectType belongs to.
        /// </summary>
        private TileSet tileset;
    }
}
