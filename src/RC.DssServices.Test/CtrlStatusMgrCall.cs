namespace RC.DssServices.Test
{
    /// <summary>
    /// SynchronUiCall on the ControlStatusManager of the UI.
    /// </summary>
    class CtrlStatusMgrCall : SynchronUiCall
    {
        /// <summary>
        /// Constructs a CtrlStatusMgrCall object.
        /// </summary>
        public CtrlStatusMgrCall(ControlStatusManager statusMgr, FormStatus newStatus)
        {
            this.statusMgr = statusMgr;
            this.newStatus = newStatus;
        }

        /// <see cref="SynchronUiCall.Execute_i"/>
        protected override void Execute_i()
        {
            this.statusMgr.Status = this.newStatus;
            this.statusMgr.UpdateControls();
        }

        /// <summary>
        /// The ControlStatusManager object that will be called.
        /// </summary>
        private ControlStatusManager statusMgr;

        /// <summary>
        /// The new status to set.
        /// </summary>
        private FormStatus newStatus;
    }

    /// <summary>
    /// SynchronUiCall on the HostControlStatusManager of the UI.
    /// </summary>
    class HostCtrlStatusMgrCall : SynchronUiCall
    {
        /// <summary>
        /// Constructs a HostCtrlStatusMgrCall object.
        /// </summary>
        public HostCtrlStatusMgrCall(HostControlStatusManager statusMgr, HostControlStatus newStatus)
        {
            this.statusMgr = statusMgr;
            this.newStatus = newStatus;
        }

        /// <see cref="SynchronUiCall.Execute_i"/>
        protected override void Execute_i()
        {
            this.statusMgr.Status = this.newStatus;
            this.statusMgr.UpdateControls();
        }

        /// <summary>
        /// The HostControlStatusManager object that will be called.
        /// </summary>
        private HostControlStatusManager statusMgr;

        /// <summary>
        /// The new status to set.
        /// </summary>
        private HostControlStatus newStatus;
    }

    /// <summary>
    /// SynchronUiCall on a GuestControlStatusManager of the UI.
    /// </summary>
    class GuestCtrlStatusMgrCall : SynchronUiCall
    {
        /// <summary>
        /// Constructs a GuestCtrlStatusMgrCall object.
        /// </summary>
        public GuestCtrlStatusMgrCall(GuestControlStatusManager statusMgr, GuestControlStatus newStatus)
        {
            this.statusMgr = statusMgr;
            this.newStatus = newStatus;
        }

        /// <see cref="SynchronUiCall.Execute_i"/>
        protected override void Execute_i()
        {
            this.statusMgr.Status = this.newStatus;
            this.statusMgr.UpdateControls();
        }

        /// <summary>
        /// The GuestControlStatusManager object that will be called.
        /// </summary>
        private GuestControlStatusManager statusMgr;

        /// <summary>
        /// The new status to set.
        /// </summary>
        private GuestControlStatus newStatus;
    }

    /// <summary>
    /// SynchronUiCall for select a new color in the combobox of the host.
    /// </summary>
    class HostChangeColorCall : SynchronUiCall
    {
        /// <summary>
        /// Constructs a HostChangeColorCall object.
        /// </summary>
        public HostChangeColorCall(HostControlStatusManager statusMgr, PlayerColor newColor)
        {
            this.statusMgr = statusMgr;
            this.newColor = newColor;
        }

        /// <see cref="SynchronUiCall.Execute_i"/>
        protected override void Execute_i()
        {
            this.statusMgr.SelectNewColor(this.newColor);
        }

        /// <summary>
        /// The HostControlStatusManager object that will be called.
        /// </summary>
        private HostControlStatusManager statusMgr;

        /// <summary>
        /// The new color to set.
        /// </summary>
        private PlayerColor newColor;
    }

    /// <summary>
    /// SynchronUiCall for select a new color in the combobox of a guest.
    /// </summary>
    class GuestChangeColorCall : SynchronUiCall
    {
        /// <summary>
        /// Constructs a GuestChangeColorCall object.
        /// </summary>
        public GuestChangeColorCall(GuestControlStatusManager statusMgr, PlayerColor newColor)
        {
            this.statusMgr = statusMgr;
            this.newColor = newColor;
        }

        /// <see cref="SynchronUiCall.Execute_i"/>
        protected override void Execute_i()
        {
            this.statusMgr.SelectNewColor(this.newColor);
        }

        /// <summary>
        /// The GuestControlStatusManager object that will be called.
        /// </summary>
        private GuestControlStatusManager statusMgr;

        /// <summary>
        /// The new color to set.
        /// </summary>
        private PlayerColor newColor;
    }

    /// <summary>
    /// SynchronUiCall for set the UI back to inactive status when the current DSS has been finished.
    /// </summary>
    class EndOfDssUiCall : SynchronUiCall
    {
        /// <summary>
        /// Constructs an EndOfDssUiCall object.
        /// </summary>
        public EndOfDssUiCall(IUiInvoke ui)
        {
            this.ui = ui;
        }

        /// <see cref="SynchronUiCall.Execute_i"/>
        protected override void Execute_i()
        {
            this.ui.EndOfDss();
        }

        /// <summary>
        /// Reference to the UI.
        /// </summary>
        private IUiInvoke ui;
    }
}
