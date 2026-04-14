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
    public class WorldGrid : HexaMap
    {
        public new WorldCell this[HexaCoord coord] => GetCell(coord) ?? throw new ArgumentOutOfRangeException(nameof(coord));
        public new WorldCell this[HexaIndex index] => GetCell(index) ?? throw new ArgumentOutOfRangeException(nameof(index));

        public WorldGrid(int width, int height, Tile[,] tiles)
            : base(width, height,
                  (map, index, coord) => CreateCell(tiles, map, index, coord),
                  null,
                  null)
        {
        }

        public new WorldCell? GetCell(HexaCoord coord)
        {
            return (WorldCell?)base.GetCell((HexaIndex)coord);
        }
        public new WorldCell? GetCell(HexaIndex index)
        {
            return (WorldCell?)base.GetCell(index.X, index.Y);
        }
        public new WorldCell? GetCell(int x, int y)
        {
            return (WorldCell?)base.GetCell(x, y);
        }

        public new IEnumerable<WorldCell> EnumerateCells()
        {
            return base.EnumerateCells().Cast<WorldCell>();
        }

        static WorldCell CreateCell(Tile[,] tiles, HexaMap map, HexaIndex index, HexaCoord coord)
        {
            return new WorldCell(tiles[index.Y, index.X], map, index, coord);
        }
    }

    public class WorldCell : HexaCell
    {
        public Tile Tile { get; }

        public WorldCell(Tile tile, HexaMap map, HexaIndex index, HexaCoord coord) : base(map, index, coord)
        {
            Tile = tile;
        }

        public new WorldCell? GetNeighbor(HexaNeighborPosition position)
        {
            return (WorldCell?)base.GetNeighbor(position);
        }

        public new IEnumerable<WorldCell> EnumerateNeighbors()
        {
            return base.EnumerateNeighbors().Cast<WorldCell>();
        }
    }
}
