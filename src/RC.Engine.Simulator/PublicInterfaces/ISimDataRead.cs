using RC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// Read interface of simulation data of a built-in type.
    /// </summary>
    public interface ISimDataRead
    {
        /// <summary>
        /// Reads a 8-bit byte referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a byte.
        /// </exception>
        byte ReadByte();

        /// <summary>
        /// Reads a 16-bit little-endian signed integer referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a short.
        /// </exception>
        short ReadShort();

        /// <summary>
        /// Reads a 32-bit little-endian signed integer referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not an int.
        /// </exception>
        int ReadInt();

        /// <summary>
        /// Reads a 64-bit little-endian signed integer referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a long.
        /// </exception>
        long ReadLong();

        /// <summary>
        /// Reads an RCNumber referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a num.
        /// </exception>
        RCNumber ReadNumber();

        /// <summary>
        /// Reads an RCIntVector referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not an intvect.
        /// </exception>
        RCIntVector ReadIntVector();

        /// <summary>
        /// Reads an RCNumVector referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a numvect.
        /// </exception>
        RCNumVector ReadNumVector();

        /// <summary>
        /// Reads an RCIntRectangle referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not an intrect.
        /// </exception>
        RCIntRectangle ReadIntRectangle();

        /// <summary>
        /// Reads an RCNumRectangle referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a numrect.
        /// </exception>
        RCNumRectangle ReadNumRectangle();
    }
}
