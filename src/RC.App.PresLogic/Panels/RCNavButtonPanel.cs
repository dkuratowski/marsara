using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;
using RC.App.PresLogic.Controls;

namespace RC.App.PresLogic.Panels
{
    /// <summary>
    /// Represents a panel that contains one navigation button (Ok or Cancel).
    /// </summary>
    public class RCNavButtonPanel : RCAppPanel
    {
        /// <summary>
        /// Creates an RCNavButtonPanel instance.
        /// </summary>
        /// <param name="backgroundRect">The area of the background of the panel in workspace coordinates.</param>
        /// <param name="buttonRect">The area of the button on the panel relative to the background rectangle.</param>
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
        public RCNavButtonPanel(RCIntRectangle backgroundRect, RCIntRectangle buttonRect,
                                ShowMode showMode, HideMode hideMode,
                                int appearDuration, int disappearDuration,
                                string backgroundSprite, string buttonText)
            : base(backgroundRect, buttonRect, showMode, hideMode, appearDuration, disappearDuration, backgroundSprite)
        {
            this.navigationButton = new RCMenuButton(buttonText, this.Clip);
            this.AddControl(this.navigationButton);
        }

        /// <summary>
        /// Gets the navigation button of this panel.
        /// </summary>
        public RCMenuButton NavigationButton { get { return this.navigationButton; } }

        /// <summary>
        /// Reference to the navigation button on this panel.
        /// </summary>
        private RCMenuButton navigationButton;
    }
}
