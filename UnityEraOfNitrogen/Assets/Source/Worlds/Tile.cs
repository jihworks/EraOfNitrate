// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.EraOfNitrogen.Worlds.Generators;
using Jih.Unity.EraOfNitrogen.Worlds.Runtime;
using Jih.Unity.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        /// <summary>
        /// 연접한 셀 중 하나라도 땅이면 근해.
        /// </summary>
        [JsonProperty] public bool IsNearOcean { get; private set; }
        /// <summary>
        /// 초기 생성된 도로가 존재하는지 여부.
        /// </summary>
        [JsonProperty] public bool HasRoad { get; private set; }

        [JsonProperty(nameof(Doodads))] readonly List<Doodad> _doodads;
        [JsonIgnore] public IReadOnlyList<Doodad> Doodads => _doodads;

        [JsonIgnore, MemberNotNullWhen(true,
            nameof(_world),
            nameof(_cell))]
        public bool IsInitialized { get; private set; }

        [JsonIgnore] World? _world;
        [JsonIgnore] public World World => _world.ThrowIfNull(nameof(World));
        /// <summary>
        /// 바다인 경우 <c>null</c>.
        /// </summary>
        [JsonIgnore] public Province? Province { get; private set; }

        [JsonIgnore] MapCell? _cell;
        [JsonIgnore] public MapCell Cell => _cell.ThrowIfNull(nameof(Cell));

        [JsonIgnore] RoadElement? _roadElement;
        [JsonIgnore] public RoadElement? RoadElement => _roadElement;

        [JsonConstructor]
        private Tile()
        {
            _doodads = null!;
        }

        public Tile(GeneratorCell cell)
        {
            Coord = cell.Coord;
            IsLand = cell.IsLand;
            IsCoastlineLand = cell.IsCoastlineLand;
            IsNearOcean = cell.IsNearOcean;
            HasRoad = cell.HasRoad;

            _doodads = new List<Doodad>(cell.Doodads.Count);
            foreach (var doodad in cell.Doodads)
            {
                _doodads.Add(new Doodad(doodad));
            }
        }

        public void Initialize(World world, Province? province, MapCell cell)
        {
            if (IsInitialized)
            {
                return;
            }

            _world = world;

            if (province is not null && !IsLand)
            {
                throw new InvalidOperationException("타일에 프로빈스가 할당되지만 땅이 아님.");
            }
            Province = province;

            _cell = cell;

            foreach (var doodad in Doodads)
            {
                doodad.Initialize(this);
            }

            IsInitialized = true;
        }

        public void Spawned(RoadElement roadElement)
        {
            if (_roadElement is not null)
            {
                throw new InvalidOperationException("이미 도로가 스폰됐지만 다시 스폰됨.");
            }

            _roadElement = roadElement;
        }
    }
}
