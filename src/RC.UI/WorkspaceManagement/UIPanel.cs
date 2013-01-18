using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.UI
{
    /// <summary>
    /// Represents a method which is called when the status of a UIPanel has been changed.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="newState">The new state of the sender.</param>
    public delegate void UIPanelStateChangedHdl(UIPanel sender, UIPanel.Status newState);

    /// <summary>
    /// Represents a panel on a UIPage.
    /// </summary>
    public class UIPanel : UIContainer
    {
        /// <summary>
        /// Occurs when the status of this UIPanel has been changed.
        /// </summary>
        public event UIPanelStateChangedHdl StatusChanged;

        /// <summary>
        /// Enumerates the possible states of a UIPanel.
        /// </summary>
        public enum Status
        {
            Hidden = 0,
            Appearing = 1,
            Visible = 2,
            Disappearing = 3
        }

        /// <summary>
        /// Enumerates the possible modes of showing a UIPanel.
        /// </summary>
        public enum ShowMode
        {
            Appear = 0,             /// The panel will simply appear on the page.
            DriftFromTop = 1,       /// The panel will drift from the top into the page with a contant deceleration.
            DriftFromRight = 2,     /// The panel will drift from the right into the page with a contant deceleration.
            DriftFromBottom = 3,    /// The panel will drift from the bottom into the page with a contant deceleration.
            DriftFromLeft = 4       /// The panel will drift from the left into the page with a contant deceleration.
        }

        /// <summary>
        /// Enumerates the possible modes of hiding a UIPanel.
        /// </summary>
        public enum HideMode
        {
            Disappear = 0,          /// The panel will simply disappear from the page.
            DriftToTop = 1,         /// The panel will drift to the top of the page with a contant acceleration.
            DriftToRight = 2,       /// The panel will drift to the right of the page with a contant acceleration.
            DriftToBottom = 3,      /// The panel will drift to the bottom of the page with a contant acceleration.
            DriftToLeft = 4         /// The panel will drift to the left of the page with a contant acceleration.
        }

        /// <summary>
        /// Constructs a UIPanel object.
        /// </summary>
        /// <param name="backgroundRect">The area of the background of the panel in workspace coordinates.</param>
        /// <param name="contentRect">The area of the content of the panel relative to the background rectangle.</param>
        /// <param name="showMode">The mode how the panel will appear on a page when being shown.</param>
        /// <param name="hideMode">The mode how the panel will disappear from a page when being hidden.</param>
        /// <param name="appearDuration">
        /// The duration of showing this UIPanel in milliseconds. This parameter will be ignored in case
        /// of ShowMode.Appear.
        /// </param>
        /// <param name="disappearDuration">
        /// The duration of hiding this UIPanel in milliseconds. This parameter will be ignored in case
        /// of HideMode.Disappear.
        /// </param>
        /// <remarks>
        /// The backgroundRect shall entirely contain the contentRect.
        /// The origin of the panel's coordinate system will be the top-left corner of contentRect.
        /// The range rectangle of the panel will be backgroundRect relative to contentRect.
        /// The clip rectangle of the panel will be contentRect in the panel's coordinate system.
        /// </remarks>
        public UIPanel(RCIntRectangle backgroundRect, RCIntRectangle contentRect,
                       ShowMode showMode, HideMode hideMode,
                       int appearDuration, int disappearDuration)
            : base(backgroundRect.Location + contentRect.Location,
                   new RCIntRectangle(contentRect.Location * (-1), backgroundRect.Size))
        {
            /// Set the contentRect as the clip. This line will crash if the backgroundRect doesn't entirely
            /// contain the contentRect.
            this.Clip = new RCIntRectangle(0, 0, contentRect.Width, contentRect.Height);

            this.currentStatus = Status.Hidden;
            this.showMode = showMode;
            this.hideMode = hideMode;
            this.showDuration = appearDuration;
            this.hideDuration = disappearDuration;
            this.normalPosition = this.Position;

            /// Compute the paths of showing/hiding the panel
            this.ComputeShowPath();
            this.ComputeHidePath();
        }

        /// <summary>
        /// Gets the current status of this UIPanel.
        /// </summary>
        public Status CurrentStatus { get { return this.currentStatus; } }

        /// <summary>
        /// Shows this panel on the active page. Showing a panel is only available if the panel is in Hidden
        /// state, is on the active UIPage and there is no opened UIDialogs at the UIWorkspace.
        /// </summary>
        public void Show()
        {
            if (this.currentStatus != Status.Hidden) { throw new InvalidOperationException("UIPanel can only be shown in Hidden state!"); }

            if (this.showMode == ShowMode.Appear)
            {
                this.currentStatus = Status.Appearing;
                if (this.StatusChanged != null) { this.StatusChanged(this, this.currentStatus); }
                this.currentStatus = Status.Visible;
                this.Position = this.normalPosition;
                if (this.StatusChanged != null) { this.StatusChanged(this, this.currentStatus); }
            }
            else
            {
                this.currentStatus = Status.Appearing;
                this.Position = this.showStartPos;
                this.movementStartTime = -1;
                if (this.StatusChanged != null) { this.StatusChanged(this, this.currentStatus); }
                UIRoot.Instance.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
            }
        }

        /// <summary>
        /// Hides this panel on the active page. Hiding a panel is only available if the panel is in Visible
        /// state, is on the active UIPage and there is no opened UIDialogs at the UIWorkspace.
        /// </summary>
        public void Hide()
        {
            if (this.currentStatus != Status.Visible) { throw new InvalidOperationException("UIPanel can only be hidden in Visible state!"); }

            if (this.hideMode == HideMode.Disappear)
            {
                this.currentStatus = Status.Disappearing;
                if (this.StatusChanged != null) { this.StatusChanged(this, this.currentStatus); }
                this.currentStatus = Status.Hidden;
                this.Position = this.normalPosition;
                if (this.StatusChanged != null) { this.StatusChanged(this, this.currentStatus); }
            }
            else
            {
                this.currentStatus = Status.Disappearing;
                this.Position = this.normalPosition;
                this.movementStartTime = -1;
                if (this.StatusChanged != null) { this.StatusChanged(this, this.currentStatus); }
                UIRoot.Instance.SystemEventQueue.Subscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
            }
        }

        #region Internal methods

        /// <summary>
        /// This method is called on every frame update.
        /// </summary>
        /// <param name="evtArgs">Contains timing informations.</param>
        private void OnFrameUpdate(UIUpdateSystemEventArgs evtArgs)
        {
            if (this.movementStartTime == -1)
            {
                /// First update event
                this.movementStartTime = evtArgs.TimeSinceStart - evtArgs.TimeSinceLastUpdate;
            }

            int t = evtArgs.TimeSinceStart - this.movementStartTime;
            if (this.currentStatus == Status.Appearing)
            {
                if (t > this.showDuration)
                {
                    /// Appearing of the panel finished
                    this.currentStatus = Status.Visible;
                    this.Position = this.normalPosition;
                    UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
                    if (this.StatusChanged != null) { this.StatusChanged(this, this.currentStatus); }
                }
                else
                {
                    int s = this.showPathLength - (int)(this.showFuncCoeff * (float)((this.showDuration - t) * (this.showDuration - t)));

                    if (this.showMode == ShowMode.DriftFromBottom) { this.Position = this.showStartPos + new RCIntVector(0, -s); }
                    else if (this.showMode == ShowMode.DriftFromLeft) { this.Position = this.showStartPos + new RCIntVector(s, 0); }
                    else if (this.showMode == ShowMode.DriftFromRight) { this.Position = this.showStartPos + new RCIntVector(-s, 0); }
                    else if (this.showMode == ShowMode.DriftFromTop) { this.Position = this.showStartPos + new RCIntVector(0, s); }
                    else { throw new UIException("Unexpected call on UIPanel.OnFrameUpdate!"); }
                }
            }
            else if (this.currentStatus == Status.Disappearing)
            {
                if (t > this.hideDuration)
                {
                    /// Disappearing of the panel finished
                    this.currentStatus = Status.Hidden;
                    this.Position = this.normalPosition;
                    UIRoot.Instance.SystemEventQueue.Unsubscribe<UIUpdateSystemEventArgs>(this.OnFrameUpdate);
                    if (this.StatusChanged != null) { this.StatusChanged(this, this.currentStatus); }
                }
                else
                {
                    int s = (int)(this.hideFuncCoeff * (float)(t * t));

                    if (this.hideMode == HideMode.DriftToBottom) { this.Position = this.normalPosition + new RCIntVector(0, s); }
                    else if (this.hideMode == HideMode.DriftToLeft) { this.Position = this.normalPosition + new RCIntVector(-s, 0); }
                    else if (this.hideMode == HideMode.DriftToRight) { this.Position = this.normalPosition + new RCIntVector(s, 0); }
                    else if (this.hideMode == HideMode.DriftToTop) { this.Position = this.normalPosition + new RCIntVector(0, -s); }
                    else { throw new UIException("Unexpected call on UIPanel.OnFrameUpdate!"); }
                }
            }
        }

        /// <summary>
        /// Computes the show-path and the coefficient of the show-function of this panel.
        /// </summary>
        private void ComputeShowPath()
        {
            RCIntVector showPath = RCIntVector.Undefined;
            if (this.showMode == ShowMode.DriftFromBottom)
            {
                showPath = (this.Position + new RCIntVector(0, this.Range.Location.Y)) -
                           new RCIntVector(this.Position.X, UIWorkspace.Instance.WorkspaceSize.Y);
            }
            else if (this.showMode == ShowMode.DriftFromLeft)
            {
                showPath = (this.Position + new RCIntVector(this.Range.Right, 0)) -
                           new RCIntVector(0, this.Position.Y);
            }
            else if (this.showMode == ShowMode.DriftFromRight)
            {
                showPath = (this.Position + new RCIntVector(this.Range.Location.X, 0)) -
                           new RCIntVector(UIWorkspace.Instance.WorkspaceSize.X, this.Position.Y);
            }
            else if (this.showMode == ShowMode.DriftFromTop)
            {
                showPath = (this.Position + new RCIntVector(0, this.Range.Bottom)) -
                           new RCIntVector(this.Position.X, 0);
            }
            else
            {
                showPath = RCIntVector.Undefined;
                showStartPos = RCIntVector.Undefined;
            }

            if (showPath != RCIntVector.Undefined)
            {
                this.showStartPos = this.Position - showPath;
                this.showPathLength = (int)showPath.Length;
                this.showFuncCoeff = this.showPathLength / (float)(this.showDuration * this.showDuration);
            }
        }

        /// <summary>
        /// Computes the hide-path and the coefficient of the hide-function of this panel.
        /// </summary>
        private void ComputeHidePath()
        {
            RCIntVector hidePath = RCIntVector.Undefined;
            if (this.hideMode == HideMode.DriftToBottom)
            {
                hidePath = new RCIntVector(this.Position.X, UIWorkspace.Instance.WorkspaceSize.Y) -
                           (this.Position + new RCIntVector(0, this.Range.Location.Y));
            }
            else if (this.hideMode == HideMode.DriftToLeft)
            {
                hidePath = new RCIntVector(0, this.Position.Y) -
                           (this.Position + new RCIntVector(this.Range.Right, 0));
            }
            else if (this.hideMode == HideMode.DriftToRight)
            {
                hidePath = new RCIntVector(UIWorkspace.Instance.WorkspaceSize.X, this.Position.Y) -
                           (this.Position + new RCIntVector(this.Range.Location.X, 0));
            }
            else if (this.hideMode == HideMode.DriftToTop)
            {
                hidePath = new RCIntVector(this.Position.X, 0) -
                           (this.Position + new RCIntVector(0, this.Range.Bottom));
            }
            else
            {
                hidePath = RCIntVector.Undefined;
            }
            if (hidePath != RCIntVector.Undefined)
            {
                this.hidePathLength = (int)hidePath.Length;
                this.hideFuncCoeff = this.hidePathLength / (float)(this.hideDuration * this.hideDuration);
            }
        }

        #endregion Internal methods

        /// <summary>
        /// The current state of this UIPanel.
        /// </summary>
        private Status currentStatus;

        /// <summary>
        /// The mode of showing this UIPanel.
        /// </summary>
        private ShowMode showMode;

        /// <summary>
        /// The duration of showing this UIPanel in milliseconds.
        /// </summary>
        private int showDuration;

        /// <summary>
        /// The mode of hiding this UIPanel.
        /// </summary>
        private HideMode hideMode;

        /// <summary>
        /// The duration of hiding this UIPanel in milliseconds.
        /// </summary>
        private int hideDuration;

        /// <summary>
        /// The length of the path of this UIPanel from it's hidden position to it's original position when being shown.
        /// </summary>
        private int showPathLength;

        /// <summary>
        /// The length of the path of this UIPanel from it's original position to it's hidden position when being hidden.
        /// </summary>
        private int hidePathLength;

        /// <summary>
        /// The starting position of the show path.
        /// </summary>
        private RCIntVector showStartPos;

        /// <summary>
        /// The starting position of the hide path.
        /// </summary>
        private RCIntVector normalPosition;

        /// <summary>
        /// The coefficient in the movement function of showing the panel.
        /// </summary>
        private float showFuncCoeff;

        /// <summary>
        /// The coefficient in the movement function of hiding the panel.
        /// </summary>
        private float hideFuncCoeff;

        /// <summary>
        /// The absolute time at the starting point of show- or hide path.
        /// </summary>
        private int movementStartTime;
    }
}
