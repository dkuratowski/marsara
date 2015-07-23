using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.UI
{
    /// <summary>
    /// Represents a method which is called when the status of a UIPage has been changed.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="newState">The new state of the sender.</param>
    public delegate void UIPageStateChangedHdl(UIPage sender, UIPage.Status newState);

    /// <summary>
    /// Represents a page of the UIWorkspace.
    /// </summary>
    public class UIPage : UISensitiveObject
    {
        /// <summary>
        /// Occurs when the status of this UIPage has been changed.
        /// </summary>
        public event UIPageStateChangedHdl StatusChanged;

        /// <summary>
        /// Enumerates the possible states of a UIPage.
        /// </summary>
        public enum Status
        {
            Inactive = 0,
            Active = 1,
            Inactivating = 2
        }

        /// <summary>
        /// Constructs a UIPage object.
        /// </summary>
        public UIPage()
            : base(new RCIntVector(0, 0), new RCIntRectangle(0, 0, UIWorkspace.Instance.WorkspaceSize.X,
                                                             UIWorkspace.Instance.WorkspaceSize.Y))
        {
            this.currentStatus = Status.Inactive;
            this.registeredPanels = new RCSet<UIPanel>();
            this.visiblePanels = new RCSet<UIPanel>();
        }

        /// <summary>
        /// Gets the current status of this UIPage.
        /// </summary>
        public Status CurrentStatus { get { return this.currentStatus; } }

        /// <summary>
        /// Activates this UIPage. Activating a UIPage is only available if there is no active UIPage and
        /// opened UIDialog at the UIWorkspace.
        /// </summary>
        public void Activate()
        {
            if (this.currentStatus != Status.Inactive) { throw new InvalidOperationException("UIPage can only be activated in Inactive state!"); }
            this.currentStatus = Status.Active;
            if (this.StatusChanged != null) { this.StatusChanged(this, this.currentStatus); }
        }

        /// <summary>
        /// Starts to deactivate this UIPage. Deactivating a UIPage is only available if there is no
        /// opened UIDialog at the UIWorkspace.
        /// </summary>
        public void Deactivate()
        {
            if (this.currentStatus != Status.Active) { throw new InvalidOperationException("UIPage can only be deactivated in Active state!"); }
            this.currentStatus = Status.Inactivating;
            if (this.StatusChanged != null) { this.StatusChanged(this, this.currentStatus); }
            foreach (UIPanel panel in this.registeredPanels)
            {
                if (panel.CurrentStatus == UIPanel.Status.Visible) { panel.Hide(); }
            }
        }

        /// <summary>
        /// Registers the given UIPanel to this UIPage.
        /// </summary>
        /// <param name="panel">The panel to register.</param>
        public void RegisterPanel(UIPanel panel)
        {
            if (panel == null) { throw new ArgumentNullException("panel"); }
            if (this.registeredPanels.Contains(panel)) { throw new InvalidOperationException("The given UIPanel is already registered at this UIPage!"); }
            if (panel.CurrentStatus != UIPanel.Status.Hidden) { throw new InvalidOperationException("The given UIPanel is not in Hidden status!"); }
            this.registeredPanels.Add(panel);
            panel.StatusChanged += this.OnPanelStatusChanged;
        }

        /// <summary>
        /// Unregisters the given UIPanel from this UIPage.
        /// </summary>
        /// <param name="panel">The panel to unregister.</param>
        public void UnregisterPanel(UIPanel panel)
        {
            if (panel == null) { throw new ArgumentNullException("panel"); }
            if (!this.registeredPanels.Contains(panel)) { throw new InvalidOperationException("The given UIPanel is not registered at this UIPage!"); }
            if (panel.CurrentStatus != UIPanel.Status.Hidden) { throw new InvalidOperationException("The given UIPanel is not in Hidden status!"); }
            this.registeredPanels.Remove(panel);
            panel.StatusChanged -= this.OnPanelStatusChanged;
        }

        /// <summary>
        /// Called when the status of a registered UIPanel has been changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="newStatus">The new status of the sender.</param>
        private void OnPanelStatusChanged(UIPanel sender, UIPanel.Status newStatus)
        {   
            if (!this.registeredPanels.Contains(sender)) { throw new InvalidOperationException("Unregistered UIPanel!"); }
            if (this.currentStatus == Status.Inactive) { throw new InvalidOperationException("Invalid UIPanel operation!"); }

            /// Add or remove the panel to or from the visiblePanels collection.
            if (newStatus != UIPanel.Status.Hidden) { this.visiblePanels.Add(sender); } else { this.visiblePanels.Remove(sender); }

            if (this.currentStatus == Status.Active)
            {
                if (newStatus == UIPanel.Status.Hidden)
                {
                    this.Detach(sender);
                    TraceManager.WriteAllTrace("UIPanel hidden", UITraceFilters.INFO);
                }
                else if (newStatus == UIPanel.Status.Appearing)
                {
                    this.Attach(sender);
                    TraceManager.WriteAllTrace("UIPanel appearing", UITraceFilters.INFO);
                }
                else if (newStatus == UIPanel.Status.Visible)
                {
                    //this.Detach(sender);
                    this.AttachSensitive(sender);
                    TraceManager.WriteAllTrace("UIPanel visible", UITraceFilters.INFO);
                }
                else if (newStatus == UIPanel.Status.Disappearing)
                {
                    this.DetachSensitive(sender);
                    //this.Attach(sender);
                    TraceManager.WriteAllTrace("UIPanel disappearing", UITraceFilters.INFO);
                }
            }
            else if (this.currentStatus == Status.Inactivating)
            {
                if (newStatus == UIPanel.Status.Visible)
                {
                    TraceManager.WriteAllTrace("UIPanel visible", UITraceFilters.INFO);
                    sender.Hide();
                }
                else if (newStatus == UIPanel.Status.Disappearing)
                {
                    this.DetachSensitive(sender);
                    TraceManager.WriteAllTrace("UIPanel disappearing", UITraceFilters.INFO);
                }
                else if (newStatus == UIPanel.Status.Hidden)
                {
                    this.Detach(sender);
                    TraceManager.WriteAllTrace("UIPanel hidden", UITraceFilters.INFO);
                    if (this.visiblePanels.Count == 0)
                    {
                        this.currentStatus = Status.Inactive;
                        if (this.StatusChanged != null) { this.StatusChanged(this, this.currentStatus); }
                    }
                }
                else
                {
                    throw new InvalidOperationException("Invalid UIPanel operation!");
                }
            }
        }

        /// <summary>
        /// The current status of this UIPage.
        /// </summary>
        private Status currentStatus;

        /// <summary>
        /// List of the UIPanels registered to this UIPage.
        /// </summary>
        private RCSet<UIPanel> registeredPanels;

        /// <summary>
        /// List of the registered UIPanels that are not in Hidden state.
        /// </summary>
        private RCSet<UIPanel> visiblePanels;
    }
}
