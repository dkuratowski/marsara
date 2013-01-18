using System.Drawing;

namespace RC.RenderSystem
{
    /// <summary>
    /// This is the interface of any object that wants to be notified about events during frame rendering
    /// in a render loop.
    /// </summary>
    public interface IRenderLoopListener
    {
        /// <summary>
        /// This function is called by the render system at the very beginning of every frame.
        /// </summary>
        /// <param name="timeSinceLastFrameFinish">
        /// The elapsed time since the finish of the last frame in milliseconds. This value is 0 if this is
        /// the first frame.
        /// </param>
        /// <returns>
        /// The implementor should return true otherwise the render loop will stop.
        /// </returns>
        bool CreateFrameBegin(long timeSinceLastFrameFinish);

        /// <summary>
        /// This function is called just before the frame buffer is being locked for drawing.
        /// </summary>
        /// <param name="timeSinceFrameBegin">
        /// The elapsed time since the beginning of the current frame in milliseconds.
        /// </param>
        /// <remarks>
        /// The frame buffer cannot be accessed between DrawBegin() and DrawFinish() calls.
        /// </remarks>
        void DrawBegin(long timeSinceFrameBegin);

        /// <summary>
        /// This function is called just after the frame buffer is being unlocked as drawing completed.
        /// </summary>
        /// <param name="timeSinceDrawBegin">
        /// The elapsed time since the beginning of the draw operation in milliseconds. This is the time
        /// duration while the frame buffer was locked.
        /// </param>
        /// <param name="refreshedArea">
        /// The bounding box of the regions on the Display that has been refreshed (the values of this
        /// rectangle are in logical pixels).
        /// </param>
        /// <remarks>
        /// The frame buffer cannot be accessed between DrawBegin() and DrawFinish() calls.
        /// </remarks>
        void DrawFinish(long timeSinceDrawBegin, Rectangle refreshedArea);

        /// <summary>
        /// This function is called when rendering the current frame has completely finished.
        /// </summary>
        /// <param name="timeSinceLastFrameFinish">
        /// The elapsed time since the finish of the previous frame in milliseconds. This value is 0 if this
        /// is the first frame.
        /// </param>
        /// <param name="timeSinceThisFrameBegin">
        /// The elapsed time since the beginning of the current frame in milliseconds.
        /// </param>
        /// <param name="timeSinceDrawFinish">
        /// The elapsed time since the draw operation has finished and the frame buffer has been unlocked.
        /// </param>
        /// <returns>
        /// The implementor should return true otherwise the render loop will stop.
        /// </returns>
        bool CreateFrameFinish(long timeSinceLastFrameFinish,
                               long timeSinceThisFrameBegin,
                               long timeSinceDrawFinish);
    }
}
