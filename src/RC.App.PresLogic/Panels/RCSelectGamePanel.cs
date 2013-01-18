using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// The Select Game panel on the Select Game page.
    /// </summary>
    public class RCSelectGamePanel : RCAppPanel
    {
        /// <summary>
        /// Creates an RCSelectGamePanel instance.
        /// </summary>
        /// <param name="backgroundRect">The area of the background of the panel in workspace coordinates.</param>
        /// <param name="buttonRect">The area of the button inside the panel relative to the background rectangle.</param>
        /// <param name="showMode">The mode how the panel will appear on a page when being shown.</param>
        /// <param name="hideMode">The mode how the panel will disappear from a page when being hidden.</param>
        /// <param name="appearDuration">
        /// The duration of showing this UIPanel in milliseconds. This parameter will be ignored in case
        /// of ShowMode.Appear.
        /// </param>
        /// <param name="disappearDuration">
        /// The duration of hiding this UIPanel in milliseconds. This parameter will be ignored in case
        /// of HideMode.Disappear.
        /// </param>
        /// <param name="backgroundSprite">
        /// Name of the sprite resource that will be the background of this panel or null if there is no background.
        /// </param>
        /// <param name="buttonText">The text that should be displayed on the navigation button.</param>
        public RCSelectGamePanel(RCIntRectangle backgroundRect, RCIntRectangle buttonRect,
                                 ShowMode showMode, HideMode hideMode,
                                 int appearDuration, int disappearDuration,
                                 string backgroundSprite)
            : base(backgroundRect, buttonRect, showMode, hideMode, appearDuration, disappearDuration, backgroundSprite)
        {
            this.selectGameTitle = new UIString(SELECT_GAME_TITLE, UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font9B"),
                                              UIWorkspace.Instance.PixelScaling, UIColor.LightBlue);
            this.createGameButton = new RCMenuButton(CREATE_GAME_BUTTON, new RCIntRectangle(0, 99, 85, 15));
            this.AddControl(this.createGameButton);
        }

        /// <summary>
        /// Gets a reference to the "Create Game" button.
        /// </summary>
        public RCMenuButton CreateGameButton { get { return this.createGameButton; } }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            base.Render_i(renderContext);
            renderContext.RenderString(this.selectGameTitle, new RCIntVector(1, -8));
        }

        /// <summary>
        /// The UIString displayed as the title of this panel.
        /// </summary>
        private UIString selectGameTitle;

        /// <summary>
        /// Reference to the "Create Game" button.
        /// </summary>
        private RCMenuButton createGameButton;

        /// <summary>
        /// Constants of this panel.
        /// </summary>
        private const string SELECT_GAME_TITLE = "Games";
        private const string CREATE_GAME_BUTTON = "Create Game";
    }
}
