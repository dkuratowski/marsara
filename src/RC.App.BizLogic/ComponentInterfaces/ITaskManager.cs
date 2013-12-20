using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.ComponentModel;

namespace RC.App.BizLogic.ComponentInterfaces
{
    /// <summary>
    /// Represents the starting methods of background tasks.
    /// </summary>
    /// <param name="parameter">An optional parameter for the task.</param>
    public delegate void TaskMethod(object parameter);

    /// <summary>
    /// Represents the handler functions of system update events.
    /// </summary>
    /// <param name="timeSinceLastUpdate">The elapsed time since the last system update in milliseconds.</param>
    /// <param name="timeSinceStart">The elapsed time since the start of the system in milliseconds.</param>
    public delegate void SystemUpdateHdl(int timeSinceLastUpdate, int timeSinceStart);

    /// <summary>
    /// The task manager can be accessed from the BizLogic components using this interface.
    /// </summary>
    [ComponentInterface]
    public interface ITaskManager
    {
        /// <summary>
        /// Starts a background task. Can only be called from the UI-thread.
        /// </summary>
        /// <param name="taskMethod">The starting method of the task.</param>
        /// <param name="name">The name of the executing thread of the task.</param>
        /// <returns>A reference to the interface of the created task.</returns>
        ITask StartTask(TaskMethod taskMethod, string name);

        /// <summary>
        /// Starts a background task with the given parameter. Can only be called from the UI-thread.
        /// </summary>
        /// <param name="taskMethod">The starting method of the task.</param>
        /// <param name="name">The name of the executing thread of the task.</param>
        /// <param name="parameter">The starting parameter of the task.</param>
        /// <returns>A reference to the interface of the created task.</returns>
        ITask StartTask(TaskMethod taskMethod, string name, object parameter);

        /// <summary>
        /// Subscribes to system update events.
        /// </summary>
        /// <param name="handler">The handler function to be called on system updates.</param>
        void SubscribeToSystemUpdate(SystemUpdateHdl handler);

        /// <summary>
        /// Unsubscribes from system update events.
        /// </summary>
        /// <param name="handler">The handler function to unsubscribe.</param>
        void UnsubscribeFromSystemUpdate(SystemUpdateHdl handler);

        /// <summary>
        /// Posts a message back to the UI-thread. Can only be called from the threads of the background tasks.
        /// </summary>
        /// <param name="message">The message to post.</param>
        void PostMessage(object message);
    }

    /// <summary>
    /// Delegate for handling background task events.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="message">Optional message from the background task.</param>
    public delegate void TaskEventHdl(ITask sender, object message);

    /// <summary>
    /// Interface of background tasks.
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// This event is raised on the UI-thread if the background task has been finished.
        /// </summary>
        /// <remarks>The message is null.</remarks>
        event TaskEventHdl Finished;

        /// <summary>
        /// This event is raised on the UI-thread if the background task has sent a message.
        /// </summary>
        /// <remarks>The message depends on the type of the background task.</remarks>
        event TaskEventHdl Message;

        /// <summary>
        /// This event is raised on the UI-thread if the background task has been failed.
        /// </summary>
        /// <remarks>The message is the exception caught by the background task.</remarks>
        event TaskEventHdl Failed;
    }
}
