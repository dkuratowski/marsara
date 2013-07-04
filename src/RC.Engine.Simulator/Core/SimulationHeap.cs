using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.InternalInterfaces;
using System.IO;
using RC.Engine.Simulator.PublicInterfaces;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Implementation of the simulation-heap where every simulation data is stored.
    /// </summary>
    /// TODO: this implementation might have a possible performance issue!
    class SimulationHeap : ISimulationHeap
    {
        /// <summary>
        /// Constructs an empty SimulationHeap with the given page size and capacity.
        /// </summary>
        /// <param name="pageSize">The size of the pages on the simulation-heap.</param>
        /// <param name="capacity">The capacity of the simulation-heap.</param>
        public SimulationHeap(int pageSize, int capacity)
        {
            if (pageSize < MIN_PAGESIZE || pageSize > MAX_PAGESIZE) { throw new ArgumentOutOfRangeException("pageSize"); }
            if (capacity < pageSize) { throw new ArgumentOutOfRangeException("capacity"); }

            this.pageSize = pageSize;
            this.capacity = capacity;
            this.maxAddress = this.pageSize - 1;
            this.pages = new List<byte[]>();
            this.pages.Add(new byte[this.pageSize]);
        }

        /// <summary>
        /// Constructs a SimulationHeap with the given content and capacity.
        /// </summary>
        /// <param name="content">The content of the simulation heap.</param>
        /// <param name="capacity">The capacity of the simulation-heap.</param>
        public SimulationHeap(byte[] content, int capacity)
        {
            if (content == null) { throw new ArgumentNullException("content"); }

            int pageSizeAddress = BitConverter.ToInt32(content, 0);

            int pageSize = BitConverter.ToInt32(content, pageSizeAddress);
            if (pageSize < MIN_PAGESIZE || pageSize > MAX_PAGESIZE) { throw new SimulationHeapException("Page-size is out of bounds!"); }
            if (content.Length % pageSize != 0) { throw new SimulationHeapException("The page-size must be a divisor of the size of the content array!"); }
            content[pageSizeAddress + 0] = content[pageSizeAddress + 1] = content[pageSizeAddress + 2] = content[pageSizeAddress + 3] = 0;

            this.pageSize = pageSize;
            this.capacity = capacity;

            /// Cut the content into pages.
            this.pages = new List<byte[]>();
            for (int pageIdx = 0; pageIdx < content.Length / this.pageSize; pageIdx++)
            {
                byte[] page = new byte[this.pageSize];
                for (int i = 0; i < this.pageSize; i++)
                {
                    page[i] = content[pageIdx * pageSize + i];
                }
                this.pages.Add(page);
            }

            this.maxAddress = content.Length - 1;
        }

        #region ISimulationHeap methods

        /// <see cref="ISimulationHeap.WriteByte"/>
        public void WriteByte(int address, byte newVal)
        {
            while (address > this.maxAddress) { ExtendHeap(); }
            this.pages[address / this.pageSize][address % this.pageSize] = newVal;
        }

        /// <see cref="ISimulationHeap.WriteShort"/>
        public void WriteShort(int address, short newVal)
        {
            while (address > this.maxAddress - 1) { ExtendHeap(); }
            byte[] newValBytes = BitConverter.GetBytes(newVal);
            this.pages[(address + 0) / this.pageSize][(address + 0) % this.pageSize] = newValBytes[0];
            this.pages[(address + 1) / this.pageSize][(address + 1) % this.pageSize] = newValBytes[1];
        }

        /// <see cref="ISimulationHeap.WriteInt"/>
        public void WriteInt(int address, int newVal)
        {
            while (address > this.maxAddress - 3) { ExtendHeap(); }
            byte[] newValBytes = BitConverter.GetBytes(newVal);
            this.pages[(address + 0) / this.pageSize][(address + 0) % this.pageSize] = newValBytes[0];
            this.pages[(address + 1) / this.pageSize][(address + 1) % this.pageSize] = newValBytes[1];
            this.pages[(address + 2) / this.pageSize][(address + 2) % this.pageSize] = newValBytes[2];
            this.pages[(address + 3) / this.pageSize][(address + 3) % this.pageSize] = newValBytes[3];
        }

        /// <see cref="ISimulationHeap.WriteLong"/>
        public void WriteLong(int address, long newVal)
        {
            while (address > this.maxAddress - 7) { ExtendHeap(); }
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

        /// <see cref="ISimulationHeap.ComputeHash"/>
        public byte[] ComputeHash()
        {
            uint crc = 0xffffffff;
            for (int pageIdx = 0; pageIdx < this.pages.Count; ++pageIdx)
            {
                byte[] page = this.pages[pageIdx];
                for (int i = 0; i < this.pageSize; ++i)
                {
                    byte index = (byte)(((crc) & 0xff) ^ page[i]);
                    crc = (uint)((crc >> 8) ^ CRC32_TABLE[index]);
                }
            }
            return BitConverter.GetBytes(~crc);
        }

        /// <see cref="ISimulationHeap.Dump"/>
        public byte[] Dump()
        {
            int pageSizeAddress = this.ReadInt(0);
            this.WriteInt(pageSizeAddress, this.pageSize);

            byte[] retArray = new byte[this.pages.Count * this.pageSize];
            int byteIdx = 0;
            for (int pageIdx = 0; pageIdx < this.pages.Count; ++pageIdx)
            {
                byte[] page = this.pages[pageIdx];
                for (int i = 0; i < this.pageSize; ++i)
                {
                    retArray[byteIdx] = page[i];
                    byteIdx++;
                }
            }

            return retArray;
        }

        #endregion ISimulationHeap methods
        
        /// <summary>
        /// Static class-level constructor for computing the helper CRC32-table.
        /// </summary>
        static SimulationHeap()
        {
            CRC32_TABLE = new uint[256];
            uint temp = 0;
            for (uint i = 0; i < CRC32_TABLE.Length; ++i)
            {
                temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                    {
                        temp = (uint)((temp >> 1) ^ CRC32_POLYNOM);
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }
                CRC32_TABLE[i] = temp;
            }
        }

        /// <summary>
        /// Extends the heap with a new page.
        /// </summary>
        private void ExtendHeap()
        {
            this.pages.Add(new byte[this.pageSize]);
            this.maxAddress += this.pageSize;
            if (this.maxAddress >= this.capacity) { throw new SimulationHeapException("Simulation heap overflow!"); }
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
        /// The capacity of the simulation-heap.
        /// </summary>
        private readonly int capacity;

        /// <summary>
        /// The minimum page size.
        /// </summary>
        private const int MIN_PAGESIZE = 32;

        /// <summary>
        /// The maximum page size.
        /// </summary>
        private const int MAX_PAGESIZE = 65536;

        /// <summary>
        /// Helper table for computing the CRC32 hash.
        /// </summary>
        private static readonly uint[] CRC32_TABLE;

        /// <summary>
        /// The generator polynom that is used to compute the CRC32 hash.
        /// </summary>
        private const uint CRC32_POLYNOM = 0xedb88320;
    }
}
