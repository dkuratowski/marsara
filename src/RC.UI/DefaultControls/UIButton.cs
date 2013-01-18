using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Represents a button control.
    /// </summary>
    public class UIButton : UIControl
    {
        #region Event and delegate definitions

        /// <summary>
        /// Occurs when this button has been pushed and then released by an input device.
        /// </summary>
        public event UIInputEventHdl Pressed;

        #endregion Event and delegate definitions

        /// <summary>
        /// Constructs a UIButton object at the given position with the given size.
        /// </summary>
        /// <param name="position">The position of the UIButton.</param>
        /// <param name="size">The size of the UIButton.</param>
        public UIButton(RCIntVector position, RCIntVector size)
            : base(position, size)
        {
            this.isHighlighted = false;
            this.isPushed = false;

            this.MouseSensor.Enter += this.OnMouseEnter;
            this.MouseSensor.Leave += this.OnMouseLeave;
            this.MouseSensor.ButtonDown += this.OnMouseDown;
            this.MouseSensor.ButtonUp += this.OnMouseUp;
        }

        /// <see cref="UISensitiveObject.ResetState"/>
        public override void ResetState()
        {
            base.ResetState();
            this.activatorBtn = UIMouseButton.Undefined;
            this.isHighlighted = false;
            this.isPushed = false;
        }

        #region Status flags

        /// <summary>
        /// Gets whether this UIButton is highlighted or not.
        /// </summary>
        protected bool IsHighlighted { get { return this.isHighlighted; } }

        /// <summary>
        /// Gets whether this UIButton is pushed or not.
        /// </summary>
        protected bool IsPushed { get { return this.isPushed; } }

        #endregion Status flags

        #region Mouse sensor event handlers

        /// <summary>
        /// Called when the mouse pointer has entered to the range of this button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnMouseEnter(UISensitiveObject sender)
        {
            if (this.IsEnabled)
            {
                this.isHighlighted = true;
                if (this.activatorBtn == UIMouseButton.Left) { this.isPushed = true; }
            }
        }

        /// <summary>
        /// Called when the mouse pointer has left the range of this button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnMouseLeave(UISensitiveObject sender)
        {
            if (this.IsEnabled)
            {
                this.isHighlighted = false;
                if (this.activatorBtn == UIMouseButton.Left) { this.isPushed = false; }
            }
        }

        /// <summary>
        /// Called when a mouse button has been pushed over the range of this button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="evtArgs">The details of the event.</param>
        private void OnMouseDown(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.IsEnabled && this.activatorBtn == UIMouseButton.Undefined &&
                evtArgs.Button == UIMouseButton.Left)
            {
                this.activatorBtn = evtArgs.Button;
                this.isPushed = true;
            }
        }

        /// <summary>
        /// Called when a mouse button has been released over the range of this button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="evtArgs">The details of the event.</param>
        private void OnMouseUp(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.IsEnabled && this.activatorBtn == UIMouseButton.Left &&
                evtArgs.Button == UIMouseButton.Left)
            {
                this.activatorBtn = UIMouseButton.Undefined;

                if (this.isHighlighted && this.Pressed != null)
                {
                    this.isPushed = false;
                    this.Pressed(this);
                }
            }
        }

        /// <summary>
        /// The UIMouseButton that activated the mouse sensor of this UIButton.
        /// </summary>
        private UIMouseButton activatorBtn;

        #endregion Mouse sensor event handlers

        /// <summary>
        /// This flag indicates whether this UIButton is highlighted or not.
        /// </summary>
        private bool isHighlighted;

        /// <summary>
        /// This flag indicates whether this UIButton is pushed or not.
        /// </summary>
        private bool isPushed;
    }
}
