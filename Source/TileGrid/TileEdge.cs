// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;

namespace Jih.Unity.Infrastructure.TileGrid
{
    public class TileEdge
    {
        public TileMap Map => Vertex0.Map;

        public TileEdgeCoord Coord { get; }

        public TileVertex Vertex0 { get; }
        public TileVertex Vertex1 { get; }

        /// <summary>
        /// Cell on the right side of the edge, when looking from <see cref="Vertex0"/> to <see cref="Vertex1"/>.
        /// </summary>
        /// <remarks>
        /// This value is always NOT <c>null</c>.<br/>
        /// If see the edge from center of this cell, the edge direction(from 0 to 1) is always CW.
        /// </remarks>
        public TileCell RightCell { get; }
        /// <summary>
        /// Cell on the left side of the edge, when looking from <see cref="Vertex0"/> to <see cref="Vertex1"/>.
        /// </summary>
        /// <remarks>
        /// If see the edge from center of this cell, the edge direction(from 0 to 1) is always CCW.
        /// </remarks>
        public TileCell? LeftCell { get; internal set; }

        public TileEdge(TileEdgeCoord coord, TileVertex vertex0, TileVertex vertex1, TileCell rightCell)
        {
            Coord = coord;
            Vertex0 = vertex0;
            Vertex1 = vertex1;
            RightCell = rightCell;
        }

        public void GetCwOrder(TileCell cell, out TileVertex from, out TileVertex to)
        {
            if (!TryGetCwOrder(cell, out from, out to))
            {
                throw new ArgumentException($"The cell {cell.Coord} is not adjacent to edge {Coord}.", nameof(cell));
            }
        }
        public bool TryGetCwOrder(TileCell cell, out TileVertex from, out TileVertex to)
        {
            if (RightCell == cell)
            {
                from = Vertex0;
                to = Vertex1;
                return true;
            }
            if (LeftCell == cell)
            {
                from = Vertex1;
                to = Vertex0;
                return true;
            }

            from = Vertex0;
            to = Vertex1;
            return false;
        }

        public IEnumerable<TileCell> EnumerateCells()
        {
            yield return RightCell;

            if (LeftCell is not null)
            {
                yield return LeftCell;
            }
        }
    }
}
