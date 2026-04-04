// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;

namespace Jih.Unity.Infrastructure.HexaGrid
{
    public class HexaVertex
    {
        public HexaMap Map { get; }

        public HexaVertexCoord Coord { get; }

        internal HexaCell?[] CellsInternal { get; } = new HexaCell?[6];
        /// <remarks>
        /// Index: <see cref="HexaVertexPosition"/><br/>
        /// Means contacting direction to the cell from this vertex.<br/>
        /// Can be <c>null</c> if there is no cell in that direction.
        /// </remarks>
        public IReadOnlyList<HexaCell?> Cells => CellsInternal;

        internal HexaEdge?[] EdgesInternal { get; } = new HexaEdge?[6];
        /// <remarks>
        /// Index: <see cref="HexaVertexPosition"/><br/>
        /// Means connecting direction of another vertex on the edge from this vertex.<br/>
        /// Can be <c>null</c> if there is no edge in that direction.
        /// </remarks>
        public IReadOnlyList<HexaEdge?> Edges => EdgesInternal;

        public HexaVertex(HexaMap map, HexaVertexCoord coord)
        {
            Map = map;
            Coord = coord;
        }

        public HexaCell? GetCell(HexaVertexPosition position)
        {
            return CellsInternal[(int)position];
        }


        public HexaEdge? GetEdge(HexaVertexPosition position)
        {
            return EdgesInternal[(int)position];
        }

        public IEnumerable<HexaCell> EnumerateCells()
        {
            foreach (var cell in Cells)
            {
                if (cell is not null)
                {
                    yield return cell;
                }
            }
        }

        public IEnumerable<HexaEdge> EnumerateEdges()
        {
            foreach (var edge in Edges)
            {
                if (edge is not null)
                {
                    yield return edge;
                }
            }
        }
    }
}
