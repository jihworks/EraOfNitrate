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

        [JsonProperty(nameof(PortTile))] Tile? _portTile;
        [JsonIgnore] public Tile? PortTile => _portTile;

        [JsonProperty(nameof(LandTiles))] readonly List<Tile> _landTiles = new();
        /// <summary>
        /// <see cref="CityTile"/>, <see cref="PortTile"/> 포함.
        /// </summary>
        [JsonIgnore] public IReadOnlyList<Tile> LandTiles => _landTiles;

        [JsonProperty(nameof(OceanTiles))] readonly List<Tile> _oceanTiles = new();
        [JsonIgnore] public IReadOnlyList<Tile> OceanTiles => _oceanTiles;

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

        public void Bind(MapProvince mapProvince, IReadOnlyDictionary<TileCoord, Tile> tilesMap, bool initialBind)
        {
            _mapProvince = mapProvince;

            if (initialBind)
            {
                _cityTile = tilesMap[mapProvince.CityTile.Coord];

                if (mapProvince.PortTile is not null)
                {
                    _portTile = tilesMap[mapProvince.PortTile.Coord];
                }

                for (int i = 0; i < mapProvince.LandTiles.Count; i++)
                {
                    _landTiles.Add(tilesMap[mapProvince.LandTiles[i].Coord]);
                }

                for (int i = 0; i < mapProvince.OceanTiles.Count; i++)
                {
                    _oceanTiles.Add(tilesMap[mapProvince.OceanTiles[i].Coord]);
                }
            }
            // 타일들에 대한 바인딩은 이미 월드에서 진행함.
        }

        public void Initialize(World world, IReadOnlyDictionary<uint, Province> provinceMap, WorldGrid worldGrid)
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

            foreach (var tile in _landTiles)
            {
                tile.Initialize(world, this, worldGrid[tile.Coord]);
            }
            foreach (var tile in _oceanTiles)
            {
                tile.Initialize(world, this, worldGrid[tile.Coord]);
            }

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
