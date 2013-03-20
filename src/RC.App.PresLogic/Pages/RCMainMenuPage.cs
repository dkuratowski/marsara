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
    /// The Main Menu page of the RC application.
    /// </summary>
    public class RCMainMenuPage : RCAppPage
    {
        /// <summary>
        /// Represents a method that is called when loading the resources has been finished.
        /// </summary>
        public delegate void LoadFinishedHdl();

        /// <summary>
        /// Raised when loading the resources has been finished.
        /// </summary>
        public event LoadFinishedHdl LoadFinished;

        /// <summary>
        /// Constructs an RCMainMenuPage.
        /// </summary>
        public RCMainMenuPage()
        {
            this.currentPhase = ShowPhase.OnlyBackground;
            this.firstActivationTime = 0;
            this.hasBeenActivatedOnce = false;
            this.loadingTask = null;
            this.background = UIResourceManager.GetResource<UISprite>("RC.App.Sprites.MainMenuBackground");
            this.titleAnimation = UIResourceManager.GetResource<UIAnimation>("RC.App.Animations.MainMenuTitleAnim");
            this.headerFooterFont = UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font6");
            this.loadingFont = UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font7");
            this.headerString = new UIString("DAVID MOLDVAI ENTERTAINMENT PRESENTS:", this.headerFooterFont, UIWorkspace.Instance.PixelScaling, new UIColor(220, 220, 220));
            this.footerString = new UIString("COPYRIGHT 1984", this.headerFooterFont, UIWorkspace.Instance.PixelScaling, new UIColor(220, 220, 220));
            this.loadingString = new UIString("Loading...", this.loadingFont, UIWorkspace.Instance.PixelScaling, new UIColor(220, 220, 220));
        }

        /// <see cref="UIObject.Render_i"/>
        protected override void Render_i(IUIRenderContext renderContext)
        {
            renderContext.RenderSprite(this.background, new RCIntVector(0, 0));

            if (this.currentPhase != ShowPhase.OnlyBackground)
            {
                UISprite titleSprite = this.titleAnimation.CurrentSprite;
                renderContext.RenderSprite(titleSprite, new RCIntVector((this.Range.Width - titleSprite.Size.X) / 2, 30));
            }

            if (this.currentPhase == ShowPhase.Loading)
            {
                renderContext.RenderString(this.headerString, new RCIntVector((this.Range.Width - this.headerString.Width) / 2, 20));
                renderContext.RenderString(this.footerString, new RCIntVector((this.Range.Width - this.footerString.Width) / 2, this.Range.Height - 10));
                renderContext.RenderString(this.loadingString, new RCIntVector((this.Range.Width - this.loadingString.Width) / 2, 130));
            }
            else if (this.currentPhase == ShowPhase.Normal)
            {
                renderContext.RenderString(this.headerString, new RCIntVector((this.Range.Width - this.headerString.Width) / 2, 20));
                renderContext.RenderString(this.footerString, new RCIntVector((this.Range.Width - this.footerString.Width) / 2, this.Range.Height - 10));
            }
        }

        /// <summary>
        /// Enumerates the possible presentation phases of this page.
        /// </summary>
        private enum ShowPhase
        {
            OnlyBackground = 0,
            AnimatingTitle = 1,
            Loading = 2,
            Normal = 3
        }

        /// <summary>
        /// Called when a menu button has been pressed.
        /// </summary>
        private void OnMenupointPressed(UISensitiveObject sender)
        {
            if (sender == this.menuPanel[START_GAME_MENUPOINT])
            {
                /// Navigate to the Registry page.
                this.NavigateToPage("Registry");
            }
            else if (sender == this.menuPanel[CREDITS_MENUPOINT])
            {
                /// Navigate to the Credits page.
                this.NavigateToPage("Credits");
            }
            else if (sender == this.menuPanel[EXIT_MENUPOINT])
            {
                /// Stop the render loop
                UIRoot.Instance.GraphicsPlatform.RenderLoop.Stop();
            }
        }

        /// <see cref="RCAppPage.OnActivated"/>
        protected override void OnActivated()
        {
            if (!this.hasBeenActivatedOnce)
            {
                this.firstActivationTime = UIRoot.Instance.GraphicsPlatform.RenderLoop.TimeSinceStart;
                UIRoot.Instance.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(this.UpdateHdl);
                this.hasBeenActivatedOnce = true;
            }
            else
            {
                this.menuPanel.Show();
            }
        }

        /// <summary>
        /// Called by the framework on updates.
        /// </summary>
        /// <param name="evtArgs">The details of the event.</param>
        private void UpdateHdl(UIUpdateSystemEventArgs evtArgs)
        {
            if (this.currentPhase == ShowPhase.OnlyBackground)
            {
                if (UIRoot.Instance.GraphicsPlatform.RenderLoop.TimeSinceStart - this.firstActivationTime >
                    START_TITLE_ANIMATION)
                {
                    this.currentPhase = ShowPhase.AnimatingTitle;
                    this.titleAnimation.Reset(false);
                    this.titleAnimation.Start();
                }
            }
            else if (this.currentPhase == ShowPhase.AnimatingTitle)
            {
                if (this.titleAnimation.CurrentTimepoint > this.titleAnimation.Duration)
                {
                    this.currentPhase = ShowPhase.Loading;
                    this.titleAnimation.Stop();
                    UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.UpdateHdl);
                    this.loadingTask = UIResourceManager.LoadResourceGroupAsync("RC.App.CommonResources");
                    this.loadingTask.Finished += this.LoadingFinished;
                    this.loadingTask.Failed += this.LoadingFailed;
                }
            }
        }

        /// <summary>
        /// Called if loading the resources has been finished.
        /// </summary>
        private void LoadingFinished(IUIBackgroundTask sender, object args)
        {
            this.loadingTask.Finished -= this.LoadingFinished;
            this.loadingTask.Failed -= this.LoadingFailed;

            if (this.LoadFinished != null) { this.LoadFinished(); }

            this.loadingTask = null;
            this.currentPhase = ShowPhase.Normal;

            UIFont menuFont = UIResourceManager.GetResource<UIFont>("RC.App.Fonts.Font9B");
            string[] menuPoints = new string[3] { START_GAME_MENUPOINT, CREDITS_MENUPOINT, EXIT_MENUPOINT };
            this.menuPanel = new RCMainMenuPanel(new RCIntRectangle((UIWorkspace.Instance.WorkspaceSize.X - MENU_PANEL_WIDTH) / 2,
                                                                 100,
                                                                 MENU_PANEL_WIDTH,
                                                                 (menuFont.CharTopMaximum + menuFont.CharBottomMaximum + 1) * menuPoints.Length),
                                                 menuPoints);
            this.RegisterPanel(this.menuPanel);

            this.menuPanel[START_GAME_MENUPOINT].Pressed += this.OnMenupointPressed;
            this.menuPanel[CREDITS_MENUPOINT].Pressed += this.OnMenupointPressed;
            this.menuPanel[EXIT_MENUPOINT].Pressed += this.OnMenupointPressed;

            this.menuPanel.Show();
            /// TODO: this is only a temporary solution
            UISprite mouseIcon = UIResourceManager.GetResource<UISprite>("RC.App.Sprites.MenuPointerSprite");
            UIBasicPointer basicPtr = new UIBasicPointer(mouseIcon, new RCIntVector(0, 0));
            UIWorkspace.Instance.SetMousePointer(basicPtr);
        }

        /// <summary>
        /// Called if loading the resources has been failed.
        /// </summary>
        private void LoadingFailed(IUIBackgroundTask sender, object args)
        {
            this.loadingTask.Finished -= this.LoadingFinished;
            this.loadingTask.Failed -= this.LoadingFailed;

            throw (Exception)args;
        }

        /// <summary>
        /// Reference to the menu panel.
        /// </summary>
        private RCMainMenuPanel menuPanel;

        /// <summary>
        /// Reference to the background.
        /// </summary>
        private UISprite background;

        /// <summary>
        /// Reference to the title animation.
        /// </summary>
        private UIAnimation titleAnimation;

        /// <summary>
        /// The string at the header of this page.
        /// </summary>
        private UIString headerString;

        /// <summary>
        /// The string at the footer of this page.
        /// </summary>
        private UIString footerString;

        /// <summary>
        /// The font of the loading string.
        /// </summary>
        private UIFont loadingFont;

        /// <summary>
        /// The string displayed during loading.
        /// </summary>
        private UIString loadingString;

        /// <summary>
        /// The font of the header and the footer.
        /// </summary>
        private UIFont headerFooterFont;

        /// <summary>
        /// The current presentation phase of this page.
        /// </summary>
        private ShowPhase currentPhase;

        /// <summary>
        /// The system time on the first activation of this page.
        /// </summary>
        private int firstActivationTime;

        /// <summary>
        /// This flag indicates whether this page has been activated at least once.
        /// </summary>
        private bool hasBeenActivatedOnce;

        /// <summary>
        /// Reference to the loading task.
        /// </summary>
        private IUIBackgroundTask loadingTask;

        /// <summary>
        /// Time in milliseconds when to start the title animation.
        /// </summary>
        private const int START_TITLE_ANIMATION = 2000;

        /// <summary>
        /// The width of the menu panel.
        /// </summary>
        private const int MENU_PANEL_WIDTH = 100;

        /// <summary>
        /// The texts of the menupoints.
        /// </summary>
        private const string START_GAME_MENUPOINT = "START GAME";
        private const string CREDITS_MENUPOINT = "CREDITS";
        private const string EXIT_MENUPOINT = "EXIT";
    }
}
