using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.App.BizLogic.Services;
using RC.App.BizLogic.Views;
using RC.Common;
using RC.Common.ComponentModel;
using RC.UI;

namespace RC.App.PresLogic.Controls
{
    /// <summary>
    /// Represents a button on the command panel of the gameplay page.
    /// </summary>
    public class RCCommandButton : UIButton
    {
        /// <summary>
        /// Constructs a command button at the given rectangular area inside the command panel.
        /// </summary>
        /// <param name="slotCoords">The coordinates of the slot of this button on the command panel (row; col).</param>
        /// <param name="commandButtonSprites">
        /// List of the command button sprite groups mapped by the appropriate button state.
        /// </param>
        public RCCommandButton(RCIntVector slotCoords, Dictionary<CmdButtonStateEnum, SpriteGroup> cmdButtonSprites)
            : base(BUTTON_POSITIONS[slotCoords.X, slotCoords.Y].Location, BUTTON_POSITIONS[slotCoords.X, slotCoords.Y].Size)
        {
            if (slotCoords == RCIntVector.Undefined) { throw new ArgumentNullException("slotCoords"); }
            if (cmdButtonSprites == null) { throw new ArgumentNullException("cmdButtonSprites"); }

            this.slotCoords = slotCoords;
            this.commandButtonSprites = cmdButtonSprites;
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.commandPanelView = viewService.CreateView<ICommandPanelView>();
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            CmdButtonStateEnum buttonState = this.commandPanelView.GetCmdButtonState(this.slotCoords.X, this.slotCoords.Y);
            if (buttonState == CmdButtonStateEnum.None) { return; }
            if (!this.commandButtonSprites.ContainsKey(buttonState)) { return; }

            SpriteInst renderedSprite = this.commandPanelView.GetCmdButtonSprite(this.slotCoords.X, this.slotCoords.Y);
            renderContext.RenderSprite(this.commandButtonSprites[buttonState][renderedSprite.Index],
                                       renderedSprite.DisplayCoords,
                                       renderedSprite.Section);
        }

        /// <summary>
        /// Reference to the command panel view.
        /// </summary>
        private ICommandPanelView commandPanelView;

        /// <summary>
        /// The coordinates of the slot of this button on the command panel (row; col).
        /// </summary>
        private RCIntVector slotCoords;

        /// <summary>
        /// List of the command button sprite groups mapped by the appropriate button state.
        /// </summary>
        private Dictionary<CmdButtonStateEnum, SpriteGroup> commandButtonSprites;

        /// <summary>
        /// The position of the command buttons inside the command panel based on their position.
        /// </summary>
        private static readonly RCIntRectangle[,] BUTTON_POSITIONS = new RCIntRectangle[3, 3]
        {
            { new RCIntRectangle(1, 1, 20, 20), new RCIntRectangle(22, 1, 20, 20), new RCIntRectangle(43, 1, 20, 20) },
            { new RCIntRectangle(1, 22, 20, 20), new RCIntRectangle(22, 22, 20, 20), new RCIntRectangle(43, 22, 20, 20) },
            { new RCIntRectangle(1, 43, 20, 20), new RCIntRectangle(22, 43, 20, 20), new RCIntRectangle(43, 43, 20, 20) },
        };
    }
}
