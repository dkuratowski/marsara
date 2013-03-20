using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;
using RC.Common;
using RC.App.PresLogic.Panels;

namespace RC.App.PresLogic.Pages
{
    /// <summary>
    /// The Multiplayer Setup page of the RC application.
    /// </summary>
    public class RCMultiSetupPage : RCAppPage
    {
        /// <summary>
        /// Constructs an RCSelectGamePage instance.
        /// </summary>
        public RCMultiSetupPage()
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

            this.multiSetupPanel = new RCMultiSetupPanel(new RCIntRectangle(0, 0, 180, 139), new RCIntRectangle(10, 11, 170, 128),
                                                         UIPanel.ShowMode.DriftFromLeft, UIPanel.HideMode.DriftToLeft,
                                                         300, 300,
                                                         "RC.App.Sprites.LeftLargePanel");

            this.multiChatPanel = new RCMultiChatPanel(new RCIntRectangle(0, 131, 180, 67), new RCIntRectangle(6, 11, 174, 56),
                                                       UIPanel.ShowMode.DriftFromBottom, UIPanel.HideMode.DriftToBottom,
                                                       300, 300,
                                                       "RC.App.Sprites.MultiChatPanel");

            this.gameInfoPanel = new RCGameInfoPanel(new RCIntRectangle(193, 0, 127, 139), new RCIntRectangle(0, 20, 117, 119),
                                                     UIPanel.ShowMode.DriftFromRight, UIPanel.HideMode.DriftToRight,
                                                     300, 300,
                                                     "RC.App.Sprites.RightMediumPanel");

            this.RegisterPanel(this.okButtonPanel);
            this.RegisterPanel(this.cancelButtonPanel);
            this.RegisterPanel(this.multiSetupPanel);
            this.RegisterPanel(this.multiChatPanel);
            this.RegisterPanel(this.gameInfoPanel);

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
            this.multiSetupPanel.Show();
            this.multiChatPanel.Show();
            this.gameInfoPanel.Show();
        }

        /// <summary>
        /// Called when one of the navigation buttons has been pressed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnButtonPressed(UISensitiveObject sender)
        {
            if (sender == this.okButtonPanel.NavigationButton)
            {
                /// TODO: start countdown and begin the game
            }
            else if (sender == this.cancelButtonPanel.NavigationButton)
            {
                /// TODO: exit from the setup stage of the game
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
        /// Reference to the Multiplayer Setup panel.
        /// </summary>
        private RCMultiSetupPanel multiSetupPanel;

        /// <summary>
        /// Reference to the Multiplayer Chat panel.
        /// </summary>
        private RCMultiChatPanel multiChatPanel;

        /// <summary>
        /// Reference to the information panel of the game.
        /// </summary>
        private RCGameInfoPanel gameInfoPanel;
    }
}
