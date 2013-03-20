using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;

namespace RC.App.PresLogic.Panels
{
    /// <summary>
    /// Represents a panel in the RC application.
    /// </summary>
    public class RCAppPanel : UIPanel
    {
        /// <summary>
        /// Creates an RCAppPanel instance.
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
        /// <remarks>
        /// The backgroundRect shall entirely contain the contentRect.
        /// The origin of the panel's coordinate system will be the top-left corner of contentRect.
        /// The range rectangle of the panel will be backgroundRect relative to contentRect.
        /// The clip rectangle of the panel will be contentRect in the panel's coordinate system.
        /// </remarks>
        public RCAppPanel(RCIntRectangle backgroundRect, RCIntRectangle contentRect,
                          ShowMode showMode, HideMode hideMode,
                          int appearDuration, int disappearDuration,
                          string backgroundSprite)
            : base(backgroundRect, contentRect, showMode, hideMode, appearDuration, disappearDuration)
        {
            this.StatusChanged += this.OnPanelStatusChanged;
            this.background = backgroundSprite != null ? UIResourceManager.GetResource<UISprite>(backgroundSprite) : null;
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            if (this.background != null)
            {
                renderContext.RenderSprite(this.background, this.Range.Location);
            }
        }

        /// <summary>
        /// Called when the status of this panel has been changed.
        /// </summary>
        private void OnPanelStatusChanged(UIPanel sender, Status newStatus)
        {
            if (newStatus == Status.Visible)
            {
                this.AttachControls();
                this.AttachControlsSensitive();
            }
            else if (newStatus == Status.Disappearing)
            {
                this.DetachControlsSensitive();
                this.DetachControls();
            }
        }

        /// <summary>
        /// Reference to the background of this panel or null reference if there is no background.
        /// </summary>
        private UISprite background;
    }
}
