using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Base class of every controls.
    /// </summary>
    public class UIControl : UISensitiveObject
    {
        /// <summary>
        /// Constructs a UIControl object at the given position and size.
        /// </summary>
        /// <param name="position">The position of the control.</param>
        /// <param name="size">The size of the control.</param>
        public UIControl(RCIntVector position, RCIntVector size)
            : base(position, new RCIntRectangle(0, 0, size.X, size.Y))
        {
            if (size.X <= 0 || size.Y <= 0) { throw new ArgumentOutOfRangeException("size"); }
            this.isEnabled = true;
        }

        /// <summary>
        /// Gets or sets whether this UIControl is enabled or not.
        /// </summary>
        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set { this.isEnabled = value; }
        }

        /// <summary>
        /// Gets or sets whether this UIControl is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get { return this.isSelected; }
            set { this.isSelected = value; }
        }

        /// <summary>
        /// This flag indicates whether this UIControl is in enabled state or not.
        /// </summary>
        private bool isEnabled;

        /// <summary>
        /// This flag indicates whether this UIControl is selected or not.
        /// </summary>
        private bool isSelected;
    }
}
