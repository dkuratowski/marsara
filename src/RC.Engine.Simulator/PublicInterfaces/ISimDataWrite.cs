using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// Write interface of simulation data of a built-in type.
    /// </summary>
    public interface ISimDataWrite
    {
        /// <summary>
        /// Writes a 8-bit byte referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a byte.
        /// </exception>
        void WriteByte(byte newVal);

        /// <summary>
        /// Writes the 16-bit little-endian signed integer referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a short.
        /// </exception>
        void WriteShort(short newVal);

        /// <summary>
        /// Writes the 32-bit little-endian signed integer referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not an int.
        /// </exception>
        void WriteInt(int newVal);

        /// <summary>
        /// Writes the 64-bit little-endian signed integer referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a long.
        /// </exception>
        void WriteLong(long newVal);

        /// <summary>
        /// Writes the RCNumber referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a num.
        /// </exception>
        void WriteNumber(RCNumber newVal);

        /// <summary>
        /// Writes the RCIntVector referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not an intvect.
        /// </exception>
        void WriteIntVector(RCIntVector newVal);

        /// <summary>
        /// Writes the RCNumVector referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a numvect.
        /// </exception>
        void WriteNumVector(RCNumVector newVal);

        /// <summary>
        /// Writes the RCIntRectangle referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not an intrect.
        /// </exception>
        void WriteIntRectangle(RCIntRectangle newVal);

        /// <summary>
        /// Writes the RCNumRectangle referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a numrect.
        /// </exception>
        void WriteNumRectangle(RCNumRectangle newVal);
    }
}
