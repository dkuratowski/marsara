using System;
using System.Collections.Generic;
using System.Threading;

namespace RC.Common.PNService
{
    /// <summary>
    /// Represents a group of transitions in the Petri-network.
    /// </summary>
    /// <remarks>
    /// When you construct a Petri-network, you have to put every transition to exactly one transition group.
    /// Transitions in the same group can only be fired from the same thread.
    /// </remarks>
    class PNTransitionGroup : IDisposable
    {
        /// <summary>
        /// Constructs a transition group in a Petri-network.
        /// </summary>
        /// <param name="lockObject">The object that will be locked on transition executions.</param>
        public PNTransitionGroup(object lockObject)
        {
            this.lockObject = lockObject;
            this.releaseEvent = new AutoResetEvent(false);
            this.threadAttached = new Semaphore(1, 1);
            this.transitions = new Dictionary<PNTransition, PetriNet.PNTransitionType>();
            this.fireableTransitions = new RCSet<PNTransition>();
        }

        /// <summary>
        /// Registers a transition into this group.
        /// </summary>
        /// <param name="transition">The transition you want to register.</param>
        /// <param name="type">The type of the registered transition.</param>
        public void RegisterTransition(PNTransition transition, PetriNet.PNTransitionType type)
        {
            this.transitions.Add(transition, type);

            /// Every transition is fireable at the beginning.
            TransitionBecameFireable(transition, true);
        }

        /// <summary>
        /// This function is called if a transition became fireable or unfireable.
        /// </summary>
        /// <param name="caller">The caller transition.</param>
        /// <param name="fireable">True if the transition became fireable, false if it became unfireable.</param>
        public void TransitionBecameFireable(PNTransition caller, bool fireable)
        {
            if (fireable)
            {
                /// Add the transition to the fireable transition set, and release the event if this is the first
                /// fireable transition.
                if (this.fireableTransitions.Count == 0) { this.releaseEvent.Set(); }
                this.fireableTransitions.Add(caller);
            }
            else
            {
                /// Remove the transition from the fireable transition set and reset the release event if necessary.
                this.fireableTransitions.Remove(caller);
                if (this.fireableTransitions.Count == 0) { this.releaseEvent.Reset(); }
            }
        }

        /// <see cref="PetriNet.AttachThread"/>
        public void AttachThread(RCSet<PNTransition> extTransitions, Dictionary<PNTransition, PetriNet.PNCallback> callbacks)
        {
            if (this.threadAttached.WaitOne(0))
            {
                try
                {
                    /// Check the parameters.
                    CheckThreadAttachParameters(extTransitions, callbacks);

                    /// First we only allowed to fire external transitions.
                    PetriNet.PNTransitionType stage = PetriNet.PNTransitionType.EXTERNAL;

                    /// Start the thread control loop.
                    while (true)
                    {
                        /// Wait for a fireable transition.
                        this.releaseEvent.WaitOne();

                        lock (this.lockObject)
                        {
                            while (this.fireableTransitions.Count > 0)
                            {
                                if (stage == PetriNet.PNTransitionType.EXTERNAL)
                                {
                                    /// Find the first fireable external transition
                                    PNTransition firstExtTr = null;
                                    foreach (PNTransition fireableTr in this.fireableTransitions)
                                    {
                                        if (this.transitions[fireableTr] == PetriNet.PNTransitionType.CALLBACK)
                                        {
                                            throw new PetriNetException("Invalid type of fireable transition detected!");
                                        }

                                        if (firstExtTr == null && this.transitions[fireableTr] == PetriNet.PNTransitionType.EXTERNAL &&
                                            extTransitions.Contains(fireableTr))
                                        {
                                            firstExtTr = fireableTr;
                                        }
                                    }

                                    /// Fire the transition.
                                    if (firstExtTr != null)
                                    {
                                        firstExtTr.Fire();
                                        stage = PetriNet.PNTransitionType.INTERNAL;
                                    }
                                    else
                                    {
                                        /// No fireable external transitions have been found.
                                        break;
                                    }
                                }
                                else if (stage == PetriNet.PNTransitionType.INTERNAL)
                                {
                                    /// Find the first fireable internal transition or a callback transition
                                    PNTransition trToFire = null;
                                    foreach (PNTransition fireableTr in this.fireableTransitions)
                                    {
                                        if (this.transitions[fireableTr] == PetriNet.PNTransitionType.INTERNAL)
                                        {
                                            if (trToFire == null) { trToFire = fireableTr; }
                                        }
                                        else if (this.transitions[fireableTr] == PetriNet.PNTransitionType.CALLBACK)
                                        {
                                            if (this.fireableTransitions.Count == 1)
                                            {
                                                /// End of thread attach: fire the transition, invoke the callback and return.
                                                fireableTr.Fire();
                                                callbacks[fireableTr](fireableTr.Index);

                                                /// Detach the thread and exit from the function.
                                                this.threadAttached.Release();
                                                return;
                                            }
                                            else { throw new PetriNetException("Invalid state during a thread attach!"); }
                                        }
                                        else { throw new PetriNetException("Invalid type of fireable transition detected!"); }
                                    }

                                    if (trToFire != null)
                                    {
                                        trToFire.Fire();
                                    }
                                    else
                                    {
                                        /// No fireable internal transitions have been found.
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    /// Error occured --> Detach the thread and re-throw the exception.
                    this.threadAttached.Release();
                    throw;
                }
            }
            else
            {
                throw new PetriNetException("You cannot attach more than one threads to the same transition group at the same time!");
            }
        }

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            this.releaseEvent.Close();
            this.threadAttached.Close();
        }

        /// <summary>
        /// Checks the parameters when a thread is being attached to the Petri-network.
        /// </summary>
        /// <exception cref="PetriNetException">If the parameters are wrong.</exception>
        private void CheckThreadAttachParameters(RCSet<PNTransition> extTransitions, Dictionary<PNTransition, PetriNet.PNCallback> callbacks)
        {
            /// All given external transitions have to exist in this transition group.
            foreach (PNTransition tr in extTransitions)
            {
                if (!this.transitions.ContainsKey(tr) || this.transitions[tr] != PetriNet.PNTransitionType.EXTERNAL)
                {
                    throw new PetriNetException("The external transition was not registered at the transition group!");
                }
            }

            /// All given callback transitions have to exist in this transition group.
            foreach (KeyValuePair<PNTransition, PetriNet.PNCallback> item in callbacks)
            {
                if (!this.transitions.ContainsKey(item.Key) || this.transitions[item.Key] != PetriNet.PNTransitionType.CALLBACK)
                {
                    throw new PetriNetException("The callback transition was not registered at the transition group!");
                }
            }

            /// Callback functions must be assigned to each callback transitions in this transition group.
            foreach (KeyValuePair<PNTransition, PetriNet.PNTransitionType> item in this.transitions)
            {
                if (item.Value == PetriNet.PNTransitionType.CALLBACK && (!callbacks.ContainsKey(item.Key) || callbacks[item.Key] == null))
                {
                    throw new PetriNetException("No callback function assigned for a callback transition!");
                }
            }
        }

        /// <summary>
        /// Set of the transitions registered to this group.
        /// </summary>
        private Dictionary<PNTransition, PetriNet.PNTransitionType> transitions;

        /// <summary>
        /// Set of the transitions that are currently fireables.
        /// </summary>
        private RCSet<PNTransition> fireableTransitions;

        /// <summary>
        /// Reference to the lock object of the Petri-network.
        /// </summary>
        private object lockObject;

        /// <summary>
        /// This event will be set when a transition of this group becomes fireable.
        /// </summary>
        private AutoResetEvent releaseEvent;

        /// <summary>
        /// This semaphore indicates whether a thread is attached or not to this transition group of the Petri-network.
        /// </summary>
        private Semaphore threadAttached;
    }
}
