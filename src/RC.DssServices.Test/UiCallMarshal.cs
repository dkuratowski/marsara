using System;
using RC.Common;

namespace RC.DssServices.Test
{
    /// <summary>
    /// This class is responsible for marshaling the calls on the UI to the UI-thread.
    /// </summary>
    class UiCallMarshal
    {
        /// <summary>
        /// Contructs a UiCallMarshal object.
        /// </summary>
        public UiCallMarshal(ControlStatusManager statusMgr, IUiInvoke ui)
        {
            this.statusMgr = statusMgr;
            this.ui = ui;
        }

        /// <summary>
        /// Sets the status of the MainForm.
        /// </summary>
        public void SetMainControlStatus(FormStatus newStatus)
        {
            if (RCThread.CurrentThread == MainForm.UiThread) { throw new Exception("This call is not allowed from the UI-thread!"); }

            CtrlStatusMgrCall uiCall = new CtrlStatusMgrCall(this.statusMgr, newStatus);
            this.ui.InvokeUI(uiCall);
        }

        /// <summary>
        /// Sets the status of the host control.
        /// </summary>
        public void SetHostControlStatus(HostControlStatus newStatus)
        {
            if (RCThread.CurrentThread == MainForm.UiThread) { throw new Exception("This call is not allowed from the UI-thread!"); }

            HostCtrlStatusMgrCall uiCall = new HostCtrlStatusMgrCall(this.statusMgr.HostStatus, newStatus);
            this.ui.InvokeUI(uiCall);
        }

        /// <summary>
        /// Sets the status of a guest control.
        /// </summary>
        public void SetGuestControlStatus(int whichGuest, GuestControlStatus newStatus)
        {
            if (RCThread.CurrentThread == MainForm.UiThread) { throw new Exception("This call is not allowed from the UI-thread!"); }
            if (whichGuest < 0 || whichGuest >= this.statusMgr.GuestStatuses.Length) { throw new Exception("Unexpected guest index!"); }

            GuestCtrlStatusMgrCall uiCall = new GuestCtrlStatusMgrCall(this.statusMgr.GuestStatuses[whichGuest], newStatus);
            this.ui.InvokeUI(uiCall);
        }

        /// <summary>
        /// Sets the control status managers back to inactive state when the current DSS has been finished.
        /// </summary>
        public void EndOfDss()
        {
            if (RCThread.CurrentThread == MainForm.UiThread) { throw new Exception("This call is not allowed from the UI-thread!"); }

            EndOfDssUiCall uiCall = new EndOfDssUiCall(this.ui);
            this.ui.InvokeUI(uiCall);
        }

        /// <summary>
        /// Selects a new color at the host's combobox.
        /// </summary>
        public void SelectNewHostColor(PlayerColor color)
        {
            if (RCThread.CurrentThread == MainForm.UiThread) { throw new Exception("This call is not allowed from the UI-thread!"); }

            HostChangeColorCall uiCall = new HostChangeColorCall(this.statusMgr.HostStatus, color);
            this.ui.InvokeUI(uiCall);
        }

        /// <summary>
        /// Selects a new color at a guest's combobox.
        /// </summary>
        public void SelectNewGuestColor(int whichGuest, PlayerColor color)
        {
            if (RCThread.CurrentThread == MainForm.UiThread) { throw new Exception("This call is not allowed from the UI-thread!"); }
            if (whichGuest < 0 || whichGuest >= this.statusMgr.GuestStatuses.Length) { throw new Exception("Unexpected guest index!"); }

            GuestChangeColorCall uiCall = new GuestChangeColorCall(this.statusMgr.GuestStatuses[whichGuest], color);
            this.ui.InvokeUI(uiCall);
        }

        /// <summary>
        /// Refreshes the draw display on the UI.
        /// </summary>
        public void RefreshDisplay()
        {
            if (RCThread.CurrentThread == MainForm.UiThread) { throw new Exception("This call is not allowed from the UI-thread!"); }

            this.ui.InvalidateDisplay();
        }

        /// <summary>
        /// Reference to the ControlStatusManager object.
        /// </summary>
        private ControlStatusManager statusMgr;

        /// <summary>
        /// Reference to the invocation interface of the UI.
        /// </summary>
        private IUiInvoke ui;
    }
}
