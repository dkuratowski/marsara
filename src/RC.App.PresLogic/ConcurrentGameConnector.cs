using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.App.PresLogic
{
    /// <summary>
    /// Handles a collection of connectors that will be connected/disconnected concurrently. A connection/disconnection operation is
    /// successful only if all of the aggregated operations were successful.
    /// Note: All aggregated connectors shall be in the state ConnectionStatusEnum.Offline.
    /// </summary>
    class ConcurrentGameConnector : IGameConnector, IDisposable
    {
        /// <summary>
        /// Constructs ConcurrentGameConnector instance.
        /// </summary>
        /// <param name="firstConnector">
        /// The first connector to connect concurrently.
        /// </param>
        /// <param name="furtherConnectors">The further connectors to connect concurrently.</param>
        public ConcurrentGameConnector(IGameConnector firstConnector, params IGameConnector[] furtherConnectors)
        {
            if (firstConnector == null) { throw new ArgumentNullException("firstConnector"); }
            if (furtherConnectors == null) { throw new ArgumentNullException("furtherConnectors"); }

            this.connectedConnectors = new HashSet<IGameConnector>();
            this.disconnectedConnectors = new HashSet<IGameConnector>();

            /// Handle the first connector.
            if (firstConnector.ConnectionStatus != ConnectionStatusEnum.Offline) { throw new ArgumentException("All aggregated connectors shall be in the state ConnectionStatusEnum.Offline!", "firstConnector"); }
            this.disconnectedConnectors.Add(firstConnector);
            firstConnector.ConnectorOperationFinished += this.OnOperationFinished;

            /// Handle the further connectors.
            foreach (IGameConnector connector in furtherConnectors)
            {
                if (connector == null) { throw new ArgumentException("None of the elements of furtherConnectors can be null!", "furtherConnectors"); }
                if (connector.ConnectionStatus != ConnectionStatusEnum.Offline) { throw new ArgumentException("All aggregated connectors shall be in the state ConnectionStatusEnum.Offline!", "furtherConnectors"); }
                this.disconnectedConnectors.Add(connector);
                connector.ConnectorOperationFinished += this.OnOperationFinished;
            }

            this.isDisposed = false;
            this.currentStatus = ConnectionStatusEnum.Offline;
        }

        #region IDisposable members

        /// <see cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (this.isDisposed) { throw new ObjectDisposedException("ConcurrentGameConnector"); }
            if (this.currentStatus != ConnectionStatusEnum.Offline) { throw new InvalidOperationException("The connector is not offline!"); }

            foreach (IGameConnector connector in this.disconnectedConnectors) { connector.ConnectorOperationFinished -= this.OnOperationFinished; }
            this.isDisposed = true;
        }

        #endregion IDisposable members

        #region IGameConnector members

        /// <see cref="IGameConnector.Connect"/>
        public void Connect()
        {
            if (this.isDisposed) { throw new ObjectDisposedException("ConcurrentGameConnector"); }
            if (this.currentStatus != ConnectionStatusEnum.Offline) { throw new InvalidOperationException("The connector is not offline!"); }

            this.currentStatus = ConnectionStatusEnum.Connecting;
            foreach (IGameConnector connector in this.disconnectedConnectors) { connector.Connect(); }
        }

        /// <see cref="IGameConnector.Disconnect"/>
        public void Disconnect()
        {
            if (this.isDisposed) { throw new ObjectDisposedException("ConcurrentGameConnector"); }
            if (this.currentStatus != ConnectionStatusEnum.Online) { throw new InvalidOperationException("The connector is not online!"); }

            this.currentStatus = ConnectionStatusEnum.Disconnecting;
            foreach (IGameConnector connector in this.connectedConnectors) { connector.Disconnect(); }
        }

        /// <see cref="IGameConnector.ConnectionStatus"/>
        public ConnectionStatusEnum ConnectionStatus
        {
            get
            {
                if (this.isDisposed) { throw new ObjectDisposedException("ConcurrentGameConnector"); }
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
            if (this.isDisposed) { throw new ObjectDisposedException("ConcurrentGameConnector"); }
            if (connector == null) { throw new ArgumentNullException("connector"); }

            if (this.currentStatus == ConnectionStatusEnum.Connecting)
            {
                if (!this.disconnectedConnectors.Contains(connector)) { throw new InvalidOperationException("Unknown connector!"); }
                if (connector.ConnectionStatus != ConnectionStatusEnum.Online) { throw new InvalidOperationException("Aggregated connector is not online!"); }

                this.disconnectedConnectors.Remove(connector);
                this.connectedConnectors.Add(connector);

                if (this.disconnectedConnectors.Count == 0)
                {
                    this.currentStatus = ConnectionStatusEnum.Online;
                    if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
                }
            }
            else if (this.currentStatus == ConnectionStatusEnum.Disconnecting)
            {
                if (!this.connectedConnectors.Contains(connector)) { throw new InvalidOperationException("Unknown connector!"); }
                if (connector.ConnectionStatus != ConnectionStatusEnum.Offline) { throw new InvalidOperationException("Aggregated connector is not offline!"); }

                this.connectedConnectors.Remove(connector);
                this.disconnectedConnectors.Add(connector);

                if (this.connectedConnectors.Count == 0)
                {
                    this.currentStatus = ConnectionStatusEnum.Offline;
                    if (this.ConnectorOperationFinished != null) { this.ConnectorOperationFinished(this); }
                }
            }
            else
            {
                throw new InvalidOperationException("IGameConnector.ConnectorOperationFinished event raised unexpectedly!");
            }
        }

        #endregion Internal members

        /// <summary>
        /// List of the connected connectors.
        /// </summary>
        private readonly HashSet<IGameConnector> connectedConnectors;

        /// <summary>
        /// List of the disconnected connectors.
        /// </summary>
        private readonly HashSet<IGameConnector> disconnectedConnectors;

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
