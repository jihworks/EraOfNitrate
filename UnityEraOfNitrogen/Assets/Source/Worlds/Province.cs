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
using System.Linq;

namespace Jih.Unity.EraOfNitrogen.Worlds
{
    [JsonObject]
    public class Province
    {
        [JsonIgnore] MapProvince? _mapProvince;
        [JsonIgnore] MapProvince MapProvince => _mapProvince.ThrowIfNull(nameof(MapProvince));

        /// <summary>
        /// 동일 월드 내 프로빈스들 사이에서 유일한 값.
        /// </summary>
        [JsonIgnore] public uint Id => MapProvince.Id;

        [JsonIgnore] public Biome Biome => MapProvince.Biome;

        [JsonProperty(nameof(CityTile))] Tile? _cityTile;
        [JsonIgnore] public Tile CityTile => _cityTile.ThrowIfNull(nameof(CityTile));

        [JsonProperty(nameof(Tiles))] readonly List<Tile> _tiles = new();
        /// <summary>
        /// <see cref="CityTile"/> 포함.
        /// </summary>
        [JsonIgnore] public IReadOnlyList<Tile> Tiles => _tiles;

        [JsonProperty(nameof(Citizens))] readonly List<Citizen> _citizens = new();
        [JsonIgnore] public IReadOnlyList<Citizen> Citizens => _citizens;

        [JsonIgnore, MemberNotNullWhen(true,
            nameof(_world),
            nameof(_adjacentProvinces),
            nameof(_connectedProvinces))]
        public bool IsInitialized { get; private set; }

        [JsonIgnore] World? _world;
        [JsonIgnore] public World World => _world.ThrowIfNull(nameof(World));

        [JsonIgnore] List<Province>? _adjacentProvinces;
        [JsonIgnore] public IReadOnlyList<Province> AdjacentProvinces => _adjacentProvinces.ThrowIfNull(nameof(AdjacentProvinces));

        [JsonIgnore] List<Province>? _connectedProvinces;
        [JsonIgnore] public IReadOnlyList<Province> ConnectedProvices => _connectedProvinces.ThrowIfNull(nameof(ConnectedProvices));

        [JsonIgnore] readonly List<DoodadCluster> _doodadClusters = new();
        [JsonIgnore] public IReadOnlyList<DoodadCluster> DoodadClusters => _doodadClusters;

        [JsonConstructor]
        public Province()
        {
        }

        public void Bind(MapProvince mapProvince)
        {
            _mapProvince = mapProvince;

            Tile? cityTile = null;

            if (_tiles.Count <= 0)
            {
                for (int i = 0; i < mapProvince.Tiles.Count; i++)
                {
                    _tiles.Add(new Tile());
                }
            }
            for (int i = 0; i < mapProvince.Tiles.Count; i++)
            {
                MapTile mapTile = mapProvince.Tiles[i];
                Tile tile = _tiles[i];
                
                tile.Bind(mapTile);

                if (mapTile == mapProvince.CityTile)
                {
                    cityTile = tile;
                }
            }

            _cityTile = cityTile ?? throw new InvalidOperationException("도시 셀이 프로빈스 셀 리스트에 없음.");
        }

        public void Initialize(World world, IReadOnlyDictionary<uint, Province> provinceMap)
        {
            if (IsInitialized)
            {
                return;
            }

            _world = world;

            _adjacentProvinces = new List<Province>(MapProvince.AdjacentProvinceIds.Count);
            _adjacentProvinces.AddRange(MapProvince.AdjacentProvinceIds.Select(id => provinceMap[id]));

            _connectedProvinces = new List<Province>(MapProvince.ConnectedProvinceIds.Count);
            _connectedProvinces.AddRange(MapProvince.ConnectedProvinceIds.Select(id => provinceMap[id]));

            IsInitialized = true;
        }

        public void Spwaned(DoodadCluster doodadCluster)
        {
            if (_doodadClusters.Contains(doodadCluster))
            {
                throw new InvalidOperationException("두대드 클러스터가 이미 스폰됐지만 다시 스폰됨.");
            }

            _doodadClusters.Add(doodadCluster);
        }
    }
}
