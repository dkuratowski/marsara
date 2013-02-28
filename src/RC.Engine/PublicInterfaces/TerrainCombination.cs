using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common;

namespace RC.Engine.PublicInterfaces
{
    /// <summary>
    /// Enumerates the possible terrain combination of mixed tiles. Mixed tiles can be placed at the
    /// transition of two terrain types. The letters in these values indicates that which part of the
    /// mixed tile has which terrain type of the transition (A or B). The tiles have four parts identified
    /// by the corresponding direction (north, east, south, west). The letters in the values are in that
    /// order.
    /// For example: assume that we have 'dirt' and 'grass' as two terrain types (A and B respectively)
    /// and we are looking for mixed tiles that can be placed at dirt-grass transitions. In this case
    /// TerrainCombination.ABBA indicates a mixed tile that has dirt at north and west and grass at east and
    /// south.
    /// Use TerrainCombination.Simple for simple tiles.
    /// </summary>
    public enum TerrainCombination
    {
        [EnumMapping("Simple")]
        Simple = 0x0, /// Simple tile of a terrain type

        [EnumMapping("AAAB")]
        AAAB = 0x1, /// North-A, East-A, South-A, West-B

        [EnumMapping("AABA")]
        AABA = 0x2, /// North-A, East-A, South-B, West-A

        [EnumMapping("AABB")]
        AABB = 0x3, /// North-A, East-A, South-B, West-B

        [EnumMapping("ABAA")]
        ABAA = 0x4, /// North-A, East-B, South-A, West-A

        [EnumMapping("ABAB")]
        ABAB = 0x5, /// North-A, East-B, South-A, West-B

        [EnumMapping("ABBA")]
        ABBA = 0x6, /// North-A, East-B, South-B, West-A

        [EnumMapping("ABBB")]
        ABBB = 0x7, /// North-A, East-B, South-B, West-B

        [EnumMapping("BAAA")]
        BAAA = 0x8, /// North-B, East-A, South-A, West-A

        [EnumMapping("BAAB")]
        BAAB = 0x9, /// North-B, East-A, South-A, West-B

        [EnumMapping("BABA")]
        BABA = 0xA, /// North-B, East-A, South-B, West-A

        [EnumMapping("BABB")]
        BABB = 0xB, /// North-B, East-A, South-B, West-B

        [EnumMapping("BBAA")]
        BBAA = 0xC, /// North-B, East-B, South-A, West-A

        [EnumMapping("BBAB")]
        BBAB = 0xD, /// North-B, East-B, South-A, West-B

        [EnumMapping("BBBA")]
        BBBA = 0xE, /// North-B, East-B, South-B, West-A
    }
}
