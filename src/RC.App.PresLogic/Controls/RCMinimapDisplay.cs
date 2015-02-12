using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.Common.Diagnostics;
using RC.UI;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Represents the minimap display control.
    /// </summary>
    public class RCMinimapDisplay : UIControl, IGameConnector
    {
        /// <summary>
        /// Constructs a minimap display control at the given position with the given size.
        /// </summary>
        /// <param name="isoTileSpriteGroup">Reference to the sprites of the isometric tile types.</param>
        /// <param name="terrainObjectSpriteGroup">Reference to the sprites of the terrain object types.</param>
        /// <param name="position">The position of the minimap display control.</param>
        /// <param name="size">The size of the minimap display control.</param>
        public RCMinimapDisplay(ISpriteGroup isoTileSpriteGroup, ISpriteGroup terrainObjectSpriteGroup, RCIntVector position, RCIntVector size)
            : base(position, size)
        {
            if (isoTileSpriteGroup == null) { throw new ArgumentNullException("isoTileSpriteGroup"); }
            if (terrainObjectSpriteGroup == null) { throw new ArgumentNullException("terrainObjectSpriteGroup"); }

            this.minimapView = null;
            this.isoTileSpriteGroup = isoTileSpriteGroup;
            this.terrainObjectSpriteGroup = terrainObjectSpriteGroup;

            this.connectionStatus = ConnectionStatusEnum.Offline;
            this.backgroundTask = null;
            this.stopBackgroundTaskEvent = null;
            this.newJobEvent = null;
            this.jobQueue = new Fifo<IMinimapBackgroundJob>(JOB_QUEUE_CAPACITY);

            this.windowLocationBrush = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(RCColor.WhiteHigh, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
            this.windowLocationBrush.Upload();
        }

        /// <summary>
        /// Updates the terrain sprite of the minimap display.
        /// </summary>
        public void UpdateTerrainSprite()
        {
            if (this.connectionStatus != ConnectionStatusEnum.Online) { throw new InvalidOperationException("The minimap display has not yet been connected!"); }

            UISprite spriteToUpdate = this.spriteBuffer.CheckoutTerrainSprite();
            this.terrainUpdateJob = new MinimapTerrainUpdateJob(spriteToUpdate, this.minimapView, this.isoTileSpriteGroup, this.terrainObjectSpriteGroup);
            lock (this.jobQueue) { this.jobQueue.Push(this.terrainUpdateJob); }
            this.newJobEvent.Set();
        }

        #region IGameConnector members

        /// <see cref="IGameConnector.Connect"/>
        public void Connect()
        {
            if (this.connectionStatus != ConnectionStatusEnum.Offline) { throw new InvalidOperationException("The minimap display has been connected or is currently being connected!"); }

            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.minimapView = viewService.CreateView<IMinimapView>();
            this.spriteBuffer = new MinimapSpriteBuffer(this.minimapView, this.isoTileSpriteGroup, this.terrainObjectSpriteGroup);

            this.connectionStatus = ConnectionStatusEnum.Connecting;
            this.stopBackgroundTaskEvent = new AutoResetEvent(false);
            this.newJobEvent = new AutoResetEvent(false);
            this.backgroundTask = UITaskManager.StartParallelTask(this.BackgroundTask, "RCMinimapDisplay.BackgroundTask");
            this.backgroundTask.Message += this.OnBackgroundTaskMessage;
            this.backgroundTask.Finished += this.OnBackgroundTaskFinished;
            this.backgroundTask.Failed += delegate(IUIBackgroundTask sender, object message)
            {
                throw (Exception)message;
            };
        }

        /// <see cref="IGameConnector.Disconnect"/>
        public void Disconnect()
        {
            if (this.connectionStatus != ConnectionStatusEnum.Online) { throw new InvalidOperationException("The minimap display has been disconnected or is currently being disconnected!"); }

            this.connectionStatus = ConnectionStatusEnum.Disconnecting;
            this.minimapView = null;
            this.stopBackgroundTaskEvent.Set();
        }

        /// <see cref="IGameConnector.ConnectionStatus"/>
        public ConnectionStatusEnum ConnectionStatus { get { return this.connectionStatus; } }

        /// <see cref="IGameConnector.ConnectorOperationFinished"/>
        public event Action<IGameConnector> ConnectorOperationFinished;

        #endregion IGameConnector members

        #region Overrides

        /// <see cref="UIObject.Render_i"/>
        protected sealed override void Render_i(IUIRenderContext renderContext)
        {
            /// TODO: implement the rendering operation!
            renderContext.RenderSprite(this.spriteBuffer.TerrainSprite, this.minimapView.MinimapPosition.Location);
            renderContext.RenderRectangle(this.windowLocationBrush, this.minimapView.WindowIndicator);
        }

        #endregion Overrides

        #region Background task related methods

        /// <summary>
        /// Implements the background operations of this minimap display.
        /// </summary>
        /// <param name="nothing">Unused.</param>
        private void BackgroundTask(object nothing)
        {
            this.spriteBuffer.InitBuffers();
            UITaskManager.PostMessage(this.spriteBuffer);

            while (true)
            {
                int evtIndex = WaitHandle.WaitAny(new WaitHandle[] { this.newJobEvent, this.stopBackgroundTaskEvent });
                if (evtIndex == 0)
                {
                    // New job arrived...
                    while (true)
                    {
                        IMinimapBackgroundJob job;
                        lock (this.jobQueue)
                        {
                            if (this.jobQueue.Length == 0) { break; }
                            job = this.jobQueue.Get();
                        }
                        job.Execute();
                        UITaskManager.PostMessage(job);
                    }
                }
                else if (evtIndex == 1)
                {
                    // Stop background task...
                    break;
                }
                else
                {
                    throw new InvalidOperationException("Impossible case happened!");
                }
            }

            this.spriteBuffer.DestroyBuffers();
        }

        /// <summary>
        /// This method is called on the UI-thread when the minimap background task sent a message.
        /// </summary>
        /// <param name="sender">Reference to the background task.</param>
        /// <param name="finishedJob">The job that has been executed by the background task.</param>
        private void OnBackgroundTaskMessage(IUIBackgroundTask sender, object finishedJob)
        {
            if (sender != this.backgroundTask) { throw new InvalidOperationException("Unexpected sender!"); }

            if (finishedJob == this.spriteBuffer)
            {
                this.connectionStatus = ConnectionStatusEnum.Online;
                if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
            }
            else if (finishedJob == this.terrainUpdateJob)
            {
                this.spriteBuffer.CheckinTerrainSprite(this.terrainUpdateJob.TargetSprite);
                this.terrainUpdateJob = null;
            }

            TraceManager.WriteAllTrace("Message arrived from background task...", PresLogicTraceFilters.INFO);
        }

        /// <summary>
        /// This method is called on the UI-thread when the minimap background task has been finished.
        /// </summary>
        /// <param name="sender">Reference to the background task.</param>
        /// <param name="nothing">Unused.</param>
        private void OnBackgroundTaskFinished(IUIBackgroundTask sender, object nothing)
        {
            if (sender != this.backgroundTask) { throw new InvalidOperationException("Unexpected sender!"); }

            this.backgroundTask = null;
            this.spriteBuffer = null;
            this.terrainUpdateJob = null;
            this.connectionStatus = ConnectionStatusEnum.Offline;
            this.stopBackgroundTaskEvent.Close();
            this.newJobEvent.Close();
            this.stopBackgroundTaskEvent = null;
            this.newJobEvent = null;

            if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
        }

        #endregion Background task related methods

        /// <summary>
        /// Event to stop the background task.
        /// </summary>
        private AutoResetEvent stopBackgroundTaskEvent;

        /// <summary>
        /// Event to ask the background task to perform the jobs in the job queue.
        /// </summary>
        private AutoResetEvent newJobEvent;

        /// <summary>
        /// The current connection status of this minimap display.
        /// </summary>
        private ConnectionStatusEnum connectionStatus;

        /// <summary>
        /// Reference to the background task of this minimap display control or null if the background task is not running.
        /// </summary>
        private IUIBackgroundTask backgroundTask;

        /// <summary>
        /// Reference to the minimap view.
        /// </summary>
        private IMinimapView minimapView;

        /// <summary>
        /// Reference to the sprite buffer of this minimap display.
        /// </summary>
        private MinimapSpriteBuffer spriteBuffer;

        /// <summary>
        /// Reference to the current terrain update job.
        /// </summary>
        private MinimapTerrainUpdateJob terrainUpdateJob;

        /// <summary>
        /// Reference to the job queue.
        /// </summary>
        private Fifo<IMinimapBackgroundJob> jobQueue;

        /// <summary>
        /// Brush for displaying the current location of the map window on the minimap.
        /// </summary>
        private readonly UISprite windowLocationBrush;

        /// <summary>
        /// Reference to the sprites of the isometric tile types.
        /// </summary>
        private readonly ISpriteGroup isoTileSpriteGroup;

        /// <summary>
        /// Reference to the sprites of the terrain object types.
        /// </summary>
        private readonly ISpriteGroup terrainObjectSpriteGroup;

        /// <summary>
        /// The capacity of the job queue.
        /// </summary>
        private const int JOB_QUEUE_CAPACITY = 1024;
    }
}
