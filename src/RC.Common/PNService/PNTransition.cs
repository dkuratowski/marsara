using System;
using System.Collections.Generic;

namespace RC.Common.PNService
{
    /// <summary>
    /// Represents a transition in a Petri-network.
    /// </summary>
    class PNTransition
    {
        /// <summary>
        /// Constructs a transition in a Petri-network.
        /// </summary>
        public PNTransition(PNTransitionGroup group, int index)
        {
            if (group == null) { throw new ArgumentNullException("group"); }
            this.fireable = true;
            this.index = index;
            this.group = group;
            this.outputEdges = new Dictionary<PNPlace, int>();
            this.inputEdges = new RCSet<PNPlace>();
            this.fireConditions = new Dictionary<PNPlace, bool>();
        }

        /// <summary>
        /// Creates an edge from this transition to the given place with the given weight.
        /// </summary>
        /// <param name="target">The target place of the edge.</param>
        /// <param name="weight">The weight of the edge.</param>
        public void CreateOutputEdge(PNPlace target, int weight)
        {
            if (!this.outputEdges.ContainsKey(target))
            {
                this.outputEdges.Add(target, weight);
            }
            else { throw new ArgumentException("Output edge to the given target already exists!"); }
        }

        /// <summary>
        /// Registers an edge from the given place to this transition.
        /// </summary>
        /// <param name="source">The source place of the registered edge.</param>
        public void RegisterInputEdge(PNPlace source)
        {
            if (!this.fireConditions.ContainsKey(source))
            {
                this.fireConditions.Add(source, false);
                this.inputEdges.Add(source);
                this.fireable = false;
                this.group.TransitionBecameFireable(this, false);
            }
            else { throw new ArgumentException("Input edge from the given source already exists!"); }
        }

        /// <summary>
        /// This function is called by a PNPlace if it received enough tokens to satisfy the fire condition belongs to it,
        /// or when this fire condition cannot be satisfied anymore.
        /// </summary>
        /// <param name="caller">The place that called this function.</param>
        /// <param name="satisfied">True if the fire condition can be satisfied or false otherwise.</param>
        public void FireConditionChange(PNPlace caller, bool satisfied)
        {
            if (!this.fireConditions.ContainsKey(caller)) { throw new PetriNetException("The given place is not registered to the transition as an input place!"); }

            if (satisfied)
            {
                /// A fire condition satisfied.
                if (!this.fireable && !this.fireConditions[caller])
                {
                    this.fireConditions[caller] = true;
                    foreach (KeyValuePair<PNPlace, bool> condition in this.fireConditions)
                    {
                        if (!condition.Value)
                        {
                            /// There are other unsatisfied fire conditions, so we can return.
                            return;
                        }
                    }

                    /// This was the last unsatisfied fire condition.
                    this.fireable = true;
                    this.group.TransitionBecameFireable(this, true);
                }
            }
            else
            {
                /// The fire condition cannot be satisfied, so the transition becomes unfireable.
                this.fireConditions[caller] = false;
                this.fireable = false;
                this.group.TransitionBecameFireable(this, false);
            }
        }

        /// <summary>
        /// Fires the transition.
        /// </summary>
        /// <exception cref="PetriNetException">If the transition is not fireable.</exception>
        public void Fire()
        {
            if (this.fireable)
            {
                /// Remove tokens from the input places.
                foreach (PNPlace srcPlace in this.inputEdges)
                {
                    srcPlace.RequestTokens(this);
                }

                /// Put tokens to the output places.
                foreach (KeyValuePair<PNPlace, int> outputEdge in this.outputEdges)
                {
                    outputEdge.Key.PutTokens(outputEdge.Value);
                }

                //TraceManager.WriteAllTrace("Transition " + this.index + " fired");
            }
            else { throw new PetriNetException("Non-fireable transition cannot be fired!"); }
        }

        /// <summary>
        /// Gets the index of this transition inside the Petri-network.
        /// </summary>
        public int Index { get { return this.index; } }

        /// <summary>
        /// Gets the group of this transition.
        /// </summary>
        public PNTransitionGroup Group { get { return this.group; } }

        /// <summary>
        /// Reference to the group that this transition belongs to.
        /// </summary>
        private PNTransitionGroup group;

        /// <summary>
        /// List of the target places of the output edges and their weights of this transition.
        /// </summary>
        private Dictionary<PNPlace, int> outputEdges;

        /// <summary>
        /// List of the source places of this transition.
        /// </summary>
        private RCSet<PNPlace> inputEdges;

        /// <summary>
        /// This map tells whether or not there are enough tokens at the input places to fire this transition.
        /// </summary>
        private Dictionary<PNPlace, bool> fireConditions;

        /// <summary>
        /// A flag that indicates whether this transition is fireable or not (all fire conditions are true or not).
        /// </summary>
        private bool fireable;

        /// <summary>
        /// The index of this transition inside the Petri-network.
        /// </summary>
        private int index;
    }
}
