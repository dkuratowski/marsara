using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Represents the metadata informations for the simulation.
    /// </summary>
    class SimulationMetadata
    {
        /// <summary>
        /// Constructs a SimulationMetadata object.
        /// </summary>
        public SimulationMetadata()
        {
            this.isFinalized = false;
            this.compositeHeapTypes = new Dictionary<string, SimHeapType>();
            this.indicatorDefinitions = new Dictionary<string, SimElemIndicatorDef>();
            this.behaviorFactories = new Dictionary<string, ISimElemBehaviorFactory>();
            this.behaviorFactoryInstances = new Dictionary<Type, ISimElemBehaviorFactory>();
            this.behaviorTreeDefs = new Dictionary<string, List<SimElemBehaviorTreeNode>>();
        }

        /// <summary>
        /// Gets the list of all composite heap types defined in the metadata.
        /// </summary>
        public IEnumerable<SimHeapType> CompositeTypes { get { return this.compositeHeapTypes.Values; } }

        #region Metadata buildup methods

        /// <summary>
        /// Adds a composite heap type to the metadata.
        /// </summary>
        /// <param name="heapType">The heap type to add.</param>
        public void AddCompositeHeapType(SimHeapType heapType)
        {
            if (this.isFinalized) { throw new InvalidOperationException("Metadata object already finalized!"); }
            if (heapType == null) { throw new ArgumentNullException("heapType"); }
            if (heapType.BuiltInType != BuiltInTypeEnum.NonBuiltIn || heapType.PointedTypeID != -1) { throw new ArgumentException("The given type is not a composite heap type!", "heapType"); }
            if (this.compositeHeapTypes.ContainsKey(heapType.Name)) { throw new InvalidOperationException(string.Format("Composite heap type '{0}' already defined!", heapType.Name)); }

            this.compositeHeapTypes.Add(heapType.Name, heapType);
        }

        /// <summary>
        /// Adds an indicator definition to the metadata.
        /// </summary>
        /// <param name="indicatorDef">The indicator definition to add.</param>
        public void AddIndicatorDef(SimElemIndicatorDef indicatorDef)
        {
            if (this.isFinalized) { throw new InvalidOperationException("Metadata object already finalized!"); }
            if (indicatorDef == null) { throw new ArgumentNullException("indicatorDef"); }
            if (this.indicatorDefinitions.ContainsKey(indicatorDef.ElementType)) { throw new InvalidOperationException(string.Format("Indicator definition for element type '{0}' already exists!", indicatorDef.ElementType)); }

            indicatorDef.SetIndex(this.indicatorDefinitions.Count);
            this.indicatorDefinitions.Add(indicatorDef.ElementType, indicatorDef);
        }

        /// <summary>
        /// Adds a behavior tree definition to the metadata.
        /// </summary>
        /// <param name="elementType">The name of the corresponding element type.</param>
        /// <param name="rootNodes">List of the root nodes of the behavior tree.</param>
        public void AddBehaviorTreeDef(string elementType, List<SimElemBehaviorTreeNode> rootNodes)
        {
            if (this.isFinalized) { throw new InvalidOperationException("Metadata object already finalized!"); }
            if (elementType == null) { throw new ArgumentNullException("elementType"); }
            if (rootNodes == null || rootNodes.Count == 0) { throw new ArgumentNullException("rootNodes"); }
            if (this.behaviorTreeDefs.ContainsKey(elementType)) { throw new InvalidOperationException(string.Format("Behavior tree definition for element type '{0}' already exists!", elementType)); }

            this.behaviorTreeDefs.Add(elementType, rootNodes);
        }

        /// <summary>
        /// Adds a behavior factory to this metadata object.
        /// </summary>
        /// <param name="behaviorTypeName">The type of the behaviors created by the factory.</param>
        /// <param name="assemblyName">The assembly in which the factory is implemented.</param>
        /// <param name="className">The class that contains the implementation of the factory.</param>
        /// <remarks>
        /// Only 1 factory instance will be created from each factory class even if the same class were
        /// given to more than one behavior types.
        /// </remarks>
        public void AddBehaviorFactory(string behaviorTypeName, string assemblyName, string className)
        {
            if (this.isFinalized) { throw new InvalidOperationException("Metadata object already finalized!"); }
            if (behaviorTypeName == null) { throw new ArgumentNullException("behaviorTypeName"); }
            if (assemblyName == null) { throw new ArgumentNullException("assemblyName"); }
            if (className == null) { throw new ArgumentNullException("className"); }
            if (this.behaviorFactories.ContainsKey(behaviorTypeName)) { throw new SimulatorException(string.Format("Behavior type '{0}' already defined!", behaviorTypeName)); }

            /// Load the given class from the given assembly.
            Assembly asm = Assembly.Load(assemblyName);
            if (asm == null) { throw new SimulatorException(string.Format("Assembly '{0}' not found!", assemblyName)); }
            Type factoryType = asm.GetType(className);
            if (factoryType == null) { throw new SimulatorException(string.Format("Factory type '{0}' not found!", className)); }

            /// Create a new factory instance if necessary.
            if (!this.behaviorFactoryInstances.ContainsKey(factoryType))
            {
                ISimElemBehaviorFactory newFactory = Activator.CreateInstance(factoryType) as ISimElemBehaviorFactory;
                if (newFactory == null) { throw new SimulatorException(string.Format("Factory type '{0}' doesn't implement RC.Engine.Simulator.PublicInterfaces.ISimElemBehaviorFactory!", className)); }
                this.behaviorFactoryInstances.Add(factoryType, newFactory);
            }

            /// Assign the factory instance to the given behavior type.
            this.behaviorFactories.Add(behaviorTypeName, this.behaviorFactoryInstances[factoryType]);
        }

        /// <summary>
        /// Checks and finalizes the metadata object. Buildup methods will be unavailable after calling this method.
        /// </summary>
        public void CheckAndFinalize()
        {
            foreach (SimElemIndicatorDef indicatorDef in this.indicatorDefinitions.Values)
            {
                indicatorDef.CheckAndFinalize();
            }

            foreach (List<SimElemBehaviorTreeNode> treeDef in this.behaviorTreeDefs.Values)
            {
                foreach (SimElemBehaviorTreeNode treeNode in treeDef)
                {
                    SetBehaviorFactory(treeNode);
                }
            }
            this.isFinalized = true;
        }

        /// <summary>
        /// Sets the behavior factory for the given tree node.
        /// </summary>
        /// <param name="treeNode">The tree node whose factory will be set.</param>
        private void SetBehaviorFactory(SimElemBehaviorTreeNode treeNode)
        {
            if (!this.behaviorFactories.ContainsKey(treeNode.BehaviorType)) { throw new SimulatorException(string.Format("Factory not found for behavior type '{0}'!", treeNode.BehaviorType)); }
            treeNode.SetFactory(this.behaviorFactories[treeNode.BehaviorType]);

            foreach (SimElemBehaviorTreeNode childNode in treeNode.Children)
            {
                SetBehaviorFactory(childNode);
            }
        }

        #endregion Metadata buildup methods

        /// <summary>
        /// List of the composite heap data types mapped by their names.
        /// </summary>
        private Dictionary<string, SimHeapType> compositeHeapTypes;

        /// <summary>
        /// List of the indicator definitions mapped by the names of the corresponding element types.
        /// </summary>
        private Dictionary<string, SimElemIndicatorDef> indicatorDefinitions;

        /// <summary>
        /// List of the behavior tree definitions mapped by the names of the corresponding element types.
        /// </summary>
        private Dictionary<string, List<SimElemBehaviorTreeNode>> behaviorTreeDefs;

        /// <summary>
        /// List of the registered behavior factories mapped by the names of the corresponding behavior types.
        /// </summary>
        private Dictionary<string, ISimElemBehaviorFactory> behaviorFactories;

        /// <summary>
        /// List of the behavior factory instances mapped by their runtime types.
        /// </summary>
        private Dictionary<Type, ISimElemBehaviorFactory> behaviorFactoryInstances;

        /// <summary>
        /// Becomes true when this metadata object is finalized.
        /// </summary>
        private bool isFinalized;
    }
}
