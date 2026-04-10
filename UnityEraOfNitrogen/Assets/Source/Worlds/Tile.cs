// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.EraOfNitrogen.Worlds.Generators;
using Jih.Unity.EraOfNitrogen.Worlds.Runtime;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Jih.Unity.EraOfNitrogen.Worlds
{
    [JsonObject]
    public class Tile
    {
        [JsonProperty] public TileCoord Coord { get; private set; }

        /// <summary>
        /// 땅 아니면 바다.
        /// </summary>
        [JsonProperty] public bool IsLand { get; private set; }
        /// <summary>
        /// 연접한 셀 중 하나라도 바다면 해안선.
        /// </summary>
        [JsonProperty] public bool IsCoastlineLand { get; private set; }
        [JsonProperty] public bool HasRoad { get; private set; }

        [JsonIgnore] public bool IsInitialized { get; private set; }

        [JsonIgnore] public Province? Province { get; private set; }
        [JsonIgnore, MemberNotNullWhen(true, nameof(IsInitialized))] public MapCell? Cell { get; private set; }

        [JsonConstructor]
        private Tile()
        {
        }

        public Tile(GeneratorCell cell)
        {
            Coord = cell.Coord;
            IsLand = cell.IsLand;
            IsCoastlineLand = cell.IsCoastlineLand;
            HasRoad = cell.HasRoad;
        }

        public Tile(TileCoord coord, bool isLand)
        {
            Coord = coord;
            IsLand = isLand;
        }

        public void Initialize(Province? province, MapCell cell)
        {
            if (IsInitialized)
            {
                return;
            }

            if (province is not null && !IsLand)
            {
                throw new InvalidOperationException("타일에 프로빈스가 할당되지만 땅이 아님.");
            }
            Province = province;

            Cell = cell;

            IsInitialized = true;
        }
    }
}
