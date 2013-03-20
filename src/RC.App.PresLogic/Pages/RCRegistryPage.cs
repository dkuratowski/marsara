using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;
using RC.Common.Diagnostics;
using RC.App.PresLogic.Panels;

namespace RC.App.PresLogic.Pages
{
    /// <summary>
    /// The Registry page of the RC application.
    /// </summary>
    public class RCRegistryPage : RCAppPage
    {
        /// <summary>
        /// Constructs an RCRegistryPage instance.
        /// </summary>
        public RCRegistryPage()
            : base()
        {
            this.background = UIResourceManager.GetResource<UISprite>("RC.App.Sprites.PlanetBackground");
            this.okButtonPanel = new RCNavButtonPanel(new RCIntRectangle(220, 139, 100, 32), new RCIntRectangle(0, 17, 81, 15),
                                                      UIPanel.ShowMode.DriftFromRight, UIPanel.HideMode.DriftToRight,
                                                      300, 300,
                                                      "RC.App.Sprites.OkBtnPanel",
                                                      "Ok");

            this.cancelButtonPanel = new RCNavButtonPanel(new RCIntRectangle(229, 155, 91, 35), new RCIntRectangle(0, 17, 82, 15),
                                                          UIPanel.ShowMode.DriftFromBottom, UIPanel.HideMode.DriftToBottom,
                                                          300, 300,
                                                          "RC.App.Sprites.CancelBtnPanel",
                                                          "Cancel");

            this.registryPanel = new RCRegistryPanel(new RCIntRectangle(0, 0, 180, 159), new RCIntRectangle(23, 45, 157, 114),
                                                     UIPanel.ShowMode.DriftFromLeft, UIPanel.HideMode.DriftToLeft,
                                                     300, 300,
                                                     "RC.App.Sprites.LeftMediumPanel2");
            this.RegisterPanel(this.okButtonPanel);
            this.RegisterPanel(this.cancelButtonPanel);
            this.RegisterPanel(this.registryPanel);

            this.okButtonPanel.NavigationButton.Pressed += this.OnButtonPressed;
            this.cancelButtonPanel.NavigationButton.Pressed += this.OnButtonPressed;
            this.registryPanel.NewIdButton.Pressed += this.OnButtonPressed;
            this.registryPanel.DeleteButton.Pressed += this.OnButtonPressed;
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            renderContext.RenderSprite(this.background, new RCIntVector(0, 0));
        }

        /// <see cref="RCAppPage.OnActivated"/>
        protected override void OnActivated()
        {
            this.cancelButtonPanel.Show();
            this.okButtonPanel.Show();
            this.registryPanel.Show();
        }

        /// <summary>
        /// Called when one of the navigation buttons has been pressed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnButtonPressed(UISensitiveObject sender)
        {
            if (sender == this.okButtonPanel.NavigationButton)
            {
                this.NavigateToPage("SelectGame");
            }
            else if (sender == this.cancelButtonPanel.NavigationButton)
            {
                this.NavigateToPage("MainMenu");
            }
            else if (sender == this.registryPanel.NewIdButton)
            {
                /// TODO: trace
            }
            else if (sender == this.registryPanel.DeleteButton)
            {
                /// TODO: trace
            }
        }

        /// <summary>
        /// Reference to the background.
        /// </summary>
        private UISprite background;

        /// <summary>
        /// Reference to the OK button panel.
        /// </summary>
        private RCNavButtonPanel okButtonPanel;

        /// <summary>
        /// Reference to the Cancel button panel.
        /// </summary>
        private RCNavButtonPanel cancelButtonPanel;

        /// <summary>
        /// Reference to the Registry panel.
        /// </summary>
        private RCRegistryPanel registryPanel;
    }
}
