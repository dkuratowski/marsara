using System;
using System.Collections.Generic;
using RC.App.BizLogic.BusinessComponents;
using RC.Common;
using RC.Common.ComponentModel;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Adapter for the task management subsystem of the RC.UI component.
    /// </summary>
    [Component("RC.App.PresLogic.TaskManagerBC")]
    class TaskManagerBC : ITaskManagerBC
    {
        /// <summary>
        /// Constructs a TaskManagerBC instance.
        /// </summary>
        public TaskManagerBC()
        {
            this.updateHandlers = new RCSet<SystemUpdateHdl>();
        }

        /// <see cref="ITaskManager.StartTask"/>
        public ITask StartTask(TaskMethod taskMethod, string name)
        {
            IUIBackgroundTask adaptedTask =
                UITaskManager.StartParallelTask(delegate(object param) { taskMethod(null); }, name);
            return new TaskAdapter(adaptedTask);
        }

        /// <see cref="ITaskManager.StartTask"/>
        public ITask StartTask(TaskMethod taskMethod, string name, object parameter)
        {
            IUIBackgroundTask adaptedTask =
                UITaskManager.StartParallelTask(delegate(object param) { taskMethod(parameter); }, name);
            return new TaskAdapter(adaptedTask);
        }

        /// <see cref="ITaskManager.SubscribeToSystemUpdate"/>
        public void SubscribeToSystemUpdate(SystemUpdateHdl handler)
        {
            if (this.updateHandlers.Count == 0)
            {
                UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate += this.AdaptedUpdateHdl;
            }
            this.updateHandlers.Add(handler);
        }

        /// <see cref="ITaskManager.UnsubscribeFromSystemUpdate"/>
        public void UnsubscribeFromSystemUpdate(SystemUpdateHdl handler)
        {
            this.updateHandlers.Remove(handler);
            if (this.updateHandlers.Count == 0)
            {
                UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate -= this.AdaptedUpdateHdl;
            }
        }

        /// <see cref="ITaskManager.PostMessage"/>
        public void PostMessage(object message)
        {
            UITaskManager.PostMessage(message);
        }

        /// <summary>
        /// Internal system update handler function.
        /// </summary>
        /// <param name="evtArgs">Timing informations about the system update.</param>
        private void AdaptedUpdateHdl()
        {
            int timeSinceLastUpdate = UIRoot.Instance.GraphicsPlatform.RenderLoop.TimeSinceLastUpdate;
            int timeSinceStart = UIRoot.Instance.GraphicsPlatform.RenderLoop.TimeSinceStart;
            RCSet<SystemUpdateHdl> updateHandlersCopy = new RCSet<SystemUpdateHdl>(this.updateHandlers);
            foreach (SystemUpdateHdl updateHdl in updateHandlersCopy)
            {
                updateHdl(timeSinceLastUpdate, timeSinceStart);
            }
        }

        /// <summary>
        /// The handlers subscribed for system update events.
        /// </summary>
        private RCSet<SystemUpdateHdl> updateHandlers;
    }

    /// <summary>
    /// Adapter for UI background tasks.
    /// </summary>
    class TaskAdapter : ITask
    {
        /// <summary>
        /// Constructs a TaskAdapter instance.
        /// </summary>
        /// <param name="adaptedTask">Reference to the adapted background task.</param>
        public TaskAdapter(IUIBackgroundTask adaptedTask)
        {
            if (adaptedTask == null) { throw new ArgumentNullException("adaptedTask"); }
            this.adaptedTask = adaptedTask;
        }

        /// <see cref="ITask.Finished"/>
        public event TaskEventHdl Finished
        {
            add { this.adaptedTask.Finished += this.AdaptedFinishedHdl; this.finishedInternal += value; }
            remove { this.adaptedTask.Finished -= this.AdaptedFinishedHdl; this.finishedInternal -= value; }
        }

        /// <see cref="ITask.Message"/>
        public event TaskEventHdl Message
        {
            add { this.adaptedTask.Message += this.AdaptedMessageHdl; this.messageInternal += value; }
            remove { this.adaptedTask.Message -= this.AdaptedMessageHdl; this.messageInternal -= value; }
        }

        /// <see cref="ITask.Failed"/>
        public event TaskEventHdl Failed
        {
            add { this.adaptedTask.Failed += this.AdaptedFailedHdl; this.failedInternal += value; }
            remove { this.adaptedTask.Failed -= this.AdaptedFailedHdl; this.failedInternal -= value; }
        }

        /// <summary>
        /// Adapted handler of IUIBackgoundTask.Finished events.
        /// </summary>
        private void AdaptedFinishedHdl(IUIBackgroundTask sender, object message)
        {
            if (sender == this.adaptedTask && this.finishedInternal != null) { this.finishedInternal(this, message); }
        }

        /// <summary>
        /// Adapted handler of IUIBackgoundTask.Message events.
        /// </summary>
        private void AdaptedMessageHdl(IUIBackgroundTask sender, object message)
        {
            if (sender == this.adaptedTask && this.messageInternal != null) { this.messageInternal(this, message); }
        }

        /// <summary>
        /// Adapted handler of IUIBackgoundTask.Failed events.
        /// </summary>
        private void AdaptedFailedHdl(IUIBackgroundTask sender, object message)
        {
            if (sender == this.adaptedTask && this.failedInternal != null) { this.failedInternal(this, message); }
        }

        /// <summary>
        /// Internal events.
        /// </summary>
        private event TaskEventHdl finishedInternal;
        private event TaskEventHdl messageInternal;
        private event TaskEventHdl failedInternal;

        /// <summary>
        /// Reference to the adapted background task.
        /// </summary>
        private IUIBackgroundTask adaptedTask;
    }
}
