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
        /// <summary>
        /// Get next CW position.
        /// </summary>
        public static HexaNeighborPosition Next(this HexaNeighborPosition position)
        {
            return (HexaNeighborPosition)((int)position).GetNextCircularIndex(6);
        }
        /// <summary>
        /// Get next CCW position.
        /// </summary>
        public static HexaNeighborPosition Prev(this HexaNeighborPosition position)
        {
            return (HexaNeighborPosition)((int)position).GetPrevCircularIndex(6);
        }
        /// <summary>
        /// Get point-symmetry position from center of a cell.
        /// </summary>
        public static HexaNeighborPosition Flip(this HexaNeighborPosition position)
        {
            return (HexaNeighborPosition)(((int)position + 3) % 6);
        }

        public static HexaCoord GetOffset(this HexaDiagonalPosition position)
        {
            return _diagonalOffsets[(int)position];
        }
        /// <inheritdoc cref="Next(HexaNeighborPosition)"/>
        public static HexaDiagonalPosition Next(this HexaDiagonalPosition position)
        {
            return (HexaDiagonalPosition)((int)position).GetNextCircularIndex(6);
        }
        /// <inheritdoc cref="Prev(HexaNeighborPosition)"/>
        public static HexaDiagonalPosition Prev(this HexaDiagonalPosition position)
        {
            return (HexaDiagonalPosition)((int)position).GetPrevCircularIndex(6);
        }
        /// <inheritdoc cref="Flip(HexaNeighborPosition)"/>
        public static HexaDiagonalPosition Flip(this HexaDiagonalPosition position)
        {
            return (HexaDiagonalPosition)(((int)position + 3) % 6);
        }

        public static HexaCoordF GetOffset(this HexaVertexPosition position)
        {
            return _vertexOffsets[(int)position];
        }
        /// <summary>
        /// Get a radius angle degrees from given hexa-position in general cartesian space<i>(+Y up)</i>.
        /// </summary>
        /// <remarks>
        /// Returned <c>0º</c> means <c>D90</c>.<br/>
        /// The returned value is compatible with <see cref="MathEx.RadiusVector(float)"/>.
        /// </remarks>
        public static float GetRadiusDegrees(this HexaVertexPosition position)
        {
            return ((int)position) * 60f - 90f;
        }
        /// <inheritdoc cref="Next(HexaNeighborPosition)"/>
        public static HexaVertexPosition Next(this HexaVertexPosition position)
        {
            return (HexaVertexPosition)((int)position).GetNextCircularIndex(6);
        }
        /// <inheritdoc cref="Prev(HexaNeighborPosition)"/>
        public static HexaVertexPosition Prev(this HexaVertexPosition position)
        {
            return (HexaVertexPosition)((int)position).GetPrevCircularIndex(6);
        }
        /// <inheritdoc cref="Flip(HexaNeighborPosition)"/>
        public static HexaVertexPosition Flip(this HexaVertexPosition position)
        {
            return (HexaVertexPosition)(((int)position + 3) % 6);
        }

        /// <inheritdoc cref="GetRadiusDegrees(HexaVertexPosition)"/>
        public static float GetRadiusDegrees(this HexaEdgePosition position)
        {
            return ((int)position) * 60f - 60f;
        }
        /// <inheritdoc cref="Next(HexaNeighborPosition)"/>
        public static HexaEdgePosition Next(this HexaEdgePosition position)
        {
            return (HexaEdgePosition)((int)position).GetNextCircularIndex(6);
        }
        /// <inheritdoc cref="Prev(HexaNeighborPosition)"/>
        public static HexaEdgePosition Prev(this HexaEdgePosition position)
        {
            return (HexaEdgePosition)((int)position).GetPrevCircularIndex(6);
        }
        /// <inheritdoc cref="Flip(HexaNeighborPosition)"/>
        public static HexaEdgePosition Flip(this HexaEdgePosition position)
        {
            return (HexaEdgePosition)(((int)position + 3) % 6);
        }

        public static HexaEdgePosition ConvertToEdge(this HexaNeighborPosition position)
        {
            return (HexaEdgePosition)position;
        }
        public static HexaNeighborPosition ConvertToNeighbor(this HexaEdgePosition position)
        {
            return (HexaNeighborPosition)position;
        }

        public static HexaVertexPosition ConvertToVertex(this HexaDiagonalPosition position)
        {
            return (HexaVertexPosition)position;
        }
        public static HexaDiagonalPosition ConvertToDiagonal(this HexaVertexPosition position)
        {
            return (HexaDiagonalPosition)position;
        }

        static readonly HexaCoord[] _neighborsOffsets = new HexaCoord[]
        {
            new(+1, -1, 0), new(+1, 0, -1), new(0, +1, -1), new(-1, +1, 0), new(-1, 0, +1), new(0, -1, +1),
        };
        static readonly HexaCoord[] _diagonalOffsets = new HexaCoord[]
        {
            new(+1, -2, +1), new(+2, -1, -1), new(+1, +1, -2), new(-1, +2, -1), new(-2, +1, +1), new(-1, -1, +2),
        };

        static readonly HexaCoordF[] _vertexOffsets = new HexaCoordF[]
        {
            new(+1f / 3f, -2f / 3f, +1f / 3f),
            new(+2f / 3f, -1f / 3f, -1f / 3f),
            new(+1f / 3f, +1f / 3f, -2f / 3f),
            new(-1f / 3f, +2f / 3f, -1f / 3f),
            new(-2f / 3f, +1f / 3f, +1f / 3f),
            new(-1f / 3f, -1f / 3f, +2f / 3f),
        };
    }
}
