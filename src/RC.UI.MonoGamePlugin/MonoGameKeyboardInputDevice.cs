using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.UI.MonoGamePlugin
{
    /// <summary>
    /// Implements the keyboard input device.
    /// </summary>
    class MonoGameKeyboardInputDevice : IUIKeyboardAccess
    {
        /// <summary>
        /// Constructs a MonoGameKeyboardInputDevice
        /// </summary>
        public MonoGameKeyboardInputDevice()
        {
            this.pressedKeys = new RCSet<UIKey>();
        }

        /// <summary>
        /// Sends a keyboard event if necessary.
        /// </summary>
        public void Update()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            Keys[] pressedKeys = keyboardState.GetPressedKeys();
            RCSet<UIKey> pressedKeysCopy = new RCSet<UIKey>();
            for (int i = 0; i < pressedKeys.Length; i++) { pressedKeysCopy.Add((UIKey)pressedKeys[i]); }
            if (!this.pressedKeys.SetEquals(pressedKeysCopy))
            {
                this.pressedKeys = pressedKeysCopy;
                if (this.StateChanged != null) { this.StateChanged(); }
            }
        }

        #region IUIKeyboardAccess methods

        /// <see cref="IUIKeyboardAccess.PressedKeys"/>
        public RCSet<UIKey> PressedKeys { get { return new RCSet<UIKey>(this.pressedKeys); } }

        /// <see cref="IUIKeyboardAccess.StateChanged"/>
        public event Action StateChanged;

        #endregion IUIKeyboardAccess methods

        /// <summary>
        /// Set of the currently pressed keys.
        /// </summary>
        private RCSet<UIKey> pressedKeys;
    }
}
