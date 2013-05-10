using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.InternalInterfaces
{
    /// <summary>
    /// Defines the interface of the simulation-heap where every simulation relevant information is stored.
    /// </summary>
    public interface ISimulationHeap // TODO: make private
    {
        /// <summary>
        /// Writes a 8-bit byte at the given address.
        /// </summary>
        /// <param name="address">The address to write.</param>
        /// <param name="newVal">The new value.</param>
        void WriteByte(int address, byte newVal);

        /// <summary>
        /// Writes a 16-bit little-endian signed integer at the given address.
        /// </summary>
        /// <param name="address">The address to write.</param>
        /// <param name="newVal">The new value.</param>
        void WriteShort(int address, short newVal);

        /// <summary>
        /// Writes a 32-bit little-endian signed integer at the given address.
        /// </summary>
        /// <param name="address">The address to write.</param>
        /// <param name="newVal">The new value.</param>
        void WriteInt(int address, int newVal);

        /// <summary>
        /// Writes a 64-bit little-endian signed integer at the given address.
        /// </summary>
        /// <param name="address">The address to write.</param>
        /// <param name="newVal">The new value.</param>
        void WriteLong(int address, long newVal);

        /// <summary>
        /// Reads a 8-bit byte from the given address.
        /// </summary>
        /// <param name="address">The address to read.</param>
        /// <returns>The value at the given address.</returns>
        byte ReadByte(int address);

        /// <summary>
        /// Reads a 16-bit little-endian signed integer from the given address.
        /// </summary>
        /// <param name="address">The address to read.</param>
        /// <returns>The value at the given address.</returns>
        short ReadShort(int address);

        /// <summary>
        /// Reads a 32-bit little-endian signed integer from the given address.
        /// </summary>
        /// <param name="address">The address to read.</param>
        /// <returns>The value at the given address.</returns>
        int ReadInt(int address);

        /// <summary>
        /// Reads a 64-bit little-endian signed integer from the given address.
        /// </summary>
        /// <param name="address">The address to read.</param>
        /// <returns>The value at the given address.</returns>
        long ReadLong(int address);
    }
}
