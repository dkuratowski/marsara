using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Engine.Simulator.InternalInterfaces;
using System.IO;
using RC.Engine.Simulator.PublicInterfaces;
using RC.Common;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Implementation of the simulation-heap where every simulation data is stored.
    /// </summary>
    /// TODO: this implementation might have a possible performance issue!
    class Heap : IHeap
    {
        /// <summary>
        /// Constructs an empty Heap with the given page size and capacity.
        /// </summary>
        /// <param name="pageSize">The size of the pages on the simulation-heap.</param>
        /// <param name="capacity">The capacity of the simulation-heap.</param>
        public Heap(int pageSize, int capacity)
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
        /// Constructs a Heap with the given content and capacity.
        /// </summary>
        /// <param name="content">The content of the simulation heap.</param>
        /// <param name="capacity">The capacity of the simulation-heap.</param>
        public Heap(byte[] content, int capacity)
        {
            if (content == null) { throw new ArgumentNullException("content"); }

            int pageSizeAddress = BitConverter.ToInt32(content, 0);

            int pageSize = BitConverter.ToInt32(content, pageSizeAddress);
            if (pageSize < MIN_PAGESIZE || pageSize > MAX_PAGESIZE) { throw new HeapException("Page-size is out of bounds!"); }
            if (content.Length % pageSize != 0) { throw new HeapException("The page-size must be a divisor of the size of the content array!"); }
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

        #region IHeap methods

        /// <see cref="IHeap.WriteByte"/>
        public void WriteByte(int address, byte newVal)
        {
            while (address > this.maxAddress) { ExtendHeap(); }
            this.pages[address / this.pageSize][address % this.pageSize] = newVal;
        }

        /// <see cref="IHeap.WriteShort"/>
        public void WriteShort(int address, short newVal)
        {
            while (address > this.maxAddress - 1) { ExtendHeap(); }
            byte[] newValBytes = BitConverter.GetBytes(newVal);
            this.pages[(address + 0) / this.pageSize][(address + 0) % this.pageSize] = newValBytes[0];
            this.pages[(address + 1) / this.pageSize][(address + 1) % this.pageSize] = newValBytes[1];
        }

        /// <see cref="IHeap.WriteInt"/>
        public void WriteInt(int address, int newVal)
        {
            while (address > this.maxAddress - 3) { ExtendHeap(); }
            byte[] newValBytes = BitConverter.GetBytes(newVal);
            this.pages[(address + 0) / this.pageSize][(address + 0) % this.pageSize] = newValBytes[0];
            this.pages[(address + 1) / this.pageSize][(address + 1) % this.pageSize] = newValBytes[1];
            this.pages[(address + 2) / this.pageSize][(address + 2) % this.pageSize] = newValBytes[2];
            this.pages[(address + 3) / this.pageSize][(address + 3) % this.pageSize] = newValBytes[3];
        }

        /// <see cref="IHeap.WriteLong"/>
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

        /// <see cref="IHeap.ReadByte"/>
        public byte ReadByte(int address)
        {
            return this.pages[address / this.pageSize][address % this.pageSize];
        }

        /// <see cref="IHeap.ReadShort"/>
        public short ReadShort(int address)
        {
            byte[] valueBytes = new byte[2];
            valueBytes[0] = this.pages[(address + 0) / this.pageSize][(address + 0) % this.pageSize];
            valueBytes[1] = this.pages[(address + 1) / this.pageSize][(address + 1) % this.pageSize];
            return BitConverter.ToInt16(valueBytes, 0);
        }

        /// <see cref="IHeap.ReadInt"/>
        public int ReadInt(int address)
        {
            byte[] valueBytes = new byte[4];
            valueBytes[0] = this.pages[(address + 0) / this.pageSize][(address + 0) % this.pageSize];
            valueBytes[1] = this.pages[(address + 1) / this.pageSize][(address + 1) % this.pageSize];
            valueBytes[2] = this.pages[(address + 2) / this.pageSize][(address + 2) % this.pageSize];
            valueBytes[3] = this.pages[(address + 3) / this.pageSize][(address + 3) % this.pageSize];
            return BitConverter.ToInt32(valueBytes, 0);
        }

        /// <see cref="IHeap.ReadLong"/>
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

        /// <see cref="IHeap.ComputeHash"/>
        public byte[] ComputeHash() { return CRC32.ComputeHash(this.GetByteSquence()); }

        /// <see cref="IHeap.Dump"/>
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

        #endregion IHeap methods

        /// <summary>
        /// Extends the heap with a new page.
        /// </summary>
        private void ExtendHeap()
        {
            this.pages.Add(new byte[this.pageSize]);
            this.maxAddress += this.pageSize;
            if (this.maxAddress >= this.capacity) { throw new HeapException("Simulation heap overflow!"); }
        }

        /// <summary>
        /// Gets the byte sequence of the heap as an enumerable collection.
        /// </summary>
        /// <returns>The enumerable collection that contains the bytes of this heap.</returns>
        private IEnumerable<byte> GetByteSquence()
        {
            for (int pageIdx = 0; pageIdx < this.pages.Count; ++pageIdx)
            {
                byte[] page = this.pages[pageIdx];
                for (int i = 0; i < this.pageSize; ++i)
                {
                    yield return page[i];
                }
            }
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
    }
}
