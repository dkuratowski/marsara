namespace RC.DssServices.Test
{
    /// <summary>
    /// This interface is used to call the UI from a different thread than the UI-thread.
    /// </summary>
    public interface IUiInvoke
    {
        /// <summary>
        /// Call this function from another thread if you want to call the UI.
        /// </summary>
        /// <param name="uiCall">This object represents the call.</param>
        void InvokeUI(SynchronUiCall uiCall);

        /// <summary>
        /// Invalidates the given area of the display on the UI.
        /// </summary>
        /// <param name="invalidArea"></param>
        void InvalidateDisplay(/*Rectangle invalidArea*/);

        /// <summary>
        /// Call this function to move the UI back to inactive status.
        /// </summary>
        /// <remarks>
        /// This function must be called from the context of the UI-thread. Otherwise you must use the EndOfDssUiCall
        /// class.
        /// </remarks>
        void EndOfDss();
    }
}
