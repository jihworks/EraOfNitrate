// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;

namespace Jih.Unity.Infrastructure.HexaGrid
{
    public class HexaEdge
    {
        public HexaMap Map => Vertex0.Map;

        public HexaVertex Vertex0 { get; }
        public HexaVertex Vertex1 { get; }

        public HexaEdgeCoord Coord { get; }

        /// <summary>
        /// Cell on the right side of the edge, when looking from <see cref="Vertex0"/> to <see cref="Vertex1"/>.
        /// </summary>
        /// <remarks>
        /// This value is always NOT <c>null</c>.<br/>
        /// If see the edge from center of this cell, the edge direction(from 0 to 1) is always CW.
        /// </remarks>
        public HexaCell RightCell { get; }
        /// <summary>
        /// Cell on the left side of the edge, when looking from <see cref="Vertex0"/> to <see cref="Vertex1"/>.
        /// </summary>
        /// <remarks>
        /// If see the edge from center of this cell, the edge direction(from 0 to 1) is always CCW.<br/>
        /// Can be <c>null</c> if there is no cell on the left side of the edge such as border edge of the map.
        /// </remarks>
        public HexaCell? LeftCell { get; internal set; }

        public HexaEdge(HexaVertex vertex0, HexaVertex vertex1, HexaEdgeCoord coord, HexaCell rightCell)
        {
            Vertex0 = vertex0;
            Vertex1 = vertex1;
            Coord = coord;
            RightCell = rightCell;
        }

        public bool Is0(HexaVertex vertex)
        {
            return Vertex0 == vertex;
        }
        public bool Is1(HexaVertex vertex)
        {
            return Vertex1 == vertex;
        }

        public void GetCwOrder(HexaCell cell, out HexaVertex vertex0, out HexaVertex vertex1)
        {
            if (!TryGetCwOrder(cell, out vertex0, out vertex1))
            {
                throw new ArgumentException($"The cell {cell.Coord} is not adjacent to edge {Coord}.", nameof(cell));
            }
        }

        public bool TryGetCwOrder(HexaCell cell, out HexaVertex vertex0, out HexaVertex vertex1)
        {
            if (RightCell == cell)
            {
                vertex0 = Vertex0;
                vertex1 = Vertex1;
                return true;
            }
            if (LeftCell == cell)
            {
                vertex0 = Vertex1;
                vertex1 = Vertex0;
                return true;
            }

            vertex0 = Vertex0;
            vertex1 = Vertex1;
            return false;
        }

        public IEnumerable<HexaCell> EnumerateCells()
        {
            yield return RightCell;

            if (LeftCell is not null)
            {
                yield return LeftCell;
            }
        }
    }
}
