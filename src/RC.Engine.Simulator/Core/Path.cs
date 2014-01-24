using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common;
using RC.Engine.Maps.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents a path computed by the pathfinder.
    /// </summary>
    abstract class Path : IPath
    {
        /// <summary>
        /// Constructs a new Path instance.
        /// </summary>
        public Path()
        {
            this.nodesOnPath = null;
        }

        #region IPath methods

        /// <see cref="IPath.IsReadyForUse"/>
        public abstract bool IsReadyForUse { get; }

        /// <see cref="IPath.IsTargetFound"/>
        public bool IsTargetFound
        {
            get
            {
                if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
                if (this.nodesOnPath == null) { this.nodesOnPath = this.CollectNodesOnPath(); }
                return this.nodesOnPath[this.Length - 1] == this.ToNode;
            }
        }
        
        /// <see cref="IPath.this[]"/>
        public RCIntRectangle this[int index]
        {
            get
            {
                if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
                if (this.nodesOnPath == null) { this.nodesOnPath = this.CollectNodesOnPath(); }
                return this.nodesOnPath[index].AreaOnMap;
            }
        }

        /// <see cref="IPath.Length"/>
        public int Length
        {
            get
            {
                if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
                if (this.nodesOnPath == null) { this.nodesOnPath = this.CollectNodesOnPath(); }
                return this.nodesOnPath.Count;
            }
        }

        /// <see cref="IPath.ForgetBlockedEdges"/>
        public void ForgetBlockedEdges()
        {
            if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
            this.ForgetBlockedEdgesImpl();
        }

        #endregion IPath methods

        #region Methods for debugging

        /// <summary>
        /// Gets the list of nodes in the completed list.
        /// </summary>
        /// <remarks>TODO: this is only for debugging!</remarks>
        protected internal abstract IEnumerable<PFTreeNode> CompletedNodes { get; }

        #endregion Methods for debugging

        #region Internal methods

        /// <summary>
        /// Gets a node of the path.
        /// </summary>
        /// <param name="index">The index of the node to get.</param>
        /// <returns>The node at the given index.</returns>
        /// <exception cref="InvalidOperationException">If the path is not ready for use.</exception>
        public PFTreeNode GetPathNode(int index)
        {
            if (!this.IsReadyForUse) { throw new InvalidOperationException("Path is not ready for use!"); }
            if (this.nodesOnPath == null) { this.nodesOnPath = this.CollectNodesOnPath(); }
            return this.nodesOnPath[index];
        }

        /// <summary>
        /// Copies the blocked edges of this path to the target set.
        /// </summary>
        /// <param name="targetSet">The target set to copy.</param>
        /// <remarks>Can be overriden in the derived classes. The default implementation does nothing.</remarks>
        public virtual void CopyBlockedEdges(ref HashSet<Tuple<int, int>> targetSet) { }

        /// <summary>
        /// Forgets every blocked edges that was used when the path was computed.
        /// </summary>
        /// <remarks>Can be overriden in the derived classes. The default implementation does nothing.</remarks>
        protected virtual void ForgetBlockedEdgesImpl() { }

        /// <summary>
        /// Gets the source node of this path.
        /// </summary>
        public abstract PFTreeNode FromNode { get; }

        /// <summary>
        /// Gets the target node of this path.
        /// </summary>
        public abstract PFTreeNode ToNode { get; }

        #endregion Internal methods

        /// <summary>
        /// Collects the nodes along the computed path.
        /// </summary>
        protected abstract List<PFTreeNode> CollectNodesOnPath();

        /// <summary>
        /// The list of the nodes among this path.
        /// </summary>
        private List<PFTreeNode> nodesOnPath;
    }
}
