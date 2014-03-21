using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Common
{
    /// <summary>
    /// Static helper class for calculating CRC-32 checksums.
    /// </summary>
    public static class CRC32
    {
        /// <summary>
        /// Computes the hash value of the given byte sequence.
        /// </summary>
        /// <param name="byteSequence">The input byte sequence.</param>
        /// <returns>The byte array that contains the hash.</returns>
        public static byte[] ComputeHash(IEnumerable<byte> byteSequence)
        {
            uint crc = 0xffffffff;
            foreach (byte b in byteSequence)
            {
                byte index = (byte)(((crc) & 0xff) ^ b);
                crc = (uint)((crc >> 8) ^ CRC32_TABLE[index]);
            }
            return BitConverter.GetBytes(~crc);
        }
                
        /// <summary>
        /// Static class-level constructor for computing the helper CRC32-table.
        /// </summary>
        static CRC32()
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
        /// Helper table for computing the CRC32 hash.
        /// </summary>
        private static readonly uint[] CRC32_TABLE;

        /// <summary>
        /// The generator polynom that is used to compute the CRC32 hash.
        /// </summary>
        private const uint CRC32_POLYNOM = 0xedb88320;
    }
}
