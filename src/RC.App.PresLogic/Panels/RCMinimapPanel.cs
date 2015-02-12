using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.App.PresLogic.Controls;
using RC.Common;
using RC.Common.ComponentModel;
using RC.UI;

namespace RC.App.PresLogic.Panels
{
    /// <summary>
    /// The minimap panel on the gameplay page.
    /// </summary>
    public class RCMinimapPanel : RCAppPanel, IGameConnector
    {
        /// <summary>
        /// Constructs a minimap panel.
        /// </summary>
        /// <param name="isoTileSpriteGroup">Reference to the sprites of the isometric tile types.</param>
        /// <param name="terrainObjectSpriteGroup">Reference to the sprites of the terrain object types.</param>
        /// <param name="backgroundRect">The area of the background of the panel in workspace coordinates.</param>
        /// <param name="contentRect">The area of the content of the panel relative to the background rectangle.</param>
        /// <param name="backgroundSprite">Name of the sprite resource that will be the background of this panel or null if there is no background.</param>
        public RCMinimapPanel(ISpriteGroup isoTileSpriteGroup, ISpriteGroup terrainObjectSpriteGroup, RCIntRectangle backgroundRect, RCIntRectangle contentRect, string backgroundSprite)
            : base(backgroundRect, contentRect, ShowMode.Appear, HideMode.Disappear, 0, 0, backgroundSprite)
        {
            this.minimapDisplay = new RCMinimapDisplay(isoTileSpriteGroup, terrainObjectSpriteGroup, new RCIntVector(3, 3), new RCIntVector(64, 64));
            this.minimapDisplay.ConnectorOperationFinished += this.OnMinimapConnectorOperationFinished;
        }

        #region IGameConnector members

        /// <see cref="IGameConnector.Connect"/>
        public void Connect()
        {
            ComponentManager.GetInterface<IScrollService>().AttachMinimap(this.minimapDisplay.Range.Size);
            this.minimapDisplay.Connect();
        }

        /// <see cref="IGameConnector.Disconnect"/>
        public void Disconnect()
        {
            this.RemoveControl(this.minimapDisplay);
            this.minimapDisplay.Disconnect();
        }

        /// <see cref="IGameConnector.ConnectionStatus"/>
        public ConnectionStatusEnum ConnectionStatus { get { return this.minimapDisplay.ConnectionStatus; } }

        /// <see cref="IGameConnector.ConnectorOperationFinished"/>
        public event Action<IGameConnector> ConnectorOperationFinished;

        #endregion IGameConnector members

        /// <summary>
        /// Internal event handler.
        /// </summary>
        private void OnMinimapConnectorOperationFinished(IGameConnector sender)
        {
            if (sender != this.minimapDisplay) { throw new ArgumentException("Unexpected sender of event!"); }

            if (sender.ConnectionStatus == ConnectionStatusEnum.Online) { this.AddControl(this.minimapDisplay); }

            if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
        }

        /// <summary>
        /// Reference to the minimap display control.
        /// </summary>
        private readonly RCMinimapDisplay minimapDisplay;
    }
}
