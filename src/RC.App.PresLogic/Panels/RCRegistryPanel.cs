using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;
using RC.App.PresLogic.Controls;

namespace RC.App.PresLogic.Panels
{
    /// <summary>
    /// The Registry panel on the Registry page.
    /// </summary>
    public class RCRegistryPanel : RCAppPanel
    {
        /// <summary>
        /// Creates an RCRegistryPanel instance.
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
        public RCRegistryPanel(RCIntRectangle backgroundRect, RCIntRectangle contentRect,
                               ShowMode showMode, HideMode hideMode,
                               int appearDuration, int disappearDuration,
                               string backgroundSprite)
            : base(backgroundRect, contentRect, showMode, hideMode, appearDuration, disappearDuration, backgroundSprite)
        {
            this.registryTitle = new UIString(REGISTRY_TITLE, UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font9B"),
                                              UIWorkspace.Instance.PixelScaling, UIColor.LightBlue);
            this.newIdButton = new RCMenuButton(NEW_ID_BUTTON, new RCIntRectangle(0, 99, 85, 15));
            this.deleteButton = new RCMenuButton(DELETE_BUTTON, new RCIntRectangle(88, 99, 66, 15));
            this.AddControl(this.newIdButton);
            this.AddControl(this.deleteButton);
        }

        /// <summary>
        /// Gets a reference to the "New ID" button.
        /// </summary>
        public RCMenuButton NewIdButton { get { return this.newIdButton; } }

        /// <summary>
        /// Gets a reference to the "Delete" button.
        /// </summary>
        public RCMenuButton DeleteButton { get { return this.deleteButton; } }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            base.Render_i(renderContext);
            renderContext.RenderString(this.registryTitle, new RCIntVector(1, -8));
        }

        /// <summary>
        /// The UIString displayed as the title of this panel.
        /// </summary>
        private UIString registryTitle;

        /// <summary>
        /// Reference to the "New ID" button.
        /// </summary>
        private RCMenuButton newIdButton;

        /// <summary>
        /// Reference to the "Delete" button.
        /// </summary>
        private RCMenuButton deleteButton;

        /// <summary>
        /// Constants of this panel.
        /// </summary>
        private const string REGISTRY_TITLE = "Registry";
        private const string NEW_ID_BUTTON = "New ID";
        private const string DELETE_BUTTON = "Delete";
    }
}
