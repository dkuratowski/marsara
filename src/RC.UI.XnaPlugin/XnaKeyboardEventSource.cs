using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using RC.Common.Diagnostics;

namespace RC.UI.XnaPlugin
{
    /// <summary>
    /// Source of keyboard events.
    /// </summary>
    class XnaKeyboardEventSource : UIEventSourceBase
    {
        /// <summary>
        /// Constructs an XnaKeyboardEventSource
        /// </summary>
        public XnaKeyboardEventSource()
            : base("RC.UI.XnaPlugin.XnaKeyboardEventSource")
        {
            this.prevPressedKeys = new HashSet<UIKey>();
        }

        /// <summary>
        /// Sends a keyboard event if necessary.
        /// </summary>
        public void Update()
        {
            if (this.IsActive)
            {
                KeyboardState keyboardState = Keyboard.GetState();
                Keys[] pressedKeys = keyboardState.GetPressedKeys();
                HashSet<UIKey> pressedKeysCopy = new HashSet<UIKey>();
                for (int i = 0; i < pressedKeys.Length; i++)
                {
                    pressedKeysCopy.Add((UIKey)pressedKeys[i]);
                }
                if (!this.prevPressedKeys.SetEquals(pressedKeysCopy))
                {
                    UIKeyboardSystemEventArgs evtArgs = new UIKeyboardSystemEventArgs(pressedKeysCopy);
                    UIRoot.Instance.SystemEventQueue.EnqueueEvent<UIKeyboardSystemEventArgs>(evtArgs);
                    this.prevPressedKeys = pressedKeysCopy;
                }
            }
        }

        /// <see cref="UIEventSourceBase.Activate_i"/>
        protected override void Activate_i()
        {
        }

        /// <see cref="UIEventSourceBase.Deactivate_i"/>
        protected override void Deactivate_i()
        {
        }

        /// <summary>
        /// Set of the pressed keys in the previous update.
        /// </summary>
        private HashSet<UIKey> prevPressedKeys;
    }
}
