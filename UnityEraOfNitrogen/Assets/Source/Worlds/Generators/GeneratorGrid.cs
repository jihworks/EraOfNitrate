// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.Infrastructure.HexaGrid;
using System.Collections.Generic;
using System.Linq;

namespace Jih.Unity.EraOfNitrogen.Worlds.Generators
{
    public class GeneratorGrid : HexaMap
    {
        public GeneratorGrid(int width, int height)
            : base(width, height,
                  (map, index, coord) => new GeneratorCell(map, index, coord),
                  null,
                  null)
        {
        }

        public new GeneratorCell? GetCell(HexaCoord coord)
        {
            return (GeneratorCell?)base.GetCell((HexaIndex)coord);
        }
        public new GeneratorCell? GetCell(HexaIndex index)
        {
            return (GeneratorCell?)base.GetCell(index.X, index.Y);
        }
        public new GeneratorCell? GetCell(int x, int y)
        {
            return (GeneratorCell?)base.GetCell(x, y);
        }

        public new IEnumerable<GeneratorCell> EnumerateCells()
        {
            return base.EnumerateCells().Cast<GeneratorCell>();
        }
    }
}
