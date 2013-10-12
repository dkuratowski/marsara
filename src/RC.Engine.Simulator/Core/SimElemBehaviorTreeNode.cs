using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents a node in the behavior tree definition of a simulation element type.
    /// </summary>
    class SimElemBehaviorTreeNode
    {
        /// <summary>
        /// Constructs a node of a behavior tree definition.
        /// </summary>
        /// <param name="behaviorType">The type of the behavior represented by this node.</param>
        /// <param name="behaviorFactory">Reference to the corresponding factory object.</param>
        /// <param name="children">Reference to the child nodes.</param>
        public SimElemBehaviorTreeNode(string behaviorType, List<SimElemBehaviorTreeNode> children)
        {
            if (behaviorType == null) { throw new ArgumentNullException("behaviorType"); }

            this.behaviorType = behaviorType;
            this.behaviorFactory = null; // will be filled during CheckAndFinalize
            this.children = children != null ? new List<SimElemBehaviorTreeNode>(children) : new List<SimElemBehaviorTreeNode>();
        }

        /// <summary>
        /// Sets the factory object for this behavior tree node.
        /// </summary>
        /// <param name="factory">The factory object to set.</param>
        public void SetFactory(ISimElemBehaviorFactory factory)
        {
            if (this.behaviorFactory != null) { throw new InvalidOperationException("Factory object already set!"); }
            if (factory == null) { throw new ArgumentNullException("factory"); }
            this.behaviorFactory = factory;
        }

        /// <summary>
        /// Gets the name of the behavior type that belongs to this node.
        /// </summary>
        public string BehaviorType { get { return this.behaviorType; } }

        /// <summary>
        /// Gets the reference to the factory object where the behaviors will be constructed.
        /// </summary>
        public ISimElemBehaviorFactory BehaviorFactory { get { return this.behaviorFactory; } }

        /// <summary>
        /// Gets the list of the references to the child nodes.
        /// </summary>
        public IEnumerable<SimElemBehaviorTreeNode> Children { get { return this.children; } }

        /// <summary>
        /// The name of the behavior type that belongs to this node.
        /// </summary>
        private string behaviorType;

        /// <summary>
        /// Reference to the factory object where the behaviors will be constructed.
        /// </summary>
        private ISimElemBehaviorFactory behaviorFactory;

        /// <summary>
        /// References to the child nodes.
        /// </summary>
        private List<SimElemBehaviorTreeNode> children;
    }
}
