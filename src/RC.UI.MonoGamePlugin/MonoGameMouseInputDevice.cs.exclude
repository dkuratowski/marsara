using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using RC.Common;
using System.Windows.Forms;
using RC.Common.Diagnostics;

namespace RC.UI.MonoGamePlugin
{
    /// <summary>
    /// Implements the mouse input device.
    /// </summary>
    class MonoGameMouseInputDevice : IUIMouseAccess, IDisposable
    {
        /// <summary>
        /// Constructs an MonoGameMouseInputDevice
        /// </summary>
        public MonoGameMouseInputDevice(MonoGameGraphicsPlatform platform)
        {
            this.firstUpdateHappened = false;
            this.isFormActive = true;
            this.platform = platform;
            this.pressedButtons = new RCSet<UIMouseButton>();
            this.scrollWheelPos = 0;
        }

        /// <summary>
        /// Sends a mouse event if necessary.
        /// </summary>
        public void Update()
        {
            if (!this.firstUpdateHappened)
            {
                this.platform.Window.GotFocus += this.OnFormActivated;
                this.platform.Window.LostFocus += this.OnFormDeactivated;
                this.firstUpdateHappened = true;
            }

            if (this.isFormActive)
            {
                MouseState mouseState = Mouse.GetState();
                this.delta = new RCIntVector(mouseState.X, mouseState.Y) - this.systemMousePos;

                RCSet<UIMouseButton> pressedButtons = new RCSet<UIMouseButton>();
                if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) { pressedButtons.Add(UIMouseButton.Left); }
                if (mouseState.MiddleButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) { pressedButtons.Add(UIMouseButton.Middle); }
                if (mouseState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) { pressedButtons.Add(UIMouseButton.Right); }
                if (mouseState.XButton1 == Microsoft.Xna.Framework.Input.ButtonState.Pressed) { pressedButtons.Add(UIMouseButton.X1); }
                if (mouseState.XButton2 == Microsoft.Xna.Framework.Input.ButtonState.Pressed) { pressedButtons.Add(UIMouseButton.X2); }

                if (this.delta.X != 0 || this.delta.Y != 0 ||
                    !this.pressedButtons.SetEquals(pressedButtons) ||
                    this.scrollWheelPos != mouseState.ScrollWheelValue)
                {
                    /// Set back the system mouse and enqueue a mouse event.
                    Mouse.SetPosition(this.systemMousePos.X, this.systemMousePos.Y);
                    this.pressedButtons = pressedButtons;
                    this.scrollWheelPos = mouseState.ScrollWheelValue;

                    if (this.StateChanged != null) { this.StateChanged(); }
                }
            }
        }

        #region IUIMouseAccess methods

        /// <see cref="IUIMouseAccess.Delta"/>
        public RCIntVector Delta { get { return this.delta; } }

        /// <see cref="IUIMouseAccess.PressedButtons"/>
        public RCSet<UIMouseButton> PressedButtons { get { return this.pressedButtons; } }

        /// <see cref="IUIMouseAccess.ScrollWheelPos"/>
        public int ScrollWheelPos { get { return this.scrollWheelPos; } }

        /// <see cref="IUIMouseAccess.StateChanged"/>
        public event Action StateChanged;

        #endregion IUIMouseAccess methods

        #region IDisposable methods

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            platform.Window.GotFocus -= this.OnFormActivated;
            platform.Window.LostFocus -= this.OnFormDeactivated;
        }

        #endregion IDisposable methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialPos"></param>
        public void Reset(RCIntVector initialPos)
        {
            this.systemMousePos = initialPos;
            Mouse.SetPosition(this.systemMousePos.X, this.systemMousePos.Y);
        }

        /// <summary>
        /// Called when the application form has been activated.
        /// </summary>
        private void OnFormActivated(object sender, EventArgs evt)
        {
            this.isFormActive = true;
            Mouse.SetPosition(this.systemMousePos.X, this.systemMousePos.Y);
        }

        /// <summary>
        /// Called when the application form has been deactivated.
        /// </summary>
        private void OnFormDeactivated(object sender, EventArgs evt)
        {
            this.isFormActive = false;
        }

        /// <summary>
        /// This flag indicates whether the application form is active or not.
        /// </summary>
        private bool isFormActive;

        /// <summary>
        /// Position of the system mouse cursor in screen coordinates.
        /// </summary>
        private RCIntVector systemMousePos;

        /// <summary>
        /// The difference between the previous and the current position of the mouse input device.
        /// </summary>
        private RCIntVector delta;

        /// <summary>
        /// Reference to the platform.
        /// </summary>
        private MonoGameGraphicsPlatform platform;

        /// <summary>
        /// Set of the pressed buttons in the previous update.
        /// </summary>
        private RCSet<UIMouseButton> pressedButtons;

        /// <summary>
        /// The position of the mouse scroll wheel in the previous update.
        /// </summary>
        private int scrollWheelPos;

        /// <summary>
        /// This flag is false until the first update.
        /// </summary>
        private bool firstUpdateHappened;
    }
}
