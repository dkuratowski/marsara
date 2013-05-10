using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.InternalInterfaces;
using System.IO;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Implementation of the simulation-heap where every simulation relevant data is stored.
    /// </summary>
    /// TODO: this implementation might have a possible performance issue!
    public class SimulationHeap : ISimulationHeap // TODO: make private
    {
        /// <summary>
        /// Constructs a SimulationHeap with the given page size.
        /// </summary>
        /// <param name="pageSize">The size of the pages on the simulation-heap.</param>
        public SimulationHeap(int pageSize)
        {
            if (pageSize < MIN_PAGESIZE || pageSize > MAX_PAGESIZE) { throw new ArgumentOutOfRangeException("pageSize"); }

            this.pageSize = pageSize;
            this.maxAddress = this.pageSize - 1;
            this.pages = new List<byte[]>();
            this.pages.Add(new byte[this.pageSize]);
        }

        #region ISimulationHeap methods

        /// <see cref="ISimulationHeap.WriteByte"/>
        public void WriteByte(int address, byte newVal)
        {
            if (address > this.maxAddress) { ExtendHeap(); }
            this.pages[address / this.pageSize][address % this.pageSize] = newVal;
        }

        /// <see cref="ISimulationHeap.WriteShort"/>
        public void WriteShort(int address, short newVal)
        {
            if (address > this.maxAddress - 1) { ExtendHeap(); }
            byte[] newValBytes = BitConverter.GetBytes(newVal);
            this.pages[(address + 0) / this.pageSize][(address + 0) % this.pageSize] = newValBytes[0];
            this.pages[(address + 1) / this.pageSize][(address + 1) % this.pageSize] = newValBytes[1];
        }

        /// <see cref="ISimulationHeap.WriteInt"/>
        public void WriteInt(int address, int newVal)
        {
            if (address > this.maxAddress - 3) { ExtendHeap(); }
            byte[] newValBytes = BitConverter.GetBytes(newVal);
            this.pages[(address + 0) / this.pageSize][(address + 0) % this.pageSize] = newValBytes[0];
            this.pages[(address + 1) / this.pageSize][(address + 1) % this.pageSize] = newValBytes[1];
            this.pages[(address + 2) / this.pageSize][(address + 2) % this.pageSize] = newValBytes[2];
            this.pages[(address + 3) / this.pageSize][(address + 3) % this.pageSize] = newValBytes[3];
        }

        /// <see cref="ISimulationHeap.WriteLong"/>
        public void WriteLong(int address, long newVal)
        {
            if (address > this.maxAddress - 7) { ExtendHeap(); }
            byte[] newValBytes = BitConverter.GetBytes(newVal);
            this.pages[(address + 0) / this.pageSize][(address + 0) % this.pageSize] = newValBytes[0];
            this.pages[(address + 1) / this.pageSize][(address + 1) % this.pageSize] = newValBytes[1];
            this.pages[(address + 2) / this.pageSize][(address + 2) % this.pageSize] = newValBytes[2];
            this.pages[(address + 3) / this.pageSize][(address + 3) % this.pageSize] = newValBytes[3];
            this.pages[(address + 4) / this.pageSize][(address + 4) % this.pageSize] = newValBytes[4];
            this.pages[(address + 5) / this.pageSize][(address + 5) % this.pageSize] = newValBytes[5];
            this.pages[(address + 6) / this.pageSize][(address + 6) % this.pageSize] = newValBytes[6];
            this.pages[(address + 7) / this.pageSize][(address + 7) % this.pageSize] = newValBytes[7];
        }

        /// <see cref="ISimulationHeap.ReadByte"/>
        public byte ReadByte(int address)
        {
            return this.pages[address / this.pageSize][address % this.pageSize];
        }

        /// <see cref="ISimulationHeap.ReadShort"/>
        public short ReadShort(int address)
        {
            byte[] valueBytes = new byte[2];
            valueBytes[0] = this.pages[(address + 0) / this.pageSize][(address + 0) % this.pageSize];
            valueBytes[1] = this.pages[(address + 1) / this.pageSize][(address + 1) % this.pageSize];
            return BitConverter.ToInt16(valueBytes, 0);
        }

        /// <see cref="ISimulationHeap.ReadInt"/>
        public int ReadInt(int address)
        {
            byte[] valueBytes = new byte[4];
            valueBytes[0] = this.pages[(address + 0) / this.pageSize][(address + 0) % this.pageSize];
            valueBytes[1] = this.pages[(address + 1) / this.pageSize][(address + 1) % this.pageSize];
            valueBytes[2] = this.pages[(address + 2) / this.pageSize][(address + 2) % this.pageSize];
            valueBytes[3] = this.pages[(address + 3) / this.pageSize][(address + 3) % this.pageSize];
            return BitConverter.ToInt32(valueBytes, 0);
        }

        /// <see cref="ISimulationHeap.ReadLong"/>
        public long ReadLong(int address)
        {
            byte[] valueBytes = new byte[8];
            valueBytes[0] = this.pages[(address + 0) / this.pageSize][(address + 0) % this.pageSize];
            valueBytes[1] = this.pages[(address + 1) / this.pageSize][(address + 1) % this.pageSize];
            valueBytes[2] = this.pages[(address + 2) / this.pageSize][(address + 2) % this.pageSize];
            valueBytes[3] = this.pages[(address + 3) / this.pageSize][(address + 3) % this.pageSize];
            valueBytes[4] = this.pages[(address + 4) / this.pageSize][(address + 4) % this.pageSize];
            valueBytes[5] = this.pages[(address + 5) / this.pageSize][(address + 5) % this.pageSize];
            valueBytes[6] = this.pages[(address + 6) / this.pageSize][(address + 6) % this.pageSize];
            valueBytes[7] = this.pages[(address + 7) / this.pageSize][(address + 7) % this.pageSize];
            return BitConverter.ToInt64(valueBytes, 0);
        }

        #endregion ISimulationHeap methods

        /// <summary>
        /// Extends the heap with a new page.
        /// </summary>
        private void ExtendHeap()
        {
            this.pages.Add(new byte[this.pageSize]);
            this.maxAddress += this.pageSize;
        }

        /// <summary>
        /// The current maximum address value.
        /// </summary>
        private int maxAddress;

        /// <summary>
        /// The list of the pages of the simulation-heap.
        /// </summary>
        private readonly List<byte[]> pages;

        /// <summary>
        /// The size of the pages on the simulation-heap.
        /// </summary>
        private readonly int pageSize;

        /// <summary>
        /// The minimum page size.
        /// </summary>
        private const int MIN_PAGESIZE = 32;

        /// <summary>
        /// The maximum page size.
        /// </summary>
        private const int MAX_PAGESIZE = 65536;
    }
}
