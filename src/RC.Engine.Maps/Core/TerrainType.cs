using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Maps.Core
{
    /// <summary>
    /// Represents a type of terrain in a tileset.
    /// </summary>
    class TerrainType : ITerrainType
    {
        /// <summary>
        /// Constructs a TerrainType instance.
        /// </summary>
        /// <param name="name">The name of the terrain type.</param>
        /// <param name="tileset">The tileset of the terrain type.</param>
        public TerrainType(string name, TileSet tileset)
        {
            if (name == null) { throw new ArgumentNullException("name"); }
            if (tileset == null) { throw new ArgumentNullException("tileset"); }

            this.name = name;
            this.tileset = tileset;
            this.parent = null;
            this.children = new HashSet<TerrainType>();
            this.transitionLength = 0;
        }

        /// <summary>
        /// Adds the given terrain type object as a child of this.
        /// </summary>
        /// <param name="otherName">The name of the terrain type to add as a child.</param>
        public void AddChild(string otherName)
        {
            this.AddChild(otherName, 0);
        }

        /// <summary>
        /// Adds the given terrain type object as a child of this.
        /// </summary>
        /// <param name="otherName">The name of the terrain type to add as a child.</param>
        /// <param name="transitionLength">The transition length from this terrain type to the added child.</param>
        public void AddChild(string otherName, int transitionLength)
        {
            if (this.tileset.IsFinalized) { throw new InvalidOperationException("TileSet already finalized!"); }
            if (transitionLength < 0) { throw new ArgumentOutOfRangeException("transitionLength"); }
            if (otherName == null) { throw new ArgumentNullException("otherName"); }

            TerrainType other = this.tileset.GetTerrainTypeImpl(otherName);
            if (other == this) { throw new TileSetException("Unable to add a TerrainType to itself as a child!"); }
            if (other.parent != null) { throw new TileSetException("The given TerrainType already has a parent!"); }
            if (other.tileset != this.tileset) { throw new TileSetException("The given TerrainType is in another TileSet!"); }
            if (children.Contains(other)) { throw new TileSetException("The given TerrainType is already a child of this!"); }

            /// Add as a child
            this.children.Add(other);
            other.parent = this;

            /// Check the tree property and rollback if necessary
            if (!this.CheckTreeProperty())
            {
                this.children.Remove(other);
                other.parent = null;
                throw new TileSetException("Violating tree property!");
            }

            other.transitionLength = transitionLength;
        }
        
        /// <summary>
        /// Check and finalize the TerrainType object. Buildup methods will be unavailable after calling this method.
        /// </summary>
        public void CheckAndFinalize()
        {
        }

        #region ITerrainType methods

        /// <see cref="ITerrainType.IsDescendantOf"/>
        public bool IsDescendantOf(ITerrainType other)
        {
            if (!this.tileset.IsFinalized) { throw new InvalidOperationException("Tileset is not yet finalized!"); }
            if (this.tileset != other.Tileset) { throw new TileSetException("The given TerrainType is in another TileSet!"); }

            TerrainType current = this;
            while (current.parent != null)
            {
                if (current.parent == other)
                {
                    /// Descendant of other.
                    return true;
                }
                else
                {
                    /// Continue towards the root.
                    current = current.parent;
                }
            }

            /// Other is on another branch.
            return false;
        }

        /// <see cref="ITerrainType.FindRoute"/>
        public ITerrainType[] FindRoute(ITerrainType target)
        {
            if (!this.tileset.IsFinalized) { throw new InvalidOperationException("Tileset is not yet finalized!"); }
            if (this.tileset != target.Tileset) { throw new TileSetException("The given TerrainType is in another TileSet!"); }

            List<ITerrainType> thisToRoot = new List<ITerrainType>();
            List<ITerrainType> targetToRoot = new List<ITerrainType>();

            /// Collect terrain types from this to the root.
            ITerrainType current = this;
            do
            {
                thisToRoot.Insert(0, current);
                current = current.Parent;
            } while (current != null);

            /// Collect terrain types from the target to the root.
            current = target;
            do
            {
                targetToRoot.Insert(0, current);
                current = current.Parent;
            } while (current != null);

            /// Search the index of the last equal terrain type in the list.
            int forkIdx = -1;
            for (; forkIdx + 1 < thisToRoot.Count &&
                    forkIdx + 1 < targetToRoot.Count &&
                    thisToRoot[forkIdx + 1] == targetToRoot[forkIdx + 1];
                 ++forkIdx) ;

            /// Collect the route from the two lists.
            ITerrainType[] route = new ITerrainType[thisToRoot.Count - forkIdx + targetToRoot.Count - forkIdx - 1];
            int routeIdx = 0;
            for (int i = thisToRoot.Count - 1; i > forkIdx; i--, routeIdx++)
            {
                route[routeIdx] = thisToRoot[i];
            }
            for (int i = forkIdx; i < targetToRoot.Count; i++, routeIdx++)
            {
                route[routeIdx] = targetToRoot[i];
            }

            return route;
        }

        /// <see cref="ITerrainType.Name"/>
        public string Name { get { return this.name; } }

        /// <see cref="ITerrainType.Children"/>
        public IEnumerable<ITerrainType> Children { get { return this.children; } }

        /// <see cref="ITerrainType.Parent"/>
        public ITerrainType Parent { get { return this.GetParentImpl(); } }

        /// <see cref="ITerrainType.TransitionLength"/>
        public int TransitionLength { get { return this.transitionLength; } }

        /// <see cref="ITerrainType.Tileset"/>
        public ITileSet Tileset { get { return this.tileset; } }

        /// <see cref="ITerrainType.HasChildren"/>
        public bool HasChildren { get { return this.children.Count != 0; } }

        #endregion ITerrainType methods

        /// <summary>
        /// Gets the string representation of this TerrainType.
        /// </summary>
        public override string ToString()
        {
            return this.name;
        }

        /// <summary>
        /// Internal implementation of TerrainType.Parent property.
        /// </summary>
        public TerrainType GetParentImpl() { return this.parent; }

        /// <summary>
        /// Internal method for checking the existence of the tree property after adding a child.
        /// </summary>
        /// <returns>True if the tree property still exists, false otherwise.</returns>
        private bool CheckTreeProperty()
        {
            TerrainType current = this;
            while (current.parent != null)
            {
                if (current.parent == this)
                {
                    /// Circle found
                    return false;
                }
                else
                {
                    /// Continue towards the root
                    current = current.parent;
                }
            }
            return true;
        }

        /// <summary>
        /// The name of this terrain type.
        /// </summary>
        private string name;

        /// <summary>
        /// The parent of this terrain type.
        /// </summary>
        private TerrainType parent;

        /// <summary>
        /// The children of this terrain type.
        /// </summary>
        private HashSet<TerrainType> children;

        /// <summary>
        /// Indicates the transition length from the parent to this terrain type. 
        /// </summary>
        private int transitionLength;

        /// <summary>
        /// Reference to the tileset of this terrain type.
        /// </summary>
        private TileSet tileset;
    }
}
