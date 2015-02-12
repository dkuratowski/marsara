using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Represents a sprite group loader.
    /// </summary>
    public class SpriteGroupLoader : ISpriteGroup, IGameConnector
    {
        /// <summary>
        /// Creates a SpriteGroupLoader instance.
        /// </summary>
        /// <param name="spriteGroupFactoryMethod">Reference to the factory method that will instantiate the sprite group to be loaded.</param>
        public SpriteGroupLoader(Func<SpriteGroup> spriteGroupFactoryMethod)
        {
            if (spriteGroupFactoryMethod == null) { throw new ArgumentNullException("spriteGroupFactoryMethod"); }

            this.backgroundTask = null;
            this.underlyingSpriteGroup = null;
            this.isConnected = false;
            this.spriteGroupFactoryMethod = spriteGroupFactoryMethod;
        }

        #region ISpriteGroup members

        /// <see cref="ISpriteGroup.Item"/>
        public UISprite this[int index]
        {
            get
            {
                if (this.ConnectionStatus != ConnectionStatusEnum.Online) { throw new InvalidOperationException("SpriteGroupLoader is not online!"); }
                return this.underlyingSpriteGroup[index];
            }
        }

        #endregion ISpriteGroup members

        #region IGameConnector members

        /// <see cref="IGameConnector.Connect"/>
        public void Connect()
        {
            if (this.isConnected || this.backgroundTask != null) { throw new InvalidOperationException("The sprite group loader has been connected or is currently being connected!"); }

            this.underlyingSpriteGroup = this.spriteGroupFactoryMethod();
            this.backgroundTask = UITaskManager.StartParallelTask(o => this.underlyingSpriteGroup.Load(), "SpriteGroupLoader.Connect");
            this.backgroundTask.Finished += this.OnBackgroundTaskFinished;
            this.backgroundTask.Failed += delegate(IUIBackgroundTask sender, object message)
            {
                throw (Exception)message;
            };
        }

        /// <see cref="IGameConnector.Disconnect"/>
        public void Disconnect()
        {
            if (!this.isConnected || this.backgroundTask != null) { throw new InvalidOperationException("The sprite group loader has been disconnected or is currently being disconnected!"); }

            this.backgroundTask = UITaskManager.StartParallelTask(o => this.underlyingSpriteGroup.Unload(), "SpriteGroupLoader.Disconnect");
            this.backgroundTask.Finished += this.OnBackgroundTaskFinished;
            this.backgroundTask.Failed += delegate(IUIBackgroundTask sender, object message)
            {
                throw (Exception)message;
            };
        }

        /// <see cref="IGameConnector.ConnectionStatus"/>
        public ConnectionStatusEnum ConnectionStatus
        {
            get
            {
                if (this.backgroundTask == null) { return this.isConnected ? ConnectionStatusEnum.Online : ConnectionStatusEnum.Offline; }
                else { return this.isConnected ? ConnectionStatusEnum.Disconnecting : ConnectionStatusEnum.Connecting; }
            }
        }

        /// <see cref="IGameConnector.ConnectorOperationFinished"/>
        public event Action<IGameConnector> ConnectorOperationFinished;

        #endregion IGameConnector members

        /// <summary>
        /// Called when the currently running background task has been finished.
        /// </summary>
        private void OnBackgroundTaskFinished(IUIBackgroundTask sender, object message)
        {
            this.backgroundTask = null;
            if (!this.isConnected)
            {
                this.isConnected = true;
                if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
            }
            else
            {
                this.isConnected = false;
                if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
            }
        }

        /// <summary>
        /// This flag indicates whether this sprite group loader has been connected or not.
        /// </summary>
        private bool isConnected;

        /// <summary>
        /// Reference to the factory method that will instantiate the sprite group to be loaded.
        /// </summary>
        private readonly Func<SpriteGroup> spriteGroupFactoryMethod;

        /// <summary>
        /// Reference to the underlying sprite group.
        /// </summary>
        private SpriteGroup underlyingSpriteGroup;

        /// <summary>
        /// Reference to the currently executed connecting/disconnecting task or null if no such a task is under execution.
        /// </summary>
        private IUIBackgroundTask backgroundTask;
    }
}
