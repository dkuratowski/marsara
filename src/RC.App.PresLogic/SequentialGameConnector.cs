using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.UI;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Handles a collection of connectors that will be connected/disconnected sequentially. A connection/disconnection operation is
    /// successful only if all of the aggregated operations were successful.
    /// Note: All aggregated connectors shall be in the state ConnectionStatusEnum.Offline.
    /// </summary>
    class SequentialGameConnector : IGameConnector, IDisposable
    {
        /// <summary>
        /// Constructs SequentialGameConnector instance.
        /// </summary>
        /// <param name="firstConnector">
        /// The first connector to connect sequentially.
        /// </param>
        /// <param name="furtherConnectors">The further connectors to connect sequentially.</param>
        public SequentialGameConnector(IGameConnector firstConnector, params IGameConnector[] furtherConnectors)
        {
            if (firstConnector == null) { throw new ArgumentNullException("firstConnector"); }
            if (furtherConnectors == null) { throw new ArgumentNullException("furtherConnectors"); }

            this.connectors = new List<IGameConnector>();
            this.currentlyHandledConnectorIdx = -1;

            /// Handle the first connector.
            if (firstConnector.ConnectionStatus != ConnectionStatusEnum.Offline) { throw new ArgumentException("All aggregated connectors shall be in the state ConnectionStatusEnum.Offline!", "firstConnector"); }
            this.connectors.Add(firstConnector);

            /// Handle the further connectors.
            foreach (IGameConnector connector in furtherConnectors)
            {
                if (connector == null) { throw new ArgumentException("None of the elements of furtherConnectors can be null!", "furtherConnectors"); }
                if (connector.ConnectionStatus != ConnectionStatusEnum.Offline) { throw new ArgumentException("All aggregated connectors shall be in the state ConnectionStatusEnum.Offline!", "furtherConnectors"); }
                this.connectors.Add(connector);
            }

            this.isDisposed = false;
            this.currentStatus = ConnectionStatusEnum.Offline;
        }

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.isDisposed) { throw new ObjectDisposedException("SequentialGameConnector"); }
            if (this.currentStatus != ConnectionStatusEnum.Offline) { throw new InvalidOperationException("The connector is not offline!"); }
            this.isDisposed = true;
        }

        #endregion IDisposable members

        #region IGameConnector members

        /// <see cref="IGameConnector.Connect"/>
        public void Connect()
        {
            if (this.isDisposed) { throw new ObjectDisposedException("SequentialGameConnector"); }
            if (this.currentStatus != ConnectionStatusEnum.Offline) { throw new InvalidOperationException("The connector is not offline!"); }

            this.currentStatus = ConnectionStatusEnum.Connecting;
            this.currentlyHandledConnectorIdx = 0;

            this.connectors[this.currentlyHandledConnectorIdx].ConnectorOperationFinished += this.OnOperationFinished;
            this.connectors[this.currentlyHandledConnectorIdx].Connect();
        }

        /// <see cref="IGameConnector.Disconnect"/>
        public void Disconnect()
        {
            if (this.isDisposed) { throw new ObjectDisposedException("SequentialGameConnector"); }
            if (this.currentStatus != ConnectionStatusEnum.Online) { throw new InvalidOperationException("The connector is not online!"); }

            this.currentStatus = ConnectionStatusEnum.Disconnecting;
            this.currentlyHandledConnectorIdx = this.connectors.Count - 1;

            this.connectors[this.currentlyHandledConnectorIdx].ConnectorOperationFinished += this.OnOperationFinished;
            this.connectors[this.currentlyHandledConnectorIdx].Disconnect();
        }

        /// <see cref="IGameConnector.ConnectionStatus"/>
        public ConnectionStatusEnum ConnectionStatus
        {
            get
            {
                if (this.isDisposed) { throw new ObjectDisposedException("SequentialGameConnector"); }
                return this.currentStatus;
            }
        }

        /// <see cref="IGameConnector.ConnectorOperationFinished"/>
        public event Action<IGameConnector> ConnectorOperationFinished;

        #endregion IGameConnector members

        #region Internal members

        /// <summary>
        /// This event handler is called when the connection/disconnection operation of an aggregated connector has been finished.
        /// </summary>
        /// <param name="connector">The aggregated connector whose operation has been finished.</param>
        private void OnOperationFinished(IGameConnector connector)
        {
            if (this.isDisposed) { throw new ObjectDisposedException("SequentialGameConnector"); }
            if (connector == null) { throw new ArgumentNullException("connector"); }
            if (this.connectors[this.currentlyHandledConnectorIdx] != connector) { throw new InvalidOperationException("Unknown connector!"); }

            this.connectors[this.currentlyHandledConnectorIdx].ConnectorOperationFinished -= this.OnOperationFinished;

            if (this.currentStatus == ConnectionStatusEnum.Connecting)
            {
                if (connector.ConnectionStatus != ConnectionStatusEnum.Online) { throw new InvalidOperationException("Aggregated connector is not online!"); }

                this.currentlyHandledConnectorIdx++;

                if (this.currentlyHandledConnectorIdx == this.connectors.Count)
                {
                    /// Last connector has been connected.
                    this.currentStatus = ConnectionStatusEnum.Online;
                    this.currentlyHandledConnectorIdx = -1;
                    if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
                }
                else
                {
                    /// To avoid recursive call on the UITaskManager.
                    UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate += this.OnContinueWithNextConnector;
                }
            }
            else if (this.currentStatus == ConnectionStatusEnum.Disconnecting)
            {
                if (connector.ConnectionStatus != ConnectionStatusEnum.Offline) { throw new InvalidOperationException("Aggregated connector is not offline!"); }

                this.currentlyHandledConnectorIdx--;

                if (this.currentlyHandledConnectorIdx < 0)
                {
                    /// Last connector has been disconnected.
                    this.currentStatus = ConnectionStatusEnum.Offline;
                    this.currentlyHandledConnectorIdx = -1;
                    if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
                }
                else
                {
                    /// To avoid recursive call on the UITaskManager.
                    UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate += this.OnContinueWithNextConnector;
                }
            }
            else
            {
                throw new InvalidOperationException("IGameConnector.ConnectorOperationFinished event raised unexpectedly!");
            }
        }

        /// <summary>
        /// To avoid recursive call on the UITaskManager.
        /// </summary>
        private void OnContinueWithNextConnector()
        {
            UIRoot.Instance.GraphicsPlatform.RenderLoop.FrameUpdate -= this.OnContinueWithNextConnector;

            this.connectors[this.currentlyHandledConnectorIdx].ConnectorOperationFinished += this.OnOperationFinished;
            if (this.currentStatus == ConnectionStatusEnum.Connecting)
            {
                this.connectors[this.currentlyHandledConnectorIdx].Connect();
            }
            else if (this.currentStatus == ConnectionStatusEnum.Disconnecting)
            {
                this.connectors[this.currentlyHandledConnectorIdx].Disconnect();
            }
            else
            {
                throw new InvalidOperationException("Unexpected connection status!");
            }
        }

        #endregion Internal members

        /// <summary>
        /// List of the aggregated connectors in order.
        /// </summary>
        private readonly List<IGameConnector> connectors;

        /// <summary>
        /// The index of the connector that is currently being connected/disconnected.
        /// </summary>
        private int currentlyHandledConnectorIdx;

        /// <summary>
        /// The current status of this connector.
        /// </summary>
        private ConnectionStatusEnum currentStatus;

        /// <summary>
        /// This flag indicates if this connector has already been disposed.
        /// </summary>
        private bool isDisposed;
    }
}
