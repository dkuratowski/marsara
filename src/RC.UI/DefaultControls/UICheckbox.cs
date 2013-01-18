using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Represents a checkbox control that can be used to accept a two-state answer from the user.
    /// </summary>
    public class UICheckbox : UIButton
    {
        #region Event and delegate definitions

        /// <summary>
        /// Occurs when the IsChecked property of this UICheckbox has been changed.
        /// </summary>
        public event UIInputEventHdl CheckedStateChanged;

        #endregion Event and delegate definitions

        /// <summary>
        /// Constructs a UICheckbox object at the given position with the given size and initial state.
        /// </summary>
        /// <param name="position">The position of the UICheckbox.</param>
        /// <param name="size">The size of the UICheckbox.</param>
        /// <param name="isChecked">The initial state of the UICheckbox.</param>
        public UICheckbox(RCIntVector position, RCIntVector size, bool isChecked)
            : base(position, size)
        {
            this.isChecked = isChecked;
            this.Pressed += this.OnPressed;
        }

        /// <summary>
        /// Gets or sets the checked state of this checkbox.
        /// </summary>
        public bool IsChecked
        {
            get { return this.isChecked; }
            set
            {
                bool prevState = this.isChecked;
                this.isChecked = value;
                if (this.CheckedStateChanged != null && this.isChecked != prevState)
                {
                    this.CheckedStateChanged(this);
                }
            }
        }

        /// <summary>
        /// Called when the UIButton.Pressed event of this UICheckbox has been raised.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnPressed(UISensitiveObject sender)
        {
            this.IsChecked = !this.IsChecked;
        }

        /// <summary>
        /// This flag indicates whether this checkbox is checked or not.
        /// </summary>
        private bool isChecked;
    }
}
