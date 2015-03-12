using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Responsible for collecting minimap related data from the active scenario on the UI-thread and render
    /// the collected data into the appropriate buffers of the minimap as a background job.
    /// </summary>
    class MinimapScanner : IMinimapBackgroundJob, IDisposable
    {
        /// <summary>
        /// Constructs a MinimapScanner instance.
        /// </summary>
        /// <param name="minimapView">Reference to the view that is used to collect the data for the rendering.</param>
        public MinimapScanner(IMinimapView minimapView)
        {
            if (minimapView == null) { throw new ArgumentNullException("minimapView"); }

            this.fowBuffer = null;
            this.entitiesBuffer = null;
            this.attackSignalsBuffer = null;
            this.minimapView = minimapView;
            this.currentRow = -1;
            this.isDisposed = false;

            this.blackBrush = new CachedValue<UISprite>(() => this.CreateBrush(RCColor.Black));
            this.grayBrush = new CachedValue<UISprite>(() => this.CreateBrush(RCColor.Gray));
            this.transparentBrush = new CachedValue<UISprite>(() => this.CreateBrush(PresLogicConstants.DEFAULT_TRANSPARENT_COLOR));
            this.friendlyEntityBrush = new CachedValue<UISprite>(() => this.CreateBrush(RCColor.LightGreen));
            this.attackSignalBrush = new CachedValue<UISprite>(() => this.CreateBrush(RCColor.LightRed));
            this.entityBrushes = new Dictionary<PlayerEnum, CachedValue<UISprite>>
            {
                { PlayerEnum.Neutral, new CachedValue<UISprite>(() => this.CreateBrush(RCColor.LightCyan))},
                { PlayerEnum.Player0, new CachedValue<UISprite>(() => this.CreateBrush(PresLogicConstants.PLAYER_COLOR_MAPPINGS[PlayerEnum.Player0]))},
                { PlayerEnum.Player1, new CachedValue<UISprite>(() => this.CreateBrush(PresLogicConstants.PLAYER_COLOR_MAPPINGS[PlayerEnum.Player1]))},
                { PlayerEnum.Player2, new CachedValue<UISprite>(() => this.CreateBrush(PresLogicConstants.PLAYER_COLOR_MAPPINGS[PlayerEnum.Player2]))},
                { PlayerEnum.Player3, new CachedValue<UISprite>(() => this.CreateBrush(PresLogicConstants.PLAYER_COLOR_MAPPINGS[PlayerEnum.Player3]))},
                { PlayerEnum.Player4, new CachedValue<UISprite>(() => this.CreateBrush(PresLogicConstants.PLAYER_COLOR_MAPPINGS[PlayerEnum.Player4]))},
                { PlayerEnum.Player5, new CachedValue<UISprite>(() => this.CreateBrush(PresLogicConstants.PLAYER_COLOR_MAPPINGS[PlayerEnum.Player5]))},
                { PlayerEnum.Player6, new CachedValue<UISprite>(() => this.CreateBrush(PresLogicConstants.PLAYER_COLOR_MAPPINGS[PlayerEnum.Player6]))},
                { PlayerEnum.Player7, new CachedValue<UISprite>(() => this.CreateBrush(PresLogicConstants.PLAYER_COLOR_MAPPINGS[PlayerEnum.Player7]))},
            };

            this.pixelInfoArray = new MinimapPixelInfo[this.minimapView.MinimapPosition.Width, this.minimapView.MinimapPosition.Height];
            for (int row = 0; row < this.minimapView.MinimapPosition.Height; row++)
            {
                for (int col = 0; col < this.minimapView.MinimapPosition.Width; col++)
                {
                    this.pixelInfoArray[col, row] = new MinimapPixelInfo
                    {
                        FOWStatus = FOWTypeEnum.None,
                        EntityIndicatorType = MinimapPixelInfo.EntityIndicatorTypeEnum.None,
                        EntityOwner = PlayerEnum.Neutral
                    };
                }
            }
        }

        #region Public methods for the UI-thread

        /// <summary>
        /// Gets the Fog Of War buffer.
        /// </summary>
        public UISprite FOWBuffer { get { return this.fowBuffer; } }

        /// <summary>
        /// Gets the entities buffer.
        /// </summary>
        public UISprite EntitiesBuffer { get { return this.entitiesBuffer; } }

        /// <summary>
        /// Gets the attack signals buffer.
        /// </summary>
        public UISprite AttackSignalsBuffer { get { return this.attackSignalsBuffer; } }


        /// <summary>
        /// Initializes a scan operation starting from the upper left corner of the minimap on the UI-thread.
        /// </summary>
        /// <param name="fowBuffer">The target buffer of the Fog Of War rendering.</param>
        /// <param name="entitiesBuffer">The target buffer of the entity information rendering.</param>
        /// <param name="attackSignalsBuffer">The target buffer of the attack signal rendering.</param>
        public void InitScan(UISprite fowBuffer, UISprite entitiesBuffer, UISprite attackSignalsBuffer)
        {
            if (this.isDisposed) { throw new ObjectDisposedException("MinimapScanner"); }
            if (this.currentRow != -1) { throw new InvalidOperationException("Scan operation is still in progress!"); }
            if (fowBuffer == null) { throw new ArgumentNullException("fowBuffer"); }
            if (entitiesBuffer == null) { throw new ArgumentNullException("entitiesBuffer"); }
            if (attackSignalsBuffer == null) { throw new ArgumentNullException("attackSignalsBuffer"); }

            this.fowBuffer = fowBuffer;
            this.entitiesBuffer = entitiesBuffer;
            this.attackSignalsBuffer = attackSignalsBuffer;

            this.currentRow = 0;
        }

        /// <summary>
        /// Executes the current scan operation on the UI-thread.
        /// </summary>
        /// <returns>True if the scan operation has finished, false if further calls to MinimapScanner.ExecuteScan are necessary.</returns>
        public bool ExecuteScan()
        {
            if (this.isDisposed) { throw new ObjectDisposedException("MinimapScanner"); }
            if (this.currentRow == -1) { throw new InvalidOperationException("There is no scan operation in progress!"); }

            this.minimapView.RefreshPixelInfos(this.currentRow, SCANNED_ROWS_PER_FRAME, this.pixelInfoArray);
            this.currentRow += SCANNED_ROWS_PER_FRAME;
            if (this.currentRow >= this.minimapView.MinimapPosition.Height)
            {
                /// End of scan operation.
                this.currentRow = -1;
            }
            return this.currentRow == -1;
        }

        #endregion Public methods for the UI-thread

        #region IMinimapBackgroundJob methods

        /// <see cref="IMinimapBackgroundJob.Execute"/>
        public void Execute()
        {
            if (this.isDisposed) { throw new ObjectDisposedException("MinimapScanner"); }

            /// Download the buffers from the graphics device for modification.
            this.fowBuffer.Download();
            this.entitiesBuffer.Download();
            this.attackSignalsBuffer.Download();

            /// Render the pixel informations into the buffers.
            IUIRenderContext fowBufferContext = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateRenderContext(this.fowBuffer);
            IUIRenderContext entitiesBufferContext = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateRenderContext(this.entitiesBuffer);
            IUIRenderContext attackSignalsBufferContext = UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateRenderContext(this.attackSignalsBuffer);
            for (int row = 0; row < this.fowBuffer.Size.Y; row++)
            {
                for (int col = 0; col < this.fowBuffer.Size.X; col++)
                {
                    /// Render the FOW-status of the current pixel into the FOW-buffer.
                    if (this.pixelInfoArray[col, row].FOWStatus == FOWTypeEnum.Full)
                    {
                        fowBufferContext.RenderSprite(this.blackBrush.Value, new RCIntVector(col, row));
                    }
                    else if (this.pixelInfoArray[col, row].FOWStatus == FOWTypeEnum.None)
                    {
                        fowBufferContext.RenderSprite(this.transparentBrush.Value, new RCIntVector(col, row));
                    }
                    else if (this.pixelInfoArray[col, row].FOWStatus == FOWTypeEnum.Partial)
                    {
                        fowBufferContext.RenderSprite((col + row) % 2 == 0 ? this.grayBrush.Value : this.transparentBrush.Value, new RCIntVector(col, row));
                    }

                    /// Render the entity indicator and the attack signal of the current pixel into the entity indicator buffer
                    /// and the attack signals buffer.
                    if (this.pixelInfoArray[col, row].EntityIndicatorType == MinimapPixelInfo.EntityIndicatorTypeEnum.None)
                    {
                        /// No entity, no attack signal.
                        entitiesBufferContext.RenderSprite(this.transparentBrush.Value, new RCIntVector(col, row));
                        attackSignalsBufferContext.RenderSprite(this.transparentBrush.Value, new RCIntVector(col, row));
                    }
                    else if (this.pixelInfoArray[col, row].EntityIndicatorType == MinimapPixelInfo.EntityIndicatorTypeEnum.Friendly)
                    {
                        /// Friendly entity, no attack signal.
                        entitiesBufferContext.RenderSprite(this.friendlyEntityBrush.Value, new RCIntVector(col, row));
                        attackSignalsBufferContext.RenderSprite(this.transparentBrush.Value, new RCIntVector(col, row));
                    }
                    else if (this.pixelInfoArray[col, row].EntityIndicatorType == MinimapPixelInfo.EntityIndicatorTypeEnum.AttackedFriendly)
                    {
                        /// Friendly entity, attack signal.
                        entitiesBufferContext.RenderSprite(this.friendlyEntityBrush.Value, new RCIntVector(col, row));
                        attackSignalsBufferContext.RenderSprite(this.attackSignalBrush.Value, new RCIntVector(col, row));
                    }
                    else if (this.pixelInfoArray[col, row].EntityIndicatorType == MinimapPixelInfo.EntityIndicatorTypeEnum.NonFriendly)
                    {
                        /// Non-friendly entity, no attack signal.
                        PlayerEnum owner = this.pixelInfoArray[col, row].EntityOwner;
                        entitiesBufferContext.RenderSprite(this.entityBrushes[owner].Value, new RCIntVector(col, row));
                        attackSignalsBufferContext.RenderSprite(this.transparentBrush.Value, new RCIntVector(col, row));
                    }
                    else
                    {
                        /// Impossible case -> crash!
                        throw new InvalidOperationException("Impossible case happened!");
                    }
                }
            }
            UIRoot.Instance.GraphicsPlatform.SpriteManager.CloseRenderContext(this.fowBuffer);
            UIRoot.Instance.GraphicsPlatform.SpriteManager.CloseRenderContext(this.entitiesBuffer);
            UIRoot.Instance.GraphicsPlatform.SpriteManager.CloseRenderContext(this.attackSignalsBuffer);

            /// Upload the modified buffers to the graphics device.
            this.fowBuffer.Upload();
            this.entitiesBuffer.Upload();
            this.attackSignalsBuffer.Upload();
        }

        #endregion IMinimapBackgroundJob methods

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.isDisposed) { throw new ObjectDisposedException("MinimapScanner"); }

            UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.blackBrush.Value);
            UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.grayBrush.Value);
            UIRoot.Instance.GraphicsPlatform.SpriteManager.DestroySprite(this.transparentBrush.Value);
            this.isDisposed = true;
        }

        #endregion IDisposable methods

        /// <summary>
        /// Creates a brush with the given color.
        /// </summary>
        private UISprite CreateBrush(RCColor color)
        {
            return UIRoot.Instance.GraphicsPlatform.SpriteManager.CreateSprite(color, new RCIntVector(1, 1), UIWorkspace.Instance.PixelScaling);
        }

        /// <summary>
        /// The index of the currently scanned row or -1 if there is no scan operation in progress.
        /// </summary>
        private int currentRow;

        /// <summary>
        /// The target buffers of the rendering operation.
        /// </summary>
        private UISprite fowBuffer;
        private UISprite entitiesBuffer;
        private UISprite attackSignalsBuffer;

        /// <summary>
        /// The brushes used for rendering.
        /// </summary>
        private CachedValue<UISprite> blackBrush;
        private CachedValue<UISprite> grayBrush;
        private CachedValue<UISprite> transparentBrush;
        private CachedValue<UISprite> friendlyEntityBrush;
        private CachedValue<UISprite> attackSignalBrush;
        private Dictionary<PlayerEnum, CachedValue<UISprite>> entityBrushes;

        /// <summary>
        /// This flag indicates whether this scanner has been disposed or not.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Reference to the view that is used to collect the data for the rendering.
        /// </summary>
        private readonly IMinimapView minimapView;

        /// <summary>
        /// The 2D array that contains the render informations for every minimap pixels.
        /// </summary>
        private readonly MinimapPixelInfo[,] pixelInfoArray;

        /// <summary>
        /// The number of scanned rows per frame.
        /// </summary>
        private const int SCANNED_ROWS_PER_FRAME = 1;
    }
}
