using System;
using System.Collections.Generic;

namespace RC.Common.PNService
{
    /// <summary>
    /// Represents a place in a Petri-network.
    /// </summary>
    class PNPlace
    {
        /// <summary>
        /// Constructs a place in a Petri-network.
        /// </summary>
        public PNPlace()
        {
            this.tokens = 0;
            this.outputEdges = new Dictionary<PNTransition, int>();
        }

        /// <summary>
        /// Creates an edge from this place to the given transition with the given weight.
        /// </summary>
        /// <param name="target">The target transition of the edge.</param>
        /// <param name="weight">The weight of the edge.</param>
        public void CreateOutputEdge(PNTransition target, int weight)
        {
            if (!this.outputEdges.ContainsKey(target))
            {
                this.outputEdges.Add(target, weight);
            }
            else { throw new ArgumentException("Output edge to the given target already exists!"); }
        }

        /// <summary>
        /// Puts the given number of tokens to this place.
        /// </summary>
        /// <param name="tokenNum">
        /// The number of the tokens you want to put into this place (non-positive values will be ignored).
        /// </param>
        public void PutTokens(int tokenNum)
        {
            if (tokenNum > 0)
            {
                this.tokens += tokenNum;
                foreach (KeyValuePair<PNTransition, int> edge in this.outputEdges)
                {
                    if (this.tokens >= edge.Value)
                    {
                        edge.Key.FireConditionChange(this, true);
                    }
                }
            }
        }

        /// <summary>
        /// This function is called by a transition that is being fired, when it wants to remove the tokens from it's
        /// input places.
        /// </summary>
        /// <param name="caller">
        /// The transition that called this function.
        /// </param>
        public void RequestTokens(PNTransition caller)
        {
            if (this.outputEdges.ContainsKey(caller))
            {
                int tokenNum = this.outputEdges[caller];
                this.tokens = (this.tokens >= tokenNum) ? (this.tokens - tokenNum) : (0);
                foreach (KeyValuePair<PNTransition, int> edge in this.outputEdges)
                {
                    if (this.tokens < edge.Value)
                    {
                        edge.Key.FireConditionChange(this, false);
                    }
                }
            }
            else { throw new PetriNetException("Not enough tokens for the given transition!"); }
        }

        /// <summary>
        /// The number of the tokens in this place.
        /// </summary>
        private int tokens;

        /// <summary>
        /// List of the target transitions of the output edges and their weights of this place.
        /// </summary>
        private Dictionary<PNTransition, int> outputEdges;
    }
}
