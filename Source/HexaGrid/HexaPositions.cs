// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.Unity.Infrastructure.HexaGrid
{
    public enum HexaNeighborPosition
    {
        D30 = 0, D90 = 1, D150 = 2, D210 = 3, D270 = 4, D330 = 5,
        C1 = D30, C3 = D90, C5 = D150, C7 = D210, C9 = D270, C11 = D330,
    }

    public enum HexaDiagonalPosition
    {
        D0 = 0, D60 = 1, D120 = 2, D180 = 3, D240 = 4, D300 = 5, D360 = D0,
        C12 = D0, C2 = D60, C4 = D120, C6 = D180, C8 = D240, C10 = D300,
        N = D0, NE = D60, SE = D120, S = D180, SW = D240, NW = D300,
    }

    public enum HexaVertexPosition
    {
        D0 = 0, D60 = 1, D120 = 2, D180 = 3, D240 = 4, D300 = 5, D360 = D0,
        C12 = D0, C2 = D60, C4 = D120, C6 = D180, C8 = D240, C10 = D300,
        N = D0, NE = D60, SE = D120, S = D180, SW = D240, NW = D300,
    }

    public enum HexaEdgePosition
    {
        D30 = 0, D90 = 1, D150 = 2, D210 = 3, D270 = 4, D330 = 5,
        C1 = D30, C3 = D90, C5 = D150, C7 = D210, C9 = D270, C11 = D330,
    }

    public static class HexaPositionEx
    {
        public static HexaCoord GetOffset(this HexaNeighborPosition position)
        {
            return _neighborsOffsets[(int)position];
        }

        public static HexaCoord GetOffset(this HexaDiagonalPosition position)
        {
            return _diagonalOffsets[(int)position];
        }

        public static float GetRadiusDegrees(this HexaVertexPosition position)
        {
            return ((int)position) * 60f - 90f;
        }

        public static float GetRadiusDegrees(this HexaEdgePosition position)
        {
            return ((int)position) * 60f - 60f;
        }

        static readonly HexaCoord[] _neighborsOffsets = new HexaCoord[]
        {
            new(+1, -1, 0), new(+1, 0, -1), new(0, +1, -1), new(-1, +1, 0), new(-1, 0, +1), new(0, -1, +1),
        };
        static readonly HexaCoord[] _diagonalOffsets = new HexaCoord[]
        {
            new(+1, -2, +1), new(+2, -1, -1), new(+1, +1, -2), new(-1, +2, -1), new(-2, +1, +1), new(-1, -1, +2),
        };
    }
}
