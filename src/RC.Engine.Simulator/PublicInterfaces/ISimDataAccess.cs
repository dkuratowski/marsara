using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.Simulator.PublicInterfaces
{
    /// <summary>
    /// Interface for accessing a particular simulation data stored on the simulation-heap.
    /// </summary>
    /// <remarks>
    /// CAUTION !!! Using this interface after the corresponding simulation data has been deleted from the simulation-heap leads to undefined behavior.
    /// </remarks>
    public interface ISimDataAccess
    {
        /// <summary>
        /// Reads a 8-bit byte from the heap referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a byte.
        /// </exception>
        byte ReadByte();

        /// <summary>
        /// Reads a 16-bit little-endian signed integer from the heap referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a short.
        /// </exception>
        short ReadShort();

        /// <summary>
        /// Reads a 32-bit little-endian signed integer from the heap referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not an int.
        /// </exception>
        int ReadInt();

        /// <summary>
        /// Reads a 64-bit little-endian signed integer from the heap referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a long.
        /// </exception>
        long ReadLong();

        /// <summary>
        /// Reads an RCNumber from the heap referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a num.
        /// </exception>
        RCNumber ReadNumber();

        /// <summary>
        /// Reads an RCIntVector from the heap referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not an intvect.
        /// </exception>
        RCIntVector ReadIntVector();

        /// <summary>
        /// Reads an RCNumVector from the heap referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a numvect.
        /// </exception>
        RCNumVector ReadNumVector();

        /// <summary>
        /// Reads an RCIntRectangle from the heap referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not an intrect.
        /// </exception>
        RCIntRectangle ReadIntRectangle();

        /// <summary>
        /// Reads an RCNumRectangle from the heap referred by this interface.
        /// </summary>
        /// <returns>The value referred by this interface.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a numrect.
        /// </exception>
        RCNumRectangle ReadNumRectangle();

        /// <summary>
        /// Writes a 8-bit byte to the heap referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a byte.
        /// </exception>
        void WriteByte(byte newVal);

        /// <summary>
        /// Writes the 16-bit little-endian signed integer to the heap referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a short.
        /// </exception>
        void WriteShort(short newVal);

        /// <summary>
        /// Writes the 32-bit little-endian signed integer to the heap referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not an int.
        /// </exception>
        void WriteInt(int newVal);

        /// <summary>
        /// Writes the 64-bit little-endian signed integer to the heap referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a long.
        /// </exception>
        void WriteLong(long newVal);

        /// <summary>
        /// Writes the RCNumber to the heap referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a num.
        /// </exception>
        void WriteNumber(RCNumber newVal);

        /// <summary>
        /// Writes the RCIntVector to the heap referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not an intvect.
        /// </exception>
        void WriteIntVector(RCIntVector newVal);

        /// <summary>
        /// Writes the RCNumVector to the heap referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a numvect.
        /// </exception>
        void WriteNumVector(RCNumVector newVal);

        /// <summary>
        /// Writes the RCIntRectangle to the heap referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not an intrect.
        /// </exception>
        void WriteIntRectangle(RCIntRectangle newVal);

        /// <summary>
        /// Writes the RCNumRectangle to the heap referred by this interface.
        /// </summary>
        /// <param name="newVal">The new value to be written.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a numrect.
        /// </exception>
        void WriteNumRectangle(RCNumRectangle newVal);

        /// <summary>
        /// Writes the address of the target simulation data to the pointer on the heap referred by this interface.
        /// </summary>
        /// <param name="target">The target simulation data or null for setting null-pointer.</param>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a pointer.
        /// If the type of the target simulation data doesn't match with the type of this pointer.
        /// </exception>
        void PointTo(ISimDataAccess target);

        /// <summary>
        /// Gets the simulation data addressed by the pointer referred by this interface.
        /// </summary>
        /// <returns>
        /// The access interface of the simulation data addressed by the pointer referred by this interface or null
        /// if the pointer is 0.
        /// </returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a pointer.
        /// If the pointer referred by this interface points to an invalid part of the heap.
        /// </exception>
        ISimDataAccess Dereference();

        /// <summary>
        /// Deletes the simulation data from the heap referred by this interface.
        /// </summary>
        /// <remarks>Don't use this interface after deletion!</remarks>
        void Delete();

        /// <summary>
        /// Deletes the array of simulation data from the heap referred by this interface.
        /// </summary>
        void DeleteArray();

        /// <summary>
        /// Accesses the given field of the composite structure referred by this interface.
        /// </summary>
        /// <param name="fieldIdx">The index of the field of the composite structure to get.</param>
        /// <returns>The interface for accessing the given field.</returns>
        /// <exception cref="SimulationHeapException">
        /// If the type of the data referred by this interface is not a composite structure.
        /// </exception>
        ISimDataAccess AccessField(int fieldIdx);

        /// <summary>
        /// Accesses an item in the array referred by this interface.
        /// </summary>
        /// <param name="itemIdx">The index of the array item to get.</param>
        /// <returns>The interface for accessing the given array item.</returns>
        /// <exception cref="SimulationHeapException">
        /// If this interface is not referring to the first item of an array on the heap.
        /// </exception>
        ISimDataAccess AccessArrayItem(int itemIdx);
    }
}
