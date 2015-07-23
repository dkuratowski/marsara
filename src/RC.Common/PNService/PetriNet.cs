using System;
using System.Collections.Generic;

namespace RC.Common.PNService
{
    /// <summary>
    /// Represents a Petri-network model that can be used to synchronize a given number of threads.
    /// </summary>
    public class PetriNet : IDisposable
    {
        /// <summary>
        /// Enumerates the possible types of a transition.
        /// </summary>
        public enum PNTransitionType
        {
            INTERNAL = 0,   /// The transition will be fired internally.
            EXTERNAL = 1,   /// The transition will be fired from outside of the Petri-network.
            CALLBACK = 2    /// When the transition is fired, a callback function is called and the thread returns from the network.
        }

        /// <summary>
        /// Defines the prototype of functions that can be called back from the Petri-network if a callback transition
        /// is fired.
        /// </summary>
        /// <param name="whichTransition">The index of the callback transition that caused the callback.</param>
        public delegate void PNCallback(int whichCallbackTransition);

        /// <summary>
        /// Constructs a new Petri-network.
        /// </summary>
        /// <param name="numOfPlaces">The number of the places in this Petri-network.</param>
        /// <param name="numOfTransitions">The number of the transitions in this Petri-network.</param>
        /// <param name="numOfTransitionGroups">The number of the transition groups in this Petri-network.</param>
        public PetriNet(int numOfPlaces, int numOfTransitions, int numOfTransitionGroups)
        {
            if (numOfPlaces < 0) { throw new ArgumentException("The number of places cannot be negative!", "numOfPlaces"); }
            if (numOfTransitions < 1) { throw new ArgumentException("At least one transition is needed!", "numOfTransitions"); }
            if (numOfTransitionGroups < 1) { throw new ArgumentException("At least one transition group is needed!", "numOfTransitionGroups"); }
            if (numOfTransitionGroups > numOfTransitions) { throw new ArgumentException("Number of the transition groups cannot be greater than the number of the transitions!", "numOfTransitionGroups"); }

            this.pnDisposed = false;
            this.buildFinished = false;
            this.disposeLock = new object();
            this.lockObject = new object();
            this.transitionGroups = new PNTransitionGroup[numOfTransitionGroups];
            this.places = new PNPlace[numOfPlaces];
            this.transitions = new PNTransition[numOfTransitions];

            for (int i = 0; i < numOfTransitionGroups; i++)
            {
                this.transitionGroups[i] = new PNTransitionGroup(this.lockObject);
            }

            for (int i = 0; i < numOfPlaces; i++)
            {
                this.places[i] = new PNPlace();
            }
        }

        /// <summary>
        /// Creates a transition with the given index.
        /// </summary>
        /// <param name="transitionIndex">The index of the new transition.</param>
        /// <param name="groupIndex">The index of the group that contains the new transition.</param>
        /// <param name="type">The type of the new transition.</param>
        public void CreateTransition(int transitionIndex, int groupIndex, PNTransitionType type)
        {
            lock (this.disposeLock) { if (this.pnDisposed) { throw new ObjectDisposedException("PetriNet"); } }
            if (this.buildFinished) { throw new PetriNetException("Petri-network is not under construction!"); }
            if (transitionIndex < 0 && transitionIndex >= this.transitions.Length) { throw new ArgumentOutOfRangeException("transitionIndex"); }
            if (this.transitions[transitionIndex] != null) { throw new PetriNetException("Transition with index " + transitionIndex + " already exists!"); }
            if (groupIndex < 0 && groupIndex >= this.transitionGroups.Length) { throw new ArgumentOutOfRangeException("groupIndex"); }
            if (this.transitionGroups[groupIndex] == null) { throw new PetriNetException("Transition group with index " + groupIndex + " doesn't exists!"); }

            /// Create the transition and add it to the group.
            this.transitions[transitionIndex] = new PNTransition(this.transitionGroups[groupIndex], transitionIndex);
            this.transitionGroups[groupIndex].RegisterTransition(this.transitions[transitionIndex], type);
        }

        /// <summary>
        /// Creates an edge from the given place to the given transition with the given weight.
        /// </summary>
        /// <param name="fromPlace">The source of the edge.</param>
        /// <param name="toTransition">The target of the edge.</param>
        /// <param name="weight">The weight of the edge.</param>
        public void CreatePTEdge(int fromPlace, int toTransition, int weight)
        {
            lock (this.disposeLock) { if (this.pnDisposed) { throw new ObjectDisposedException("PetriNet"); } }
            if (this.buildFinished) { throw new PetriNetException("Petri-network is not under construction!"); }
            if (fromPlace < 0 && fromPlace >= this.places.Length) { throw new ArgumentOutOfRangeException("fromPlace"); }
            if (toTransition < 0 && toTransition >= this.transitions.Length) { throw new ArgumentOutOfRangeException("toTransition"); }
            if (this.places[fromPlace] == null) { throw new PetriNetException("Place with index " + fromPlace + " doesn't exists!"); }
            if (this.transitions[toTransition] == null) { throw new PetriNetException("Transition with index " + toTransition + " doesn't exists!"); }
            if (weight <= 0) { throw new ArgumentOutOfRangeException("weight"); }

            /// Create the edge.
            this.places[fromPlace].CreateOutputEdge(this.transitions[toTransition], weight);
            this.transitions[toTransition].RegisterInputEdge(this.places[fromPlace]);
        }

        /// <summary>
        /// Creates an edge from the given transition to the given place with the given weight.
        /// </summary>
        /// <param name="fromTransition">The source of the edge.</param>
        /// <param name="toPlace">The target of the edge.</param>
        /// <param name="weight">The weight of the edge.</param>
        public void CreateTPEdge(int fromTransition, int toPlace, int weight)
        {
            lock (this.disposeLock) { if (this.pnDisposed) { throw new ObjectDisposedException("PetriNet"); } }
            if (this.buildFinished) { throw new PetriNetException("Petri-network is not under construction!"); }
            if (toPlace < 0 && toPlace >= this.places.Length) { throw new ArgumentOutOfRangeException("toPlace"); }
            if (fromTransition < 0 && fromTransition >= this.transitions.Length) { throw new ArgumentOutOfRangeException("fromTransition"); }
            if (this.places[toPlace] == null) { throw new PetriNetException("Place with index " + toPlace + " doesn't exists!"); }
            if (this.transitions[fromTransition] == null) { throw new PetriNetException("Transition with index " + fromTransition + " doesn't exists!"); }
            if (weight <= 0) { throw new ArgumentOutOfRangeException("weight"); }

            /// Create the edge.
            this.transitions[fromTransition].CreateOutputEdge(this.places[toPlace], weight);
        }

        /// <summary>
        /// You have to call this function if the construction of the Petri-network is completed.
        /// </summary>
        /// <param name="initialTokens">
        /// The initial token distribution of the places of the Petri-network (non-positive values will be ignored).
        /// </param>
        public void CommissionNetwork(int[] initialTokens)
        {
            lock (this.disposeLock) { if (this.pnDisposed) { throw new ObjectDisposedException("PetriNet"); } }
            if (this.buildFinished) { throw new PetriNetException("Petri-network has already been commissioned!"); }
            if (initialTokens == null || initialTokens.Length == 0) { throw new ArgumentNullException("initialTokens"); }
            if (initialTokens.Length != this.places.Length) { throw new ArgumentException("The length of the initialTokens array must equal with the number of the places.", "initialTokens"); }

            for (int i = 0; i < initialTokens.Length; ++i)
            {
                this.places[i].PutTokens(initialTokens[i]);
            }

            for (int i = 0; i < this.transitions.Length; i++)
            {
                if (this.transitions[i] == null) { throw new PetriNetException("Unable to commission the Petri-network: transition " + i + " doesn't exist!"); }
            }

            for (int i = 0; i < this.transitionGroups.Length; i++)
            {
                if (this.transitionGroups[i] == null) { throw new PetriNetException("Unable to commission the Petri-network: transition group " + i + " doesn't exist!"); }
            }

            for (int i = 0; i < this.places.Length; i++)
            {
                if (this.places[i] == null) { throw new PetriNetException("Unable to commission the Petri-network: place " + i + " doesn't exist!"); }
            }

            this.buildFinished = true;
        }

        /// <summary>
        /// Call this function from a thread to attach that thread to the Petri-network. The Petri-network will automatically
        /// synchronize the attached threads depending on it's structure.
        /// </summary>
        /// <param name="externalTransitions">
        /// List of the external transitions that the thread wants to fire. The thread will be blocked until at least one
        /// of these transitions becomes fireable.
        /// </param>
        /// <param name="callbackFunctions">
        /// This map has to assign a callback function for each callback transitions.
        /// </param>
        /// <remarks>
        /// The caller thread will be blocked until at least one of the given external transitions becomes fireable. Then
        /// the Petri-network will synchronize the thread while a callback transition becomes fireable. Then this transition
        /// will be fired, the assigned callback function will be called, and the thread will be detached from the
        /// Petri-network.
        /// </remarks>
        public void AttachThread(int[] externalTransitions, Dictionary<int, PNCallback> callbackFunctions)
        {
            lock (this.disposeLock) { if (this.pnDisposed) { throw new ObjectDisposedException("PetriNet"); } }
            if (!this.buildFinished) { throw new PetriNetException("Petri-network is under construction!"); }
            if (externalTransitions == null || externalTransitions.Length == 0) { throw new ArgumentNullException("externalTransitions"); }
            if (callbackFunctions == null || callbackFunctions.Count == 0) { throw new ArgumentNullException("callbackFunctions"); }

            RCSet<PNTransition> extTransitions = new RCSet<PNTransition>();
            Dictionary<PNTransition, PNCallback> callbacks = new Dictionary<PNTransition, PNCallback>();
            PNTransition firstExt = null;

            for (int i = 0; i < externalTransitions.Length; ++i)
            {
                int trIdx = externalTransitions[i];
                if (trIdx >= 0 && trIdx < this.transitions.Length)
                {
                    if (!callbackFunctions.ContainsKey(trIdx))
                    {
                        extTransitions.Add(this.transitions[trIdx]);
                        if (firstExt == null) { firstExt = this.transitions[trIdx]; }
                    }
                    else { throw new ArgumentException("Transition " + trIdx + " already exists in externalTransitions[" + i + "]!", "callbackFunctions"); }
                }
                else { throw new ArgumentException("Transition " + trIdx + " doesn't exist!", "externalTransitions[" + i + "]"); }
            }

            foreach (KeyValuePair<int, PNCallback> item in callbackFunctions)
            {
                int trIdx = item.Key;
                if (trIdx >= 0 && trIdx < this.transitions.Length)
                {
                    callbacks.Add(this.transitions[trIdx], item.Value);
                }
                else { throw new ArgumentException("Transition " + trIdx + " doesn't exist!", "callbackFunctions"); }
            }

            firstExt.Group.AttachThread(extTransitions, callbacks);
        }

        /// <see cref="IDisposable.Dispose"/>
        /// <remarks>
        /// Detaching every thread before disposing a Petri-network is the responsibility of the client class.
        /// If you call this function with attached threads, the behavior is undefined.
        /// </remarks>
        public void Dispose()
        {
            lock (this.disposeLock)
            {
                if (!this.pnDisposed)
                {
                    foreach (PNTransitionGroup grp in this.transitionGroups)
                    {
                        grp.Dispose();
                    }
                    this.pnDisposed = true;
                }
            }
        }

        /// <summary>
        /// List of the transition groups in this Petri-network.
        /// </summary>
        /// <remarks>View PNTransitionGroup for more informations about transition groups.</remarks>
        private PNTransitionGroup[] transitionGroups;

        /// <summary>
        /// List of the places in this Petri-network.
        /// </summary>
        private PNPlace[] places;

        /// <summary>
        /// List of the transitions in this Petri-network.
        /// </summary>
        private PNTransition[] transitions;

        /// <summary>
        /// This object will be locked if a thread wants to execute a transition.
        /// </summary>
        private object lockObject;

        /// <summary>
        /// Lock object for reading and writing the pnDisposed status flag.
        /// </summary>
        private object disposeLock;

        /// <summary>
        /// This flag is true if the Petri-network has been disposed.
        /// </summary>
        private bool pnDisposed;

        /// <summary>
        /// This flag indicates whether the Petri-network is currently under construction (false) or is ready for use (true).
        /// </summary>
        private bool buildFinished;

    }
}
