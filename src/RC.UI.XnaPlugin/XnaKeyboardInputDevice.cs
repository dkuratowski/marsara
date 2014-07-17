using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using RC.Common.Diagnostics;

namespace RC.UI.XnaPlugin
{
    /// <summary>
    /// Implements the keyboard input device.
    /// </summary>
    class XnaKeyboardInputDevice : IUIKeyboardAccess
    {
        /// <summary>
        /// Constructs an XnaKeyboardEventSource
        /// </summary>
        public XnaKeyboardInputDevice()
        {
            this.pressedKeys = new HashSet<UIKey>();
        }

        /// <summary>
        /// Sends a keyboard event if necessary.
        /// </summary>
        public void Update()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            Keys[] pressedKeys = keyboardState.GetPressedKeys();
            HashSet<UIKey> pressedKeysCopy = new HashSet<UIKey>();
            for (int i = 0; i < pressedKeys.Length; i++) { pressedKeysCopy.Add((UIKey)pressedKeys[i]); }
            if (!this.pressedKeys.SetEquals(pressedKeysCopy))
            {
                this.pressedKeys = pressedKeysCopy;
                if (this.StateChanged != null) { this.StateChanged(); }
            }
        }

        #region IUIKeyboardAccess methods

        /// <see cref="IUIKeyboardAccess.PressedKeys"/>
        public HashSet<UIKey> PressedKeys { get { return new HashSet<UIKey>(this.pressedKeys); } }

        /// <see cref="IUIKeyboardAccess.StateChanged"/>
        public event Action StateChanged;

        #endregion IUIKeyboardAccess methods

        /// <summary>
        /// Set of the currently pressed keys.
        /// </summary>
        private HashSet<UIKey> pressedKeys;
    }
}
