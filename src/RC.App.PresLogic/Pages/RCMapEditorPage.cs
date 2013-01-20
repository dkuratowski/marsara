using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// This page will display the map editor.
    /// </summary>
    public class RCMapEditorPage : RCAppPage
    {
        /// <summary>
        /// Constructs an RCMapEditorPage instance.
        /// </summary>
        public RCMapEditorPage()
            : base()
        {
            this.mapDisplay = new RCMapDisplay(new RCIntVector(0, 0), UIWorkspace.Instance.WorkspaceSize);
            //this.Attach(this.mapDisplay);
            //this.AttachSensitive(this.mapDisplay);

            this.vertScrollBar = new RCScrollBar(new RCIntVector(0, 0), 150, 200);
            this.horzScrollBar = new RCScrollBar(new RCIntVector(25, 10), 250, 500, true);
            this.Attach(this.vertScrollBar);
            this.AttachSensitive(this.vertScrollBar);
            this.Attach(this.horzScrollBar);
            this.AttachSensitive(this.horzScrollBar);

            this.vertScrollBar.SelectedValueChanged += this.OnSelectedValueChanged;
            this.horzScrollBar.SelectedValueChanged += this.OnSelectedValueChanged;

            this.horzValue = new UIString("Horizontal scrollbar: {0}", UIResourceManager.GetResource<UIFont>("RC.MapEditor.Fonts.Font6"), UIWorkspace.Instance.PixelScaling, UIColor.White);
            this.horzValue[0] = this.horzScrollBar.SelectedValue;
            this.vertValue = new UIString("Vertical scrollbar: {0}", UIResourceManager.GetResource<UIFont>("RC.MapEditor.Fonts.Font6"), UIWorkspace.Instance.PixelScaling, UIColor.White);
            this.vertValue[0] = this.vertScrollBar.SelectedValue;
        }

        protected override void Render_i(IUIRenderContext renderContext)
        {
            renderContext.RenderString(this.horzValue, new RCIntVector(30, 30));
            renderContext.RenderString(this.vertValue, new RCIntVector(30, 40));
        }

        private void OnSelectedValueChanged(UISensitiveObject sender)
        {
            RCScrollBar senderScrollBar = (RCScrollBar)sender;
            if (senderScrollBar == this.horzScrollBar) { this.horzValue[0] = this.horzScrollBar.SelectedValue; }
            else if (senderScrollBar == this.vertScrollBar) { this.vertValue[0] = this.vertScrollBar.SelectedValue; }
        }

        private RCScrollBar vertScrollBar;
        private RCScrollBar horzScrollBar;
        private UIString horzValue;
        private UIString vertValue;

        /// <summary>
        /// Reference to the map display.
        /// </summary>
        private RCMapDisplay mapDisplay;
    }
}
