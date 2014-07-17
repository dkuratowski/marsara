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
    /// Interface for accessing the current state of the mouse input device.
    /// </summary>
    public interface IUIMouseAccess
    {
        /// <summary>
        /// Gets the difference between the previous and the current position of the mouse input device.
        /// </summary>
        RCIntVector Delta { get; }

        /// <summary>
        /// Gets the list of the mouse buttons that are currently being pressed.
        /// </summary>
        HashSet<UIMouseButton> PressedButtons { get; }

        /// <summary>
        /// Gets the position of the mouse scroll wheel.
        /// </summary>
        int ScrollWheelPos { get; }

        /// <summary>
        /// This event is raised when the state of the mouse input device has been changed.
        /// </summary>
        event Action StateChanged;
    }
}
