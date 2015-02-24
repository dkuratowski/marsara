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
            this.minimapScanner = null;
            this.scannerStatus = ScannerStatusEnum.Inactive;
            this.mouseHandler = null;
            this.showAttackSignalsFlag = false;
            this.timeSinceAttackSignalFlagChanged = 0;
            this.spriteBuffer = null;
            this.isoTileSpriteGroup = isoTileSpriteGroup;
            this.terrainObjectSpriteGroup = terrainObjectSpriteGroup;

            this.connectionStatus = ConnectionStatusEnum.Offline;
            this.backgroundTask = null;
            this.stopBackgroundTaskEvent = null;
            this.newJobEvent = null;
            this.jobQueue = new Fifo<IMinimapBackgroundJob>();

            this.crosshairsPointer = UIResourceManager.GetResource<UIPointer>("RC.App.Pointers.CrosshairsPointer");
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
            MinimapTerrainRenderJob terrainUpdateJob = new MinimapTerrainRenderJob(spriteToUpdate, this.minimapView, this.isoTileSpriteGroup, this.terrainObjectSpriteGroup);
            lock (this.jobQueue) { this.jobQueue.Push(terrainUpdateJob); }
            this.newJobEvent.Set();
        }

        #region IGameConnector members

        /// <see cref="IGameConnector.Connect"/>
        public void Connect()
        {
            if (this.connectionStatus != ConnectionStatusEnum.Offline) { throw new InvalidOperationException("The minimap display has been connected or is currently being connected!"); }

            TraceManager.WriteAllTrace("Start minimap connection...", PresLogicTraceFilters.INFO);

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

            TraceManager.WriteAllTrace("Start minimap disconnection...", PresLogicTraceFilters.INFO);

            this.scannerStatus = ScannerStatusEnum.Inactive;
            this.connectionStatus = ConnectionStatusEnum.Disconnecting;
            this.mouseHandler.Dispose();
            this.mouseHandler = null;
            this.minimapView = null;
            this.stopBackgroundTaskEvent.Set();
        }

        /// <see cref="IGameConnector.ConnectionStatus"/>
        public ConnectionStatusEnum ConnectionStatus { get { return this.connectionStatus; } }

        /// <see cref="IGameConnector.ConnectorOperationFinished"/>
        public event Action<IGameConnector> ConnectorOperationFinished;

        #endregion IGameConnector members

        #region Overrides

        /// <see cref="UISensitiveObject.GetMousePointer"/>
        public override UIPointer GetMousePointer(RCIntVector localPosition)
        {
            return this.mouseHandler != null && this.mouseHandler.DisplayCrosshairs ? this.crosshairsPointer : null;
        }

        /// <see cref="UIObject.Render_i"/>
        protected sealed override void Render_i(IUIRenderContext renderContext)
        {
            renderContext.RenderSprite(this.spriteBuffer.TerrainSprite, this.minimapView.MinimapPosition.Location);
            renderContext.RenderSprite(this.spriteBuffer.FOWSprite, this.minimapView.MinimapPosition.Location);
            renderContext.RenderSprite(this.spriteBuffer.EntitiesSprite, this.minimapView.MinimapPosition.Location);
            if (this.showAttackSignalsFlag)
            {
                renderContext.RenderSprite(this.spriteBuffer.AttackSignalsSprite, this.minimapView.MinimapPosition.Location);
            }
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

            this.minimapScanner.Dispose();
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
                if (this.scannerStatus == ScannerStatusEnum.Inactive)
                {
                    /// Connection completed, start the first scan operation.
                    TraceManager.WriteAllTrace("Minimap connection completed", PresLogicTraceFilters.INFO);
                    this.minimapScanner = new MinimapScanner(this.minimapView);
                    this.minimapScanner.InitScan(this.spriteBuffer.CheckoutFOWSprite(),
                                                 this.spriteBuffer.CheckoutEntitiesSprite(),
                                                 this.spriteBuffer.CheckoutAttackSignalsSprite());
                    this.scannerStatus = ScannerStatusEnum.Scanning;
                    this.mouseHandler = new MinimapMouseHandler(this);
                    this.showAttackSignalsFlag = false;
                    this.timeSinceAttackSignalFlagChanged = 0;
                    UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate += this.OnExecuteScan;

                    this.connectionStatus = ConnectionStatusEnum.Online;
                    if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
                }
            }
            else if (finishedJob is MinimapTerrainRenderJob)
            {
                /// Terrain rendering completed.
                TraceManager.WriteAllTrace("Minimap terrain refresh completed", PresLogicTraceFilters.INFO);
                this.spriteBuffer.CheckinTerrainSprite((finishedJob as MinimapTerrainRenderJob).Result);
            }
            else if (finishedJob == this.minimapScanner)
            {
                if (this.scannerStatus == ScannerStatusEnum.Rendering)
                {
                    /// Rendering completed, start the next scan operation.
                    TraceManager.WriteAllTrace("Minimap refresh completed", PresLogicTraceFilters.INFO);
                    this.spriteBuffer.CheckinFOWSprite(this.minimapScanner.FOWBuffer);
                    this.spriteBuffer.CheckinEntitiesSprite(this.minimapScanner.EntitiesBuffer);
                    this.spriteBuffer.CheckinAttackSignalsSprite(this.minimapScanner.AttackSignalsBuffer);
                    this.minimapScanner.InitScan(this.spriteBuffer.CheckoutFOWSprite(),
                                                 this.spriteBuffer.CheckoutEntitiesSprite(),
                                                 this.spriteBuffer.CheckoutAttackSignalsSprite());
                    this.scannerStatus = ScannerStatusEnum.Scanning;
                }
            }
        }

        /// <summary>
        /// This method is called on the UI-thread when the minimap background task has been finished.
        /// </summary>
        /// <param name="sender">Reference to the background task.</param>
        /// <param name="nothing">Unused.</param>
        private void OnBackgroundTaskFinished(IUIBackgroundTask sender, object nothing)
        {
            if (sender != this.backgroundTask) { throw new InvalidOperationException("Unexpected sender!"); }

            UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate -= this.OnExecuteScan;
            this.showAttackSignalsFlag = false;
            this.timeSinceAttackSignalFlagChanged = 0;
            this.backgroundTask = null;
            this.minimapView = null;
            this.minimapScanner = null;
            this.spriteBuffer = null;
            this.connectionStatus = ConnectionStatusEnum.Offline;
            this.stopBackgroundTaskEvent.Close();
            this.newJobEvent.Close();
            this.stopBackgroundTaskEvent = null;
            this.newJobEvent = null;

            TraceManager.WriteAllTrace("Minimap disconnection completed", PresLogicTraceFilters.INFO);

            if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
        }

        /// <summary>
        /// Execute the current scan operation on the UI-thread.
        /// </summary>
        private void OnExecuteScan()
        {
            /// Update the attack signal flag.
            this.timeSinceAttackSignalFlagChanged += UIRoot.Instance.GraphicsPlatform.RenderLoop.TimeSinceLastUpdate;
            if (this.timeSinceAttackSignalFlagChanged > ATTACKSIGNAL_FLASHTIME)
            {
                this.timeSinceAttackSignalFlagChanged = 0;
                this.showAttackSignalsFlag = !this.showAttackSignalsFlag;
            }

            /// Continue the scan operation if we are in scanning state.
            if (this.scannerStatus != ScannerStatusEnum.Scanning) { return; }
            if (this.minimapScanner.ExecuteScan())
            {
                /// Scan operation finished, start the rendering on the background task.
                TraceManager.WriteAllTrace("Start minimap refresh...", PresLogicTraceFilters.INFO);
                this.scannerStatus = ScannerStatusEnum.Rendering;
                lock (this.jobQueue) { this.jobQueue.Push(this.minimapScanner); }
                this.newJobEvent.Set();
            }
        }

        #endregion Background task related methods

        /// <summary>
        /// Enumerates the possible state of the minimap scanner.
        /// </summary>
        private enum ScannerStatusEnum
        {
            Inactive = 0,   /// Neither scanning nor rendering is in progress.
            Scanning = 1,   /// Scanning is in progress on the UI-thread.
            Rendering = 2   /// Rendering is in progress on the background task.
        }

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
        /// Reference to the minimap scanner if this minimap display is connected; otherwise null.
        /// </summary>
        private MinimapScanner minimapScanner;

        /// <summary>
        /// The current status of the minimap scanner.
        /// </summary>
        private ScannerStatusEnum scannerStatus;

        /// <summary>
        /// Reference to the mouse handler of this minimap display control.
        /// </summary>
        private MinimapMouseHandler mouseHandler;

        /// <summary>
        /// Reference to the job queue.
        /// </summary>
        private readonly Fifo<IMinimapBackgroundJob> jobQueue;

        /// <summary>
        /// Brush for displaying the current location of the map window on the minimap.
        /// </summary>
        private readonly UISprite windowLocationBrush;

        /// <summary>
        /// Crosshairs pointer.
        /// </summary>
        private readonly UIPointer crosshairsPointer;

        /// <summary>
        /// Fields for flashing the attack signals.
        /// </summary>
        private bool showAttackSignalsFlag;
        private int timeSinceAttackSignalFlagChanged;

        /// <summary>
        /// Reference to the sprites of the isometric tile types.
        /// </summary>
        private readonly ISpriteGroup isoTileSpriteGroup;

        /// <summary>
        /// Reference to the sprites of the terrain object types.
        /// </summary>
        private readonly ISpriteGroup terrainObjectSpriteGroup;

        /// <summary>
        /// The flashing time of the attack signals in milliseconds.
        /// </summary>
        private const int ATTACKSIGNAL_FLASHTIME = 250;
    }
}
