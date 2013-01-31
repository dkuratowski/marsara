using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.App.PresLogic
{
    /// <summary>
    /// The Multiplayer Chat panel on the Multiplayer Setup page.
    /// </summary>
    public class RCMultiChatPanel : RCAppPanel
    {
        /// <summary>
        /// Creates an RCMultiChatPanel instance.
        /// </summary>
        /// <param name="backgroundRect">The area of the background of the panel in workspace coordinates.</param>
        /// <param name="contentRect">The area of the content of the panel relative to the background rectangle.</param>
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
        public RCMultiChatPanel(RCIntRectangle backgroundRect, RCIntRectangle contentRect,
                                ShowMode showMode, HideMode hideMode,
                                int appearDuration, int disappearDuration,
                                string backgroundSprite)
            : base(backgroundRect, contentRect, showMode, hideMode, appearDuration, disappearDuration, backgroundSprite)
        {
        }
    }
}
