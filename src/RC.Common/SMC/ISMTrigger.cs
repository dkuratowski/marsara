namespace RC.Common.SMC
{
    /// <summary>
    /// The public interface of an external trigger between SMStates.
    /// </summary>
    public interface ISMTrigger
    {
        /// <summary>
        /// Fires this external trigger.
        /// </summary>
        void Fire();
    }
}
