namespace RC.UI
{
    /// <summary>
    /// Interface of the render manager. The render manager is responsible for organizing the render of a
    /// complete frame.
    /// </summary>
    public interface IUIRenderManager
    {
        /// <summary>
        /// Attaches a UIObject to the render manager. The render manager can only have 1 UIObject attached.
        /// </summary>
        /// <param name="obj">The UIObject to attach.</param>
        /// <exception cref="InvalidOperationException">
        /// If rendering is in progress.
        /// If another UIObject is currently attached to the render manager.
        /// If obj is not a root UIObject.
        /// </exception>
        void Attach(UIObject obj);

        /// <summary>
        /// Detaches the currently attacher UIObject from the render manager.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If rendering is in progress.
        /// If there is no UIObject currently attached to the render manager.
        /// </exception>
        /// <returns>Reference to the detached UIObject.</returns>
        UIObject Detach();

        /// <summary>
        /// Renders the attached UIObject and all of it's children recursively.
        /// </summary>
        /// <param name="screenContext">Render context of the screen.</param>
        /// <exception cref="InvalidOperationException">In case of recursive call on this method.</exception>
        /// <remarks>
        /// If there is no UIObject attached to the render manager then this function has no effect.
        /// </remarks>
        void Render(IUIRenderContext screenContext);

        /// <summary>
        /// Gets the currently attached UIObject or null reference if there is no UIObject currently attached.
        /// </summary>
        UIObject AttachedObject { get; }
    }
}
