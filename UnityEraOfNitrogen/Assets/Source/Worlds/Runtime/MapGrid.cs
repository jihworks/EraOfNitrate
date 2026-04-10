// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.Infrastructure.HexaGrid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jih.Unity.EraOfNitrogen.Worlds.Runtime
{
    public class MapGrid : HexaMap
    {
        public new MapCell this[HexaCoord coord] => GetCell(coord) ?? throw new ArgumentOutOfRangeException(nameof(coord));
        public new MapCell this[HexaIndex index] => GetCell(index) ?? throw new ArgumentOutOfRangeException(nameof(index));

        public MapGrid(int width, int height, Tile[,] tiles)
            : base(width, height,
                  (map, index, coord) => CreateCell(tiles, map, index, coord),
                  null,
                  null)
        {
        }

        public new MapCell? GetCell(HexaCoord coord)
        {
            return (MapCell?)base.GetCell((HexaIndex)coord);
        }
        public new MapCell? GetCell(HexaIndex index)
        {
            return (MapCell?)base.GetCell(index.X, index.Y);
        }
        public new MapCell? GetCell(int x, int y)
        {
            return (MapCell?)base.GetCell(x, y);
        }

        public new IEnumerable<MapCell> EnumerateCells()
        {
            return base.EnumerateCells().Cast<MapCell>();
        }

        static MapCell CreateCell(Tile[,] tiles, HexaMap map, HexaIndex index, HexaCoord coord)
        {
            return new MapCell(tiles[index.Y, index.X], map, index, coord);
        }
    }

    public class MapCell : HexaCell
    {
        public Tile Tile { get; }

        public MapCell(Tile tile, HexaMap map, HexaIndex index, HexaCoord coord) : base(map, index, coord)
        {
            Tile = tile;
        }

        public new IEnumerable<MapCell> EnumerateNeighbors()
        {
            return base.EnumerateNeighbors().Cast<MapCell>();
        }
    }
}
