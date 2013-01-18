using RC.NetworkingSystem;

namespace RC.DssServices.Test
{
    /// <summary>
    /// SynchronUiCall of the ILobbyLocator interface.
    /// </summary>
    class LobbyLocatorUiCall : SynchronUiCall
    {
        /// <summary>
        /// Enumerates the methods of the ILobbyListener interface.
        /// </summary>
        public enum LobbyLocatorMethod
        {
            LobbyFound = 0,
            LobbyChanged = 1,
            LobbyVanished = 2
        }

        /// <summary>
        /// Constructs a LobbyLocatorUiCall object.
        /// </summary>
        public LobbyLocatorUiCall(ILobbyLocator locatorObj, LobbyLocatorMethod method, LobbyInfo lobbyArg)
            : base()
        {
            this.locatorObj = locatorObj;
            this.method = method;
            this.lobbyArg = lobbyArg;
        }

        /// <see cref="SynchronUiCall.Execute_i"/>
        protected override void Execute_i()
        {
            if (this.method == LobbyLocatorMethod.LobbyFound)
            {
                this.locatorObj.LobbyFound(this.lobbyArg);
            }
            else if (this.method == LobbyLocatorMethod.LobbyChanged)
            {
                this.locatorObj.LobbyChanged(this.lobbyArg);
            }
            else if (this.method == LobbyLocatorMethod.LobbyVanished)
            {
                this.locatorObj.LobbyVanished(this.lobbyArg);
            }
        }

        /// <summary>
        /// The locator object that will be called in the context of the UI-thread.
        /// </summary>
        private ILobbyLocator locatorObj;

        /// <summary>
        /// The method that has to be called.
        /// </summary>
        private LobbyLocatorMethod method;

        /// <summary>
        /// The argument of the method.
        /// </summary>
        private LobbyInfo lobbyArg;
    }
}
