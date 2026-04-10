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
    public class GeneratorCell : HexaCell
    {
        /// <summary>
        /// 땅 아니면 바다.
        /// </summary>
        public bool IsLand { get; set; }
        /// <summary>
        /// 연접한 셀 중 하나라도 바다면 해안선.
        /// </summary>
        public bool IsCoastlineLand { get; set; }

        public bool HasRoad { get; set; }

        public GeneratorProvince? Province { get; set; }

        public GeneratorCell(HexaMap map, HexaIndex index, HexaCoord coord) : base(map, index, coord)
        {
        }

        public new IEnumerable<GeneratorCell> EnumerateNeighbors()
        {
            return base.EnumerateNeighbors().Cast<GeneratorCell>();
        }
    }
}
