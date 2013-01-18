using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;

namespace RC.App.PresLogic
{
    /// <summary>
    /// The Create Game page of the RC application.
    /// </summary>
    public class RCCreateGamePage : RCAppPage
    {
        /// <summary>
        /// Constructs an RCCreateGamePage instance.
        /// </summary>
        public RCCreateGamePage()
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

            this.selectMapPanel = new RCSelectMapPanel(new RCIntRectangle(0, 0, 180, 152), new RCIntRectangle(23, 45, 157, 107),
                                                       UIPanel.ShowMode.DriftFromLeft, UIPanel.HideMode.DriftToLeft,
                                                       300, 300,
                                                       "RC.App.Sprites.LeftMediumPanel0");

            this.mapInfoPanel = new RCMapInfoPanel(new RCIntRectangle(193, 0, 127, 139), new RCIntRectangle(0, 20, 117, 119),
                                                   UIPanel.ShowMode.DriftFromRight, UIPanel.HideMode.DriftToRight,
                                                   300, 300,
                                                   "RC.App.Sprites.RightMediumPanel");

            this.RegisterPanel(this.okButtonPanel);
            this.RegisterPanel(this.cancelButtonPanel);
            this.RegisterPanel(this.selectMapPanel);
            this.RegisterPanel(this.mapInfoPanel);

            this.okButtonPanel.NavigationButton.Pressed += this.OnButtonPressed;
            this.cancelButtonPanel.NavigationButton.Pressed += this.OnButtonPressed;
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
            this.selectMapPanel.Show();
            this.mapInfoPanel.Show();
        }

        /// <summary>
        /// Called when one of the navigation buttons has been pressed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnButtonPressed(UISensitiveObject sender)
        {
            if (sender == this.okButtonPanel.NavigationButton)
            {
                this.NavigateToPage("MultiSetup");
            }
            else if (sender == this.cancelButtonPanel.NavigationButton)
            {
                this.NavigateToPage("SelectGame");
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
        /// Reference to the Select Game panel.
        /// </summary>
        private RCSelectMapPanel selectMapPanel;

        /// <summary>
        /// Reference to the information panel of the selected game.
        /// </summary>
        private RCMapInfoPanel mapInfoPanel;
    }
}
