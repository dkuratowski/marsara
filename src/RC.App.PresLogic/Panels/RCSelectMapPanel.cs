using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;

namespace RC.App.PresLogic.Panels
{
    /// <summary>
    /// The map selector panel on the Create Game page
    /// </summary>
    public class RCSelectMapPanel : RCAppPanel
    {
        /// <summary>
        /// Creates an RCSelectMapPanel instance.
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
        public RCSelectMapPanel(RCIntRectangle backgroundRect, RCIntRectangle contentRect,
                                ShowMode showMode, HideMode hideMode,
                                int appearDuration, int disappearDuration,
                                string backgroundSprite)
            : base(backgroundRect, contentRect, showMode, hideMode, appearDuration, disappearDuration, backgroundSprite)
        {
            this.selectMapTitle = new UIString(SELECT_MAP_TITLE, UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font9B"),
                                              UIWorkspace.Instance.PixelScaling, RCColor.LightBlue);
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            base.Render_i(renderContext);
            renderContext.RenderString(this.selectMapTitle, new RCIntVector(1, -8));
        }

        /// <summary>
        /// The UIString displayed as the title of this panel.
        /// </summary>
        private UIString selectMapTitle;

        /// <summary>
        /// Constants of this panel.
        /// </summary>
        private const string SELECT_MAP_TITLE = "Create";
    }
}
