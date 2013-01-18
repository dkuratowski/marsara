using System;
using RC.NetworkingSystem;
using RC.Common;

namespace RC.DssServices.Test
{
    /// <summary>
    /// This class is responsible for marshaling the ILobbyLocator calls to the UI-thread.
    /// </summary>
    class LobbyLocatorMarshal : ILobbyLocator
    {
        /// <summary>
        /// Constructs a LobbyLocatorMarshal object.
        /// </summary>
        public LobbyLocatorMarshal(ILobbyLocator locatorObj, IUiInvoke ui)
        {
            this.locatorObj = locatorObj;
            this.ui = ui;
        }

        #region ILobbyLocator members

        /// <see cref="ILobbyLocator.LobbyFound"/>
        public void LobbyFound(LobbyInfo foundLobby)
        {
            if (RCThread.CurrentThread == MainForm.UiThread) { throw new Exception("This call is not allowed from the UI-thread!"); }

            LobbyLocatorUiCall uiCall = new LobbyLocatorUiCall(this.locatorObj,
                                                               LobbyLocatorUiCall.LobbyLocatorMethod.LobbyFound,
                                                               foundLobby);
            this.ui.InvokeUI(uiCall);
        }

        /// <see cref="ILobbyLocator.LobbyChanged"/>
        public void LobbyChanged(LobbyInfo changedLobby)
        {
            if (RCThread.CurrentThread == MainForm.UiThread) { throw new Exception("This call is not allowed from the UI-thread!"); }

            LobbyLocatorUiCall uiCall = new LobbyLocatorUiCall(this.locatorObj,
                                                               LobbyLocatorUiCall.LobbyLocatorMethod.LobbyChanged,
                                                               changedLobby);
            this.ui.InvokeUI(uiCall);
        }

        /// <see cref="ILobbyLocator.LobbyVanished"/>
        public void LobbyVanished(LobbyInfo vanishedLobby)
        {
            if (RCThread.CurrentThread == MainForm.UiThread) { throw new Exception("This call is not allowed from the UI-thread!"); }

            LobbyLocatorUiCall uiCall = new LobbyLocatorUiCall(this.locatorObj,
                                                               LobbyLocatorUiCall.LobbyLocatorMethod.LobbyVanished,
                                                               vanishedLobby);
            this.ui.InvokeUI(uiCall);
        }

        #endregion

        /// <summary>
        /// Reference to the ILobbyLocator interface of the UI.
        /// </summary>
        private ILobbyLocator locatorObj;

        /// <summary>
        /// Reference to the invocation interface of the UI.
        /// </summary>
        private IUiInvoke ui;
    }
}
