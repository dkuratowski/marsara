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
    public class RCCommandButton : UIButton, IDisposable
    {
        /// <summary>
        /// Constructs a command button at the given rectangular area inside the command panel.
        /// </summary>
        /// <param name="slotCoords">The coordinates of the slot of this button on the command panel (row; col).</param>
        /// <param name="commandButtonSprites">
        /// List of the command button sprite groups mapped by the appropriate button state.
        /// </param>
        public RCCommandButton(RCIntVector slotCoords, Dictionary<CommandButtonStateEnum, SpriteGroup> cmdButtonSprites)
            : base(BUTTON_POSITIONS[slotCoords.X, slotCoords.Y].Location, BUTTON_POSITIONS[slotCoords.X, slotCoords.Y].Size)
        {
            if (slotCoords == RCIntVector.Undefined) { throw new ArgumentNullException("slotCoords"); }
            if (cmdButtonSprites == null) { throw new ArgumentNullException("cmdButtonSprites"); }

            this.slotCoords = slotCoords;
            this.commandButtonSprites = cmdButtonSprites;
            IViewService viewService = ComponentManager.GetInterface<IViewService>();
            this.commandPanelView = viewService.CreateView<ICommandView>();
            this.commandService = ComponentManager.GetInterface<ICommandService>();
            this.Pressed += this.OnButtonPressed;
        }

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            this.Pressed -= this.OnButtonPressed;
        }

        #endregion IDisposable members

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            CommandButtonStateEnum buttonState = this.commandPanelView.GetCmdButtonState(this.slotCoords);
            if (buttonState == CommandButtonStateEnum.Invisible) { return; }
            if (!this.commandButtonSprites.ContainsKey(buttonState)) { return; }

            SpriteInst renderedSprite = this.commandPanelView.GetCmdButtonSprite(this.slotCoords);
            renderContext.RenderSprite(this.commandButtonSprites[buttonState][renderedSprite.Index],
                                       renderedSprite.DisplayCoords,
                                       renderedSprite.Section);
        }

        /// <summary>
        /// This method is called when this RCCommandButton has been pressed.
        /// </summary>
        private void OnButtonPressed(UISensitiveObject sender)
        {
            if (sender != this) { throw new InvalidOperationException("Unexpected sender!"); }

            CommandButtonStateEnum buttonState = this.commandPanelView.GetCmdButtonState(this.slotCoords);
            if (buttonState == CommandButtonStateEnum.Invisible || buttonState == CommandButtonStateEnum.Disabled) { return; }
            this.commandService.PressCommandButton(this.slotCoords);
        }

        /// <summary>
        /// Reference to the command panel view.
        /// </summary>
        private ICommandView commandPanelView;

        /// <summary>
        /// Reference to the command service.
        /// </summary>
        private ICommandService commandService;

        /// <summary>
        /// The coordinates of the slot of this button on the command panel (row; col).
        /// </summary>
        private RCIntVector slotCoords;

        /// <summary>
        /// List of the command button sprite groups mapped by the appropriate button state.
        /// </summary>
        private Dictionary<CommandButtonStateEnum, SpriteGroup> commandButtonSprites;

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
