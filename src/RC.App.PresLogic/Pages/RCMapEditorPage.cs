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
            this.Attach(this.mapDisplay);
            this.AttachSensitive(this.mapDisplay);

            this.mapEditorPanel = new RCMapEditorPanel(new RCIntRectangle(213, 0, 107, 190),
                                                       new RCIntRectangle(0, 25, 90, 165),
                                                       "RC.MapEditor.Sprites.CtrlPanel");
            this.RegisterPanel(this.mapEditorPanel);
        }

        /// <see cref="RCAppPage.OnActivated"/>
        protected override void OnActivated()
        {
            this.mapEditorPanel.Show();
        }

        /// <summary>
        /// Reference to the map display.
        /// </summary>
        private RCMapDisplay mapDisplay;

        /// <summary>
        /// Reference to the panel with the controls.
        /// </summary>
        private RCMapEditorPanel mapEditorPanel;
    }
}
