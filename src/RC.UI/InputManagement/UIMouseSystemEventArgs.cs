using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Enumerates the buttons on a mouse.
    /// </summary>
    public enum UIMouseButton
    {
        Undefined = 0,
        Left = 1,
        Middle = 2,
        Right = 3,
        X1 = 4,
        X2 = 5
    }

    /// <summary>
    /// Contains event data of any system mouse events.
    /// </summary>
    public class UIMouseSystemEventArgs : UIEventArgs
    {
        /// <summary>
        /// Constructs a UIMouseSystemEventArgs object.
        /// </summary>
        /// <param name="delta">The difference between the previous and the current position of the system mouse.</param>
        /// <param name="pressedBtns">The list of the mouse buttons that are currently being pressed.</param>
        /// <param name="scrollWheelPos">The position of the mouse scroll wheel.</param>
        public UIMouseSystemEventArgs(RCIntVector delta, HashSet<UIMouseButton> pressedBtns, int scrollWheelPos)
        {
            if (delta == RCIntVector.Undefined) { throw new ArgumentNullException("delta"); }
            if (pressedBtns == null) { throw new ArgumentNullException("pressedBtns"); }

            this.delta = delta;
            this.pressedButtons = pressedBtns;
            this.scrollWheelPos = scrollWheelPos;
        }

        /// <summary>
        /// Gets the difference between the previous and the current position of the system mouse.
        /// </summary>
        public RCIntVector Delta { get { return this.delta; } }

        /// <summary>
        /// Gets the list of the mouse buttons that are currently being pressed.
        /// </summary>
        public HashSet<UIMouseButton> PressedButtons { get { return this.pressedButtons; } }

        /// <summary>
        /// Gets the position of the mouse scroll wheel.
        /// </summary>
        public int ScrollWheelPos { get { return this.scrollWheelPos; } }

        /// <summary>
        /// The difference between the previous and the current position of the system mouse.
        /// </summary>
        private RCIntVector delta;

        /// <summary>
        /// The list of the mouse buttons that are currently being pressed.
        /// </summary>
        private HashSet<UIMouseButton> pressedButtons;

        /// <summary>
        /// The position of the mouse scroll wheel.
        /// </summary>
        private int scrollWheelPos;
    }
}
