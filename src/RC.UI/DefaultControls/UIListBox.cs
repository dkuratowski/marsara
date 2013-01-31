using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// A UIListBox is a list from where the user can select between several options.
    /// </summary>
    public class UIListBox : UIControl
    {
        #region Event and delegate definitions

        /// <summary>
        /// Occurs when the index of the selected option has been changed.
        /// </summary>
        public event UIInputEventHdl SelectedIndexChanged;

        #endregion Event and delegate definitions

        /// <summary>
        /// Constructs a UIListBox object.
        /// </summary>
        /// <param name="position">The position of the UIListBox.</param>
        /// <param name="itemSize">The size of an item in the UIListBox.</param>
        /// <param name="visibleItemCount">
        /// The maximum number of items that can be visible in the listbox at a given time. If the number of
        /// items in the listbox is greater a vertical scrollbar is displayed along the right side of the control.
        /// </param>
        /// <param name="timeBetweenScrolls">
        /// The minimum time should be elapsed between scrolling the listbox in milliseconds.
        /// </param>
        public UIListBox(RCIntVector position, RCIntVector itemSize, int visibleItemCount, int timeBetweenScrolls)
            : base(position, new RCIntVector(itemSize.X, itemSize.Y * visibleItemCount))
        {
            if (visibleItemCount <= 0) { throw new ArgumentOutOfRangeException("visibleItemCount"); }
            if (timeBetweenScrolls <= 0) { throw new ArgumentOutOfRangeException("timeBetweenScrolls"); }
            if (itemSize.X <= 0 || itemSize.Y <= 0) { throw new ArgumentOutOfRangeException("itemSize"); }
            
            this.visibleItemCount = visibleItemCount;
            this.totalItemCount = 0;
            this.itemSize = itemSize;
            this.selectedIndex = -1;
            this.highlightedIndex = -1;
            this.firstVisibleIndex = -1;
            this.timeSinceLastScroll = 0;
            this.timeBetweenScrolls = timeBetweenScrolls;
            this.currentStatus = Status.Normal;
            this.scrollbar = null;
            this.internalScrollBarValueChange = false;

            this.MouseSensor.Move += this.OnMouseMove;
            this.MouseSensor.ButtonDown += this.OnMouseDown;
            this.MouseSensor.ButtonUp += this.OnMouseUp;
        }

        #region Public methods and properties

        /// <summary>
        /// Gets or sets the number of items in the list.
        /// </summary>
        public int ItemCount
        {
            get { return this.totalItemCount; }
            set
            {
                if (this.currentStatus != Status.Normal) { throw new InvalidOperationException("Invalid UIListBox state!"); }
                if (value < 0) { throw new ArgumentOutOfRangeException("ItemCount"); }

                if (this.totalItemCount != value)
                {
                    this.totalItemCount = value;
                    this.firstVisibleIndex = this.totalItemCount > 0 ? 0 : -1;
                    this.selectedIndex = this.totalItemCount > 0 ? 0 : -1;
                    this.highlightedIndex = this.totalItemCount > 0 ? 0 : -1;

                    if (this.totalItemCount > this.visibleItemCount)
                    {
                        /// Create/replace the scrollbar
                        if (this.scrollbar != null)
                        {
                            this.scrollbar.SelectedValueChanged -= this.OnScrollBarValueChanged;
                            this.DetachSensitive(this.scrollbar);
                            this.Detach(this.scrollbar);
                            this.scrollbar = null;
                        }

                        this.scrollbar = this.CreateScrollbar(this.totalItemCount - this.visibleItemCount + 1, 0);
                        if (this.scrollbar != null)
                        {
                            this.Attach(this.scrollbar);
                            this.AttachSensitive(this.scrollbar);
                            this.scrollbar.SelectedValueChanged += this.OnScrollBarValueChanged;
                        }
                    }
                    else
                    {
                        /// Remove the scrollbar
                        if (this.scrollbar != null)
                        {
                            this.scrollbar.SelectedValueChanged -= this.OnScrollBarValueChanged;
                            this.DetachSensitive(this.scrollbar);
                            this.Detach(this.scrollbar);
                            this.scrollbar = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the maximum number of items that can be visible in the listbox at a given time.
        /// </summary>
        public int VisibleItemCount { get { return this.visibleItemCount; } }

        /// <summary>
        /// Gets or sets the index of the first visible item.
        /// </summary>
        public int FirstVisibleIndex
        {
            get { return this.firstVisibleIndex; }
            set
            {
                if (this.currentStatus != Status.Normal) { throw new InvalidOperationException("Invalid UIListBox state!"); }
                if (value < 0 || value >= Math.Max(this.totalItemCount - this.visibleItemCount, 0)) { throw new ArgumentOutOfRangeException("FirstVisibleIndex"); }

                if (this.firstVisibleIndex != value && this.scrollbar != null)
                {
                    this.firstVisibleIndex = value;
                    this.internalScrollBarValueChange = true;
                    this.scrollbar.SelectedValue = this.firstVisibleIndex;
                    this.internalScrollBarValueChange = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the index of the selected option or -1 if none of the items is selected.
        /// </summary>
        public int SelectedIndex
        {
            get { return this.selectedIndex; }
            set
            {
                if (this.currentStatus != Status.Normal) { throw new InvalidOperationException("Invalid UIListBox state!"); }
                if (value < -1 || value >= this.totalItemCount) { throw new ArgumentOutOfRangeException("SelectedIndex"); }

                int prevIdx = this.selectedIndex;
                this.selectedIndex = value;
                this.highlightedIndex = value;
                if (this.SelectedIndexChanged != null && this.selectedIndex != prevIdx)
                {
                    this.SelectedIndexChanged(this);
                }
            }
        }

        #endregion Public methods and properties

        /// <see cref="UISensitiveObject.ResetState"/>
        public override void ResetState()
        {
            base.ResetState();
            this.activatorBtn = UIMouseButton.Undefined;
            this.highlightedIndex = this.selectedIndex;
            this.timeSinceLastScroll = 0;
            this.internalScrollBarValueChange = false;
            UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
            this.currentStatus = Status.Normal;
        }

        /// <summary>
        /// The listbox is using this method for creating a vertical scrollbar if necessary.
        /// </summary>
        /// <param name="intervalLength">The interval length of the scrollbar.</param>
        /// <param name="selectedValue">The selected value of the scrollbar.</param>
        /// <returns>The created scrollbar or null if no scrollbar shall be displayed.</returns>
        protected virtual UIScrollBar CreateScrollbar(int intervalLength, int selectedValue)
        {
            /// Don't create scrollbar by default.
            return null;
        }

        #region Status data

        /// <summary>
        /// Gets the index of the currently highlighted option.
        /// </summary>
        protected int HighlightedIndex { get { return this.highlightedIndex; } }

        /// <summary>
        /// Enumerates the possible states of a UIListBox control.
        /// </summary>
        private enum Status
        {
            Normal = 0,
            Selecting = 1,
            ScrollingDown = 2,
            ScrollingUp = 3
        }

        #endregion Status data

        #region Event handlers

        /// <summary>
        /// Called when the mouse pointer moved over the range of this listbox.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="evtArgs">The details of the event.</param>
        private void OnMouseMove(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.IsEnabled)
            {
                if (this.currentStatus == Status.Selecting)
                {
                    if (evtArgs.Position.Y < 0)
                    {
                        /// Start to scroll up.
                        if (this.totalItemCount > this.visibleItemCount)
                        {
                            this.currentStatus = Status.ScrollingUp;
                            this.highlightedIndex = Math.Max(this.highlightedIndex - 1, 0);
                            this.firstVisibleIndex = Math.Max(this.firstVisibleIndex - 1, 0);
                            if (this.scrollbar != null)
                            {
                                this.internalScrollBarValueChange = true;
                                this.scrollbar.SelectedValue = this.firstVisibleIndex;
                                this.internalScrollBarValueChange = false;
                            }
                            UIRoot.Instance.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
                        }
                    }
                    else if (evtArgs.Position.Y >= this.Range.Height)
                    {
                        /// Start to scroll down.
                        if (this.totalItemCount > this.visibleItemCount)
                        {
                            this.currentStatus = Status.ScrollingDown;
                            this.highlightedIndex = Math.Min(this.highlightedIndex + 1, this.totalItemCount - 1);
                            this.firstVisibleIndex = Math.Min(this.firstVisibleIndex + 1, this.totalItemCount - this.visibleItemCount);
                            if (this.scrollbar != null)
                            {
                                this.internalScrollBarValueChange = true;
                                this.scrollbar.SelectedValue = this.firstVisibleIndex;
                                this.internalScrollBarValueChange = false;
                            }
                            UIRoot.Instance.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
                        }
                    }
                    else
                    {
                        /// No scrolling.
                        this.highlightedIndex = this.totalItemCount > 0 ?
                                                Math.Min((evtArgs.Position.Y / this.itemSize.Y) + this.firstVisibleIndex, this.totalItemCount - 1) :
                                                -1;
                    }
                }
                else if (this.currentStatus == Status.ScrollingUp || this.currentStatus == Status.ScrollingDown)
                {
                    if (evtArgs.Position.Y >= 0 && evtArgs.Position.Y < this.Range.Height)
                    {
                        /// Stop scrolling.
                        this.currentStatus = Status.Selecting;
                        this.highlightedIndex = this.totalItemCount > 0 ?
                                                Math.Min((evtArgs.Position.Y / this.itemSize.Y) + this.firstVisibleIndex, this.totalItemCount - 1) :
                                                -1;
                        UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
                    }
                }
            }
        }

        /// <summary>
        /// Called when a mouse button has been pushed over the range of this listbox.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="evtArgs">The details of the event.</param>
        private void OnMouseDown(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.IsEnabled && this.activatorBtn == UIMouseButton.Undefined &&
                evtArgs.Button == UIMouseButton.Left)
            {
                this.activatorBtn = evtArgs.Button;

                if (this.scrollbar == null ||
                   (this.scrollbar != null && !(this.scrollbar.Position + this.scrollbar.Range).Contains(evtArgs.Position)))
                {
                    this.highlightedIndex = this.totalItemCount > 0 ?
                                            Math.Min((evtArgs.Position.Y / this.itemSize.Y) + this.firstVisibleIndex, this.totalItemCount - 1) :
                                            -1;
                    this.currentStatus = Status.Selecting;
                }
            }
        }

        /// <summary>
        /// Called when a mouse button has been released over the range of this listbox.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="evtArgs">The details of the event.</param>
        private void OnMouseUp(UISensitiveObject sender, UIMouseEventArgs evtArgs)
        {
            if (this.IsEnabled && this.activatorBtn == UIMouseButton.Left &&
                evtArgs.Button == UIMouseButton.Left)
            {
                this.activatorBtn = UIMouseButton.Undefined;

                if (this.currentStatus != Status.Normal)
                {
                    this.currentStatus = Status.Normal;
                    this.SelectedIndex = this.highlightedIndex;
                }
            }
        }

        /// <summary>
        /// This method is called on every frame update.
        /// </summary>
        /// <param name="evtArgs">Contains timing informations.</param>
        private void OnFrameUpdate(UIUpdateSystemEventArgs evtArgs)
        {
            if (this.IsEnabled)
            {
                this.timeSinceLastScroll += evtArgs.TimeSinceLastUpdate;
                if (this.timeSinceLastScroll > this.timeBetweenScrolls)
                {
                    this.timeSinceLastScroll = 0;
                    if (this.currentStatus == Status.ScrollingUp)
                    {
                        this.highlightedIndex = Math.Max(this.highlightedIndex - 1, 0);
                        this.firstVisibleIndex = Math.Max(this.firstVisibleIndex - 1, 0);
                        if (this.scrollbar != null)
                        {
                            this.internalScrollBarValueChange = true;
                            this.scrollbar.SelectedValue = this.firstVisibleIndex;
                            this.internalScrollBarValueChange = false;
                        }
                    }
                    else if (this.currentStatus == Status.ScrollingDown)
                    {
                        this.highlightedIndex = Math.Min(this.highlightedIndex + 1, this.totalItemCount - 1);
                        this.firstVisibleIndex = Math.Min(this.firstVisibleIndex + 1, this.totalItemCount - this.visibleItemCount);
                        if (this.scrollbar != null)
                        {
                            this.internalScrollBarValueChange = true;
                            this.scrollbar.SelectedValue = this.firstVisibleIndex;
                            this.internalScrollBarValueChange = false;
                        }
                    }
                }
           }
        }

        /// <summary>
        /// Called when the SelectedValue property of the vertical scrollbar has been changed.
        /// </summary>
        /// <param name="sender">Reference to the scrollbar that sent this event.</param>
        private void OnScrollBarValueChanged(UISensitiveObject sender)
        {
            if (!this.internalScrollBarValueChange)
            {
                UIScrollBar senderScrollBar = (UIScrollBar)sender;
                this.firstVisibleIndex = senderScrollBar.SelectedValue;
            }
        }

        /// <summary>
        /// This flag indicates whether the scrollbar value has been changed internally or not.
        /// </summary>
        private bool internalScrollBarValueChange;

        /// <summary>
        /// The UIMouseButton that activated the mouse sensor of this UIListBox.
        /// </summary>
        private UIMouseButton activatorBtn;

        #endregion Event handlers

        /// <summary>
        /// Reference to the scrollbar or null if there is no scrollbar.
        /// </summary>
        private UIScrollBar scrollbar;

        /// <summary>
        /// The maximum number of items that can be visible in the listbox at a given time.
        /// </summary>
        private int visibleItemCount;

        /// <summary>
        /// The number of the items currently in the listbox.
        /// </summary>
        private int totalItemCount;

        /// <summary>
        /// The size of an item in the listbox.
        /// </summary>
        private RCIntVector itemSize;

        /// <summary>
        /// The index of the selected item or -1 if none of the items is selected.
        /// </summary>
        private int selectedIndex;

        /// <summary>
        /// The index of the highlighted item or -1 if none of the items is highlighted.
        /// </summary>
        private int highlightedIndex;

        /// <summary>
        /// The index of the first visible item or -1 if the are no items in the listbox.
        /// </summary>
        private int firstVisibleIndex;

        /// <summary>
        /// The current status of this UIListBox.
        /// </summary>
        private Status currentStatus;

        /// <summary>
        /// Temporary storage of the elapsed time since the last scrolling of the listbox.
        /// </summary>
        private int timeSinceLastScroll;

        /// <summary>
        /// The minimum time should be elapsed between scrolling the listbox in milliseconds.
        /// </summary>
        private int timeBetweenScrolls;
    }
}
