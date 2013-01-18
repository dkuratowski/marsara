using System;
using System.Net;
using RC.Common;
using RC.Common.Diagnostics;

namespace RC.NetworkingSystem
{
    /// <summary>
    /// Contains informations about a lobby announced on the network.
    /// </summary>
    public class LobbyInfo
    {
        /// <summary>
        /// Constructs a LobbyInfo object from an RCPackage.
        /// </summary>
        /// <param name="source">The package that contains the LobbyInfo data.</param>
        /// <returns>The contructed LobbyInfo or null if no LobbyInfo can be constructed from the given RCPackage.</returns>
        public static LobbyInfo FromRCPackage(RCPackage source)
        {
            if (null != source && source.IsCommitted && source.PackageFormat.ID == Network.FORMAT_LOBBY_INFO)
            {
                string idStr = source.ReadString(0);
                Guid id;
                if (!Guid.TryParse(idStr, out id))
                {
                    TraceManager.WriteAllTrace(string.Format("Unable to parse {0} as a GUID!", idStr), NetworkingSystemTraceFilters.INFO);
                    return null;
                }

                byte[] customDataBytes = source.ReadByteArray(3);
                int parsedBytes = 0;
                RCPackage customData = null;

                if (customDataBytes.Length > 0)
                {
                    RCPackage.Parse(customDataBytes, 0, customDataBytes.Length, out parsedBytes);
                }
                if (customDataBytes.Length == 0 ||
                    (null != customData && customData.IsCommitted && parsedBytes == customDataBytes.Length))
                {
                    LobbyInfo retInfo = new LobbyInfo(id,                       /// ID
                                                      source.ReadString(1),     /// IPAddress
                                                      source.ReadInt(2),        /// PortNumber
                                                      (customDataBytes.Length != 0) ? customData : null  /// Custom infos about the lobby
                                                     );
                    return retInfo;
                }
                else
                {
                    TraceManager.WriteAllTrace("LobbyInfo.FromRCPackage failed: unexpected CustomData package format!", NetworkingSystemTraceFilters.INFO);
                    return null;
                }
            }
            else
            {
                TraceManager.WriteAllTrace("LobbyInfo.FromRCPackage failed: unexpected package format!", NetworkingSystemTraceFilters.INFO);
                return null;
            }
        }

        /// <summary>
        /// Creates a LobbyInfo structure without custom data.
        /// </summary>
        public LobbyInfo(Guid id, string ipAddress, int portNumber)
        {
            if (id == Guid.Empty) { throw new ArgumentException("Guid.NULL", "id"); }
            if (ipAddress == null) { throw new ArgumentNullException("ipAddress"); }
            if (portNumber < IPEndPoint.MinPort || portNumber > IPEndPoint.MaxPort) { throw new ArgumentOutOfRangeException("portNumber"); }

            this.ID = id;
            this.IPAddress = ipAddress;
            this.PortNumber = portNumber;
            this.CustomData = null;
        }

        /// <summary>
        /// Creates a LobbyInfo structure with custom data.
        /// </summary>
        public LobbyInfo(Guid id, string ipAddress, int portNumber, RCPackage customData)
        {
            if (id == Guid.Empty) { throw new ArgumentException("Guid.NULL", "id"); }
            if (ipAddress == null) { throw new ArgumentNullException("ipAddress"); }
            if (portNumber < IPEndPoint.MinPort || portNumber > IPEndPoint.MaxPort) { throw new ArgumentOutOfRangeException("portNumber"); }

            this.ID = id;
            this.IPAddress = ipAddress;
            this.PortNumber = portNumber;
            if (null == customData)
            {
                this.CustomData = null;
            }
            else
            {
                if (customData.IsCommitted)
                {
                    this.CustomData = customData;
                }
                else
                {
                    TraceManager.WriteAllTrace("LobbyInfo.LobbyInfo warning: CustomData is not committed.", NetworkingSystemTraceFilters.INFO);
                    this.CustomData = null;
                }
            }
        }

        /// <summary>
        /// Gets the string representation of this LobbyInfo.
        /// </summary>
        /// <remarks>Use this function for debugging.</remarks>
        public override string ToString()
        {
            string retStr = string.Empty;
            retStr += "{" + this.ID.ToString() + "}" + "@" + this.IPAddress + ":" + this.PortNumber;
            return retStr;
        }

        /// <summary>
        /// Converts this LobbyInfo to an RCPackage that can be sent over the network.
        /// </summary>
        public RCPackage Package
        {
            get
            {
                RCPackage package = RCPackage.CreateCustomDataPackage(Network.FORMAT_LOBBY_INFO);
                package.WriteString(0, this.ID.ToString());
                package.WriteString(1, this.IPAddress);
                package.WriteInt(2, this.PortNumber);
                /// Write the custom data into the package or an empty byte array if no custom data exists
                if (this.CustomData != null)
                {
                    byte[] customDataBytes = new byte[this.CustomData.PackageLength];
                    this.CustomData.WritePackageToBuffer(customDataBytes, 0);
                    package.WriteByteArray(3, customDataBytes);
                }
                else
                {
                    package.WriteByteArray(3, new byte[0]);
                }
                return package;
            }
        }

        /// <summary>
        /// ID of the lobby.
        /// </summary>
        public Guid ID;

        /// <summary>
        /// IP address of the host that created the lobby.
        /// </summary>
        public string IPAddress;

        /// <summary>
        /// Port number on the host where clients can connect to.
        /// </summary>
        public int PortNumber;

        /// <summary>
        /// Custom informations embedded into this LobbyInfo. This might be used by higher level layers.
        /// </summary>
        public RCPackage CustomData;
    }
}
