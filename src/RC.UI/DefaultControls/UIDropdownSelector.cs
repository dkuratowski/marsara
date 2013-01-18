using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// The UIDropdownSelector is a dropdown list from where the user can select between
    /// several options.
    /// </summary>
    public class UIDropdownSelector : UIControl
    {
        #region Event and delegate definitions

        /// <summary>
        /// Occurs when the index of the selected option has been changed.
        /// </summary>
        public event UIInputEventHdl SelectedIndexChanged;

        #endregion Event and delegate definitions

        /// <summary>
        /// Constructs a UIDropdownSelector object.
        /// </summary>
        /// <param name="position">The position of the UIDropdownSelector.</param>
        /// <param name="size">
        /// The size of the UIDropdownSelector. In dropped-down state the height of the control will be
        /// expanded such that every selectable options are visible.
        /// </param>
        /// <param name="optionCount">The number of the selectable options of the UIDropdownSelector.</param>
        public UIDropdownSelector(RCIntVector position, RCIntVector size, int optionCount)
            : base(position, size)
        {
            if (optionCount <= 0) { throw new ArgumentOutOfRangeException("optionCount"); }

            this.optionCount = optionCount;
            this.selectedIndex = 0;
            this.highlightedIndex = 0;
            this.currentStatus = Status.Normal;
            this.normalRect = new RCIntRectangle(0, 0, size.X, size.Y);
            this.optionListRect = new RCIntRectangle(0, size.Y, size.X, size.Y * optionCount);
            this.droppedDownRect = new RCIntRectangle(0, 0, size.X, size.Y * (optionCount + 1));

            this.MouseSensor.Enter += this.OnMouseEnter;
            this.MouseSensor.Move += this.OnMouseMove;
            this.MouseSensor.Leave += this.OnMouseLeave;
            this.MouseSensor.ButtonDown += this.OnMouseDown;
            this.MouseSensor.ButtonUp += this.OnMouseUp;
        }

        /// <summary>
        /// Gets or sets the index of the selected option.
        /// </summary>
        public int SelectedIndex
        {
            get { return this.selectedIndex; }
            set
            {
                if (value < 0 || value >= this.optionCount) { throw new ArgumentOutOfRangeException("SelectedIndex"); }

                int prevIdx = this.selectedIndex;
                this.selectedIndex = value;
                if (this.SelectedIndexChanged != null && this.selectedIndex != prevIdx)
                {
                    this.SelectedIndexChanged(this);
                }
            }
        }

        /// <see cref="UISensitiveObject.ResetState"/>
        public override void ResetState()
        {
            base.ResetState();
            this.activatorBtn = UIMouseButton.Undefined;
            this.highlightedIndex = this.selectedIndex;
            this.currentStatus = Status.Normal;
        }

        #region Status data

        /// <summary>
        /// Enumerates the possible states of a UIDropdownSelector control.
        /// </summary>
        protected enum Status
        {
            Normal = 0,
            Highlighted = 1,
            DroppedDown = 2,
            Selecting = 3
        }

        /// <summary>
        /// Gets the current status of this UIDropdownSelector.
        /// </summary>
        protected Status CurrentStatus { get { return this.currentStatus; } }

        /// <summary>
        /// Gets the index of the currently highlighted option in case of Status.DroppedDown or Status.Selecting.
        /// </summary>
        protected int HighlightedIndex { get { return this.highlightedIndex; } }

        /// <summary>
        /// Gets the area where the selected option is displayed when the control is not dropped down.
        /// </summary>
        protected RCIntRectangle NormalRect { get { return this.normalRect; } }

        /// <summary>
        /// Gets the area where the option list is displayed when the control is dropped down.
        /// </summary>
        protected RCIntRectangle OptionListRect { get { return this.optionListRect; } }

        /// <summary>
        /// Gets the area of the whole control in dropped down state.
        /// </summary>
        protected RCIntRectangle DroppedDownRect { get { return this.droppedDownRect; } }

        #endregion Status data

        #region Mouse sensor event handlers

        /// <summary>
        /// Called when the mouse pointer has entered to the range of this dropdown selector..
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnMouseEnter(UISensitiveObject sender)
        {
            if (this.IsEnabled)
            {
                if (this.currentStatus == Status.Normal)
                {
                    this.currentStatus = Status.Highlighted;
                }
            }
        }

        /// <summary>
        /// Called when the mouse pointer has left the range of this dropdown selector.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        private void OnMouseLeave(UISensitiveObject sender)
        {
            if (this.IsEnabled)
            {
                if (this.currentStatus == Status.Highlighted)
                {
                    this.currentStatus = Status.Normal;
                }
            }
        }

        /// <summary>
        /// Called when the mouse pointer moved over the range of this dropdown selector.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="evtArgs">The details of the event.</param>
        private void OnMouseMove(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.IsEnabled)
            {
                if (this.currentStatus == Status.DroppedDown || this.currentStatus == Status.Selecting)
                {
                    if (this.currentStatus == Status.DroppedDown ? this.optionListRect.Contains(evtArgs.Position) : true)
                    {
                        /// The pointer is now over the option list part of the dropped down control
                        RCIntVector relativePos = evtArgs.Position - this.optionListRect.Location;
                        this.highlightedIndex = relativePos.Y >= 0 && relativePos.Y < this.optionListRect.Height
                                              ? relativePos.Y / this.normalRect.Height
                                              : -1;
                        if (highlightedIndex == -1)
                        {
                            if (relativePos.Y < 0) { this.highlightedIndex = 0; }
                            else if (relativePos.Y >= this.optionListRect.Height) { this.highlightedIndex = this.optionCount - 1; }
                        }

                        this.currentStatus = Status.Selecting;
                    }
                }
            }
        }

        /// <summary>
        /// Called when a mouse button has been pushed over the range of this dropdown selector.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="evtArgs">The details of the event.</param>
        private void OnMouseDown(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.IsEnabled && this.activatorBtn == UIMouseButton.Undefined &&
                evtArgs.Button == UIMouseButton.Left)
            {
                this.activatorBtn = evtArgs.Button;
                if (this.currentStatus == Status.Highlighted)
                {
                    this.Range = this.droppedDownRect;
                    this.BringToTop();
                    this.currentStatus = Status.DroppedDown;
                }
            }
        }

        /// <summary>
        /// Called when a mouse button has been released over the range of this dropdown selector.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="evtArgs">The details of the event.</param>
        private void OnMouseUp(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.IsEnabled && this.activatorBtn == UIMouseButton.Left &&
                evtArgs.Button == UIMouseButton.Left)
            {
                this.activatorBtn = UIMouseButton.Undefined;
                if (this.currentStatus == Status.DroppedDown || this.currentStatus == Status.Selecting)
                {
                    this.Range = this.normalRect;
                    bool selectIdx = this.currentStatus == Status.Selecting;
                    this.currentStatus = this.normalRect.Contains(evtArgs.Position) ? Status.Highlighted
                                                                                    : Status.Normal;
                    if (selectIdx) { this.SelectedIndex = this.highlightedIndex; }
                }
            }
        }

        /// <summary>
        /// The UIMouseButton that activated the mouse sensor of this UIButton.
        /// </summary>
        private UIMouseButton activatorBtn;

        #endregion Mouse sensor event handlers

        /// <summary>
        /// The index of the currently selected option.
        /// </summary>
        private int selectedIndex;

        /// <summary>
        /// The index of the currently highlighted option in case of Status.DroppedDown or Status.Selecting.
        /// </summary>
        private int highlightedIndex;

        /// <summary>
        /// The current status of this UIDropdownSelector.
        /// </summary>
        private Status currentStatus;

        /// <summary>
        /// Number of the selectable options of this UIDropdownSelector.
        /// </summary>
        private int optionCount;

        /// <summary>
        /// The area where the selected option is displayed when the control is not dropped down.
        /// </summary>
        private RCIntRectangle normalRect;

        /// <summary>
        /// The area where the option list is displayed when the control is dropped down.
        /// </summary>
        private RCIntRectangle optionListRect;

        /// <summary>
        /// The area of the whole control in dropped down state.
        /// </summary>
        private RCIntRectangle droppedDownRect;
    }
}
