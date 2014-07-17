using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Provides data for the MouseUp, MouseDown, MouseClick, MouseDblClick, MouseWheel and MouseMove events.
    /// </summary>
    public class UIMouseEventArgs
    {
        /// <summary>
        /// Use this constructor for MouseMove events.
        /// </summary>
        public UIMouseEventArgs(RCIntVector position)
        {
            this.position = position;
            this.button = UIMouseButton.Undefined;
            this.wheelDelta = -1;
        }

        /// <summary>
        /// Use this constructor for MouseWheel events.
        /// </summary>
        public UIMouseEventArgs(RCIntVector position, int wheelDelta)
        {
            this.position = position;
            this.button = UIMouseButton.Undefined;
            this.wheelDelta = wheelDelta;
        }

        /// <summary>
        /// Use this constructor for MouseUp, MouseDown, MouseClick and MouseDblClick events.
        /// </summary>
        public UIMouseEventArgs(RCIntVector position, UIMouseButton button)
        {
            this.position = position;
            this.button = button;
            this.wheelDelta = -1;
        }

        /// <summary>
        /// Gets the position of the mouse pointer when the event is raised.
        /// </summary>
        public RCIntVector Position { get { return this.position; } }

        /// <summary>
        /// Gets the affected mouse button. This field is UIMouseButton.Undefined for MouseWheel and MouseMove events.
        /// </summary>
        public UIMouseButton Button { get { return this.button; } }

        /// <summary>
        /// Gets the position of the mouse wheel relative to it's previous position. This field is -1 for MouseUp, MouseDown,
        /// MouseClick, MouseDblClick and MouseMove events.
        /// </summary>
        public int WheelDelta { get { return this.wheelDelta; } }

        /// <summary>
        /// The position of the mouse pointer when the event is raised.
        /// </summary>
        private RCIntVector position;

        /// <summary>
        /// The affected mouse button. This field is UIMouseButton.Undefined for MouseWheel and MouseMove events.
        /// </summary>
        private UIMouseButton button;

        /// <summary>
        /// The position of the mouse wheel relative to it's previous position.
        /// </summary>
        private int wheelDelta;
    }
}
