// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;

namespace Jih.Unity.Infrastructure.TileGrid
{
    public class TileVertex
    {
        public TileMap Map { get; }

        public TileVertexCoord Coord { get; }

        internal TileCell?[] CellsInternal { get; } = new TileCell?[4];
        public IReadOnlyList<TileCell?> Cells => CellsInternal;

        public TileCell? TopLeftCell => CellsInternal[(int)TileDiagonalPosition.TL];
        public TileCell? TopRightCell => CellsInternal[(int)TileDiagonalPosition.TR];
        public TileCell? BottomRightCell => CellsInternal[(int)TileDiagonalPosition.BR];
        public TileCell? BottomLeftCell => CellsInternal[(int)TileDiagonalPosition.BL];

        public TileVertex(TileMap map, TileVertexCoord coord)
        {
            Map = map;
            Coord = coord;
        }

        public IEnumerable<TileCell> EnumerateCells()
        {
            foreach (var cell in CellsInternal)
            {
                if (cell is not null)
                {
                    yield return cell;
                }
            }
        }
    }
}
