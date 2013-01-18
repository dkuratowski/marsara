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

            this.vertScrollBar = new RCScrollBar(new RCIntVector(10, 10), 150, 25);
            this.horzScrollBar = new RCScrollBar(new RCIntVector(25, 10), 250, 25, true);
            this.Attach(this.vertScrollBar);
            this.AttachSensitive(this.vertScrollBar);
            this.Attach(this.horzScrollBar);
            this.AttachSensitive(this.horzScrollBar);
        }

        private RCScrollBar vertScrollBar;
        private RCScrollBar horzScrollBar;

        /// <summary>
        /// Reference to the map display.
        /// </summary>
        private RCMapDisplay mapDisplay;
    }
}
