using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;
using RC.App.BizLogic.Views;
using RC.Common.ComponentModel;
using RC.App.BizLogic.Services;
using RC.Common.Diagnostics;

namespace RC.App.PresLogic.Panels
{
    /// <summary>
    /// The resource bar on the gameplay page
    /// </summary>
    public class RCResourceBar : RCAppPanel, IGameConnector
    {
        /// <summary>
        /// Constructs a resource bar.
        /// </summary>
        /// <param name="backgroundRect">The area of the background of the panel in workspace coordinates.</param>
        /// <param name="contentRect">The area of the content of the panel relative to the background rectangle.</param>
        /// <param name="backgroundSprite">Name of the sprite resource that will be the background of this panel or null if there is no background.</param>
        public RCResourceBar(RCIntRectangle backgroundRect, RCIntRectangle contentRect, string backgroundSprite)
            : base(backgroundRect, contentRect, ShowMode.Appear, HideMode.Disappear, 0, 0, backgroundSprite)
        {
            this.multiplayerService = ComponentManager.GetInterface<IMultiplayerService>();

            this.textFont = UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font5");
            this.mineralsText = new UIString("{0}", this.textFont, UIWorkspace.Instance.PixelScaling, RCColor.White);
            this.vespeneGasText = new UIString("{0}", this.textFont, UIWorkspace.Instance.PixelScaling, RCColor.White);
            this.normalUsedSupplyText = new UIString("{0}", this.textFont, UIWorkspace.Instance.PixelScaling, RCColor.White);
            this.criticalUsedSupplyText = new UIString("{0}", this.textFont, UIWorkspace.Instance.PixelScaling, RCColor.LightRed);
            this.totalSupplyText = new UIString("/{0}", this.textFont, UIWorkspace.Instance.PixelScaling, RCColor.White);

            this.playerView = null;
            this.mineralsTracker = null;
            this.vespeneGasTracker = null;
        }

        #region IGameConnector members

        /// <see cref="IGameConnector.Connect"/>
        public void Connect()
        {
            if (this.isConnected) { throw new InvalidOperationException("The resource bar has been connected or is currently being connected!"); }

            this.playerView = ComponentManager.GetInterface<IViewService>().CreateView<IPlayerView>();
            this.mineralsTracker = new SignalTracker();
            this.vespeneGasTracker = new SignalTracker();
            UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate += this.OnFrameUpdate;
            this.multiplayerService.GameUpdated += this.OnGameUpdate;

            this.isConnected = true;
            if (this.connectorOperationFinished != null) { this.connectorOperationFinished(this); }
        }

        /// <see cref="IGameConnector.Disconnect"/>
        public void Disconnect()
        {
            if (!this.isConnected) { throw new InvalidOperationException("The resource bar has been connected or is currently being connected!"); }

            /// Unsubscribe from frame update.
            UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate -= this.OnFrameUpdate;
            this.multiplayerService.GameUpdated -= this.OnGameUpdate;
            this.playerView = null;
            this.mineralsTracker = null;
            this.vespeneGasTracker = null;

            this.isConnected = false;
            if (this.connectorOperationFinished != null) { this.connectorOperationFinished(this); }
        }

        /// <see cref="IGameConnector.ConnectionStatus"/>
        public ConnectionStatusEnum ConnectionStatus
        {
            get { return this.isConnected ? ConnectionStatusEnum.Online : ConnectionStatusEnum.Offline; }
        }

        /// <see cref="IGameConnector.ConnectorOperationFinished"/>
        public event Action<IGameConnector> ConnectorOperationFinished
        {
            add { this.connectorOperationFinished += value; }
            remove { this.connectorOperationFinished -= value; }
        }

        #endregion IGameConnector members

        #region Overrides

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            base.Render_i(renderContext);
            if (this.ConnectionStatus != ConnectionStatusEnum.Online) { return; }

            renderContext.RenderString(this.mineralsText, MINERAL_TEXT_POS, MINERAL_VESPENE_TEXT_MAXWIDTH);
            renderContext.RenderString(this.vespeneGasText, VESPENE_TEXT_POS, MINERAL_VESPENE_TEXT_MAXWIDTH);

            UIString usedSupplyToRender = this.playerView.UsedSupply > this.playerView.TotalSupply ? this.criticalUsedSupplyText : this.normalUsedSupplyText;
            renderContext.RenderString(usedSupplyToRender, SUPPLY_TEXT_POS, SUPPLY_TEXT_MAXWIDTH);
            renderContext.RenderString(this.totalSupplyText,
                SUPPLY_TEXT_POS + new RCIntVector(usedSupplyToRender.Width + 1, 0),
                SUPPLY_TEXT_MAXWIDTH - usedSupplyToRender.Width - 1);
        }

        #endregion Overrides

        /// <summary>
        /// This method is called on every UI updates.
        /// </summary>
        private void OnFrameUpdate()
        {
            int mineralsDelayed = this.mineralsTracker.GetDelayedValue(UIRoot.Instance.GraphicsPlatform.RenderLoop.TimeSinceStart);
            int vespeneGasDelayed = this.vespeneGasTracker.GetDelayedValue(UIRoot.Instance.GraphicsPlatform.RenderLoop.TimeSinceStart);

            this.mineralsText[0] = mineralsDelayed;
            this.vespeneGasText[0] = vespeneGasDelayed;
            this.normalUsedSupplyText[0] = this.playerView.UsedSupply;
            this.criticalUsedSupplyText[0] = this.playerView.UsedSupply;
            this.totalSupplyText[0] = this.playerView.TotalSupply;
        }

        /// <summary>
        /// This method is called on every simulation frame updates.
        /// </summary>
        private void OnGameUpdate()
        {
            this.mineralsTracker.SetSignalValue(this.playerView.Minerals);
            this.vespeneGasTracker.SetSignalValue(this.playerView.VespeneGas);
        }
                
        /// <summary>
        /// This flag indicates whether this resource bar has been connected or not.
        /// </summary>
        private bool isConnected;

        /// <summary>
        /// This event is raised when the actual connector operation has been finished.
        /// </summary>
        private event Action<IGameConnector> connectorOperationFinished;

        /// <summary>
        /// Reference to the player view.
        /// </summary>
        private IPlayerView playerView;

        /// <summary>
        /// Reference to the multiplayer service.
        /// </summary>
        private IMultiplayerService multiplayerService;

        /// <summary>
        /// SignalTrackers for displaying the values delayed.
        /// </summary>
        private SignalTracker mineralsTracker;
        private SignalTracker vespeneGasTracker;

        /// <summary>
        /// The UIString that are used to render the resource values.
        /// </summary>
        private readonly UIString mineralsText;
        private readonly UIString vespeneGasText;
        private readonly UIString normalUsedSupplyText;
        private readonly UIString criticalUsedSupplyText;
        private readonly UIString totalSupplyText;

        /// <summary>
        /// The font that is used for rendering the resource amounts.
        /// </summary>
        private readonly UIFont textFont;

        private static readonly RCIntVector MINERAL_TEXT_POS = new RCIntVector(12, 7);
        private static readonly RCIntVector VESPENE_TEXT_POS = new RCIntVector(42, 7);
        private static readonly RCIntVector SUPPLY_TEXT_POS = new RCIntVector(72, 7);
        private const int MINERAL_VESPENE_TEXT_MAXWIDTH = 19;
        private const int SUPPLY_TEXT_MAXWIDTH = 35;
    }
}
