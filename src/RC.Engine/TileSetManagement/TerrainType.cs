using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine
{
    /// <summary>
    /// Represents a type of terrain in a tileset.
    /// </summary>
    public class TerrainType
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

            TerrainType other = this.tileset.GetTerrainType(otherName);
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

        /// <summary>
        /// Checks whether this terrain type is a direct or indirect descendant of the given terrain type.
        /// </summary>
        /// <param name="other">The given terrain type.</param>
        /// <returns>
        /// True if this terrain type is a direct or indirect descendant of the given terrain type, false otherwise.
        /// </returns>
        public bool IsDescendantOf(TerrainType other)
        {
            if (!this.tileset.IsFinalized) { throw new InvalidOperationException("Tileset is not yet finalized!"); }
            if (this.tileset != other.tileset) { throw new TileSetException("The given TerrainType is in another TileSet!"); }

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

        /// <summary>
        /// Finds a route from this terrain type to the given terrain type in the terrain tree.
        /// </summary>
        /// <param name="target">The target terrain type of the route.</param>
        /// <returns>
        /// A list that contains the route to the target. The first item in the list is this terrain type,
        /// the last item in the list is the target terrain type. If target equals with this, then the list
        /// will contain only one item: this terrain type.
        /// </returns>
        public TerrainType[] FindRoute(TerrainType target)
        {
            if (!this.tileset.IsFinalized) { throw new InvalidOperationException("Tileset is not yet finalized!"); }
            if (this.tileset != target.tileset) { throw new TileSetException("The given TerrainType is in another TileSet!"); }

            return this.FindRouteImpl(target);
        }

        /// <summary>
        /// Finds a transition from this terrain type to the given terrain type in the terrain tree.
        /// </summary>
        /// <param name="target">The target terrain type of the route.</param>
        /// <returns>
        /// A list that contains the transition to the target. The first item in the list is this terrain type,
        /// the last item in the list is the target terrain type. If target equals with this, then the list
        /// will contain only one item: this terrain type.
        /// </returns>
        public TerrainType[] FindTransition(TerrainType target)
        {
            if (!this.tileset.IsFinalized) { throw new InvalidOperationException("Tileset is not yet finalized!"); }
            if (this.tileset != target.tileset) { throw new TileSetException("The given TerrainType is in another TileSet!"); }

            return this.FindTransitionImpl(target);
        }

        /// <summary>
        /// Gets the string representation of this TerrainType.
        /// </summary>
        public override string ToString()
        {
            return this.name;
        }

        /// <summary>
        /// Gets the name of this terrain type.
        /// </summary>
        public string Name { get { return this.name; } }

        /// <summary>
        /// Gets the children of this terrain type.
        /// </summary>
        public IEnumerable<TerrainType> Children { get { return this.children; } }

        /// <summary>
        /// Gets the parent of this terrain type.
        /// </summary>
        public TerrainType Parent { get { return this.parent; } }

        /// <summary>
        /// Gets the transition length from the parent to this terrain type. 
        /// </summary>
        public int TransitionLength { get { return this.transitionLength; } }

        /// <summary>
        /// Gets the tileset of this terrain type.
        /// </summary>
        public TileSet Tileset { get { return this.tileset; } }

        /// <summary>
        /// Gets whether this terrain type has children or not.
        /// </summary>
        public bool HasChildren { get { return this.children.Count != 0; } }

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
        /// Internal implementation of TerrainType.FindRoute.
        /// </summary>
        private TerrainType[] FindRouteImpl(TerrainType target)
        {
            List<TerrainType> thisToRoot = new List<TerrainType>();
            List<TerrainType> targetToRoot = new List<TerrainType>();

            /// Collect terrain types from this to the root.
            TerrainType current = this;
            do
            {
                thisToRoot.Insert(0, current);
                current = current.parent;
            } while (current != null);

            /// Collect terrain types from the target to the root.
            current = target;
            do
            {
                targetToRoot.Insert(0, current);
                current = current.parent;
            } while (current != null);

            /// Search the index of the last equal terrain type in the list.
            int forkIdx = -1;
            for ( ; forkIdx + 1 < thisToRoot.Count &&
                    forkIdx + 1 < targetToRoot.Count &&
                    thisToRoot[forkIdx + 1] == targetToRoot[forkIdx + 1];
                 ++forkIdx) ;

            /// Collect the route from the two lists.
            TerrainType[] route = new TerrainType[thisToRoot.Count - forkIdx + targetToRoot.Count - forkIdx - 1];
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

        /// <summary>
        /// Internal implementation of TerrainType.FindTransition.
        /// </summary>
        private TerrainType[] FindTransitionImpl(TerrainType target)
        {
            TerrainType[] route = this.FindRouteImpl(target);
            List<TerrainType> transition = new List<TerrainType>();
            transition.Add(route[0]);
            int routeIdx = 1;
            int trans = 0;
            while (routeIdx < route.Length)
            {
                TerrainType prevTerrain = route[routeIdx - 1];
                TerrainType currTerrain = route[routeIdx];
                transition.Add(currTerrain);

                if (prevTerrain.parent == currTerrain)
                {
                    /// Previous is a child of current.
                    if (trans >= (routeIdx + 1 < route.Length && route[routeIdx + 1].parent != currTerrain ? prevTerrain.transitionLength : 0))
                    {
                        /// Continue the route.
                        trans = 0;
                        routeIdx++;
                    }
                    else
                    {
                        /// Continue the transition.
                        trans++;
                    }
                }
                else if (currTerrain.parent == prevTerrain)
                {
                    /// Previous is the parent of current.
                    if (trans >= (routeIdx - 2 >= 0 && route[routeIdx - 2].parent != prevTerrain ? currTerrain.transitionLength : 0))
                    {
                        /// Continue the route.
                        trans = 0;
                        routeIdx++;
                    }
                    else
                    {
                        /// Continue the transition.
                        trans++;
                    }
                }
                else
                {
                    /// Theoretically impossible case.
                    throw new InvalidOperationException("Unexpected case!");
                }
            }

            return transition.ToArray();
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
