namespace RC.Common.SMC
{
    class SMExternalTrigger : SMTrigger, ISMTrigger
    {
        public SMExternalTrigger(SMState source, SMState target, StateMachineController.TransitionHandler handler)
            : base(source, target, handler)
        {
            //this.triggerActive = false;
        }

        /// <see cref="ISMTrigger.Fire"/>
        public void Fire()
        {
            if (!this.sourceState.SM.Commissioned) { throw new SMException("Unable to fire an external trigger in an SM-controller that was not commissioned!"); }
            if (this.TriggerActive)
            {
                this.sourceState.SM.TriggerHasBeenFired(this);
            }
            else
            {
                throw new SMException("Unable to fire an inactive external trigger!");
            }
        }

        /// <see cref="SMTrigger.CurrentStateChanged"/>
        protected override void CurrentStateChanged(SMState currState, SMState prevState)
        {
            //    this.triggerActive = (currState == this.sourceState);
        }

        /// <summary>
        /// Gets whether this external trigger can be fired or not.
        /// </summary>
        private bool TriggerActive { get { return this.sourceState.SM.CurrentStateObj == this.sourceState; } }

        /// <summary>
        /// This flag is true if this external trigger can be fired.
        /// </summary>
        //private bool triggerActive;
    }
}
