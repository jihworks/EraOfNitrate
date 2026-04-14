// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

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
        [JsonIgnore] MapTile? _mapTile;
        [JsonIgnore] MapTile MapTile => _mapTile.ThrowIfNull(nameof(MapTile));

        [JsonIgnore] public TileCoord Coord => MapTile.Coord;

        /// <summary>
        /// 땅 아니면 바다.
        /// </summary>
        [JsonIgnore] public bool IsLand => MapTile.IsLand;
        /// <summary>
        /// 연접한 셀 중 하나라도 바다면 해안선.
        /// </summary>
        [JsonIgnore] public bool IsCoastlineLand => MapTile.IsCoastlineLand;
        /// <summary>
        /// 연접한 셀 중 하나라도 땅이면 근해.
        /// </summary>
        [JsonIgnore] public bool IsNearOcean => MapTile.IsNearOcean;
        /// <summary>
        /// 초기 생성된 도로가 존재하는지 여부.
        /// </summary>
        [JsonIgnore] public bool HasRoad => MapTile.HasRoad;

        [JsonProperty(nameof(Doodads))] readonly List<Doodad> _doodads = new();
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

        [JsonIgnore] WorldCell? _cell;
        [JsonIgnore] public WorldCell Cell => _cell.ThrowIfNull(nameof(Cell));

        [JsonIgnore] RoadElement? _roadElement;
        [JsonIgnore] public RoadElement? RoadElement => _roadElement;

        [JsonConstructor]
        public Tile()
        {
        }

        public void Bind(MapTile mapTile)
        {
            _mapTile = mapTile;

            if (_doodads.Count <= 0)
            {
                for (int i = 0; i < mapTile.Doodads.Count; i++)
                {
                    _doodads.Add(new Doodad());
                }
            }
            for (int i = 0; i < mapTile.Doodads.Count; i++)
            {
                _doodads[i].Bind(mapTile.Doodads[i]);
            }
        }

        public void Initialize(World world, Province? province, WorldCell cell)
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
