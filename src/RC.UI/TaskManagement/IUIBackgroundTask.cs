using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.UI
{
    /// <summary>
    /// Delegate for handling background task events.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="message">Optional message from the background task.</param>
    public delegate void UITaskEventHdl(IUIBackgroundTask sender, object message);

    /// <summary>
    /// Defines the UI-thread side interface for long-running background tasks.
    /// </summary>
    public interface IUIBackgroundTask
    {
        /// <summary>
        /// This event is raised on the UI-thread if the background task has been finished.
        /// </summary>
        /// <remarks>The message is null.</remarks>
        event UITaskEventHdl Finished;

        /// <summary>
        /// This event is raised on the UI-thread if the background task has sent a message.
        /// </summary>
        /// <remarks>The message depends on the type of the background task.</remarks>
        event UITaskEventHdl Message;

        /// <summary>
        /// This event is raised on the UI-thread if the background task has been failed.
        /// </summary>
        /// <remarks>The message is the exception caught by the background task.</remarks>
        event UITaskEventHdl Failed;
    }
}
