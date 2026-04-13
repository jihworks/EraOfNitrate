// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.EraOfNitrogen.Worlds.Generators;
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
        /// <summary>
        /// 동일 월드 내 프로빈스들 사이에서 유일한 값.
        /// </summary>
        [JsonProperty] public uint Id { get; private set; }

        [JsonProperty] public Biome Biome { get; private set; }

        [JsonProperty] public Tile CityTile { get; private set; }

        [JsonProperty(nameof(Tiles))] readonly List<Tile> _tiles;
        /// <summary>
        /// <see cref="CityTile"/> 포함.
        /// </summary>
        [JsonIgnore] public IReadOnlyList<Tile> Tiles => _tiles;

        [JsonProperty(nameof(AdjacentProvinceIds))] readonly List<uint> _adjacentProvinceIds;
        [JsonIgnore] public IReadOnlyList<uint> AdjacentProvinceIds => _adjacentProvinceIds;

        [JsonProperty(nameof(Citizens))] readonly List<Citizen> _citizens;
        [JsonIgnore] public IReadOnlyList<Citizen> Citizens => _citizens;

        [JsonIgnore, MemberNotNullWhen(true,
            nameof(_world),
            nameof(_adjacentProvinces))]
        public bool IsInitialized { get; private set; }

        [JsonIgnore] World? _world;
        [JsonIgnore] public World World => _world.ThrowIfNull(nameof(World));

        [JsonIgnore] List<Province>? _adjacentProvinces;
        [JsonIgnore] public IReadOnlyList<Province> AdjacentProvinces => _adjacentProvinces.ThrowIfNull(nameof(AdjacentProvinces));

        [JsonConstructor]
        private Province()
        {
            CityTile = null!;
            _tiles = null!;
            _adjacentProvinceIds = null!;
            _citizens = null!;
        }

        public Province(GeneratorProvince generatorProvince)
        {
            Id = generatorProvince.Id;

            Biome = generatorProvince.Biome;

            Tile? cityTile = null;

            _tiles = new(generatorProvince.Cells.Count);
            foreach (var cell in generatorProvince.Cells)
            {
                Tile tile = new(cell);
                _tiles.Add(tile);

                if (cell == generatorProvince.CityCell)
                {
                    cityTile = tile;
                }
            }

            CityTile = cityTile ?? throw new InvalidOperationException("도시 셀이 프로빈스 셀 리스트에 없음.");

            List<GeneratorProvince> adjacentProvinces = generatorProvince.AdjacentProvinces;
            _adjacentProvinceIds = new(adjacentProvinces.Count);
            _adjacentProvinceIds.AddRange(adjacentProvinces.Select(p => p.Id));

            _citizens = new List<Citizen>();
        }

        public void Initialize(World world, IReadOnlyDictionary<uint, Province> provinceMap)
        {
            if (IsInitialized)
            {
                return;
            }

            _world = world;

            _adjacentProvinces = new List<Province>(_adjacentProvinceIds.Count);
            _adjacentProvinces.AddRange(_adjacentProvinceIds.Select(id => provinceMap[id]));

            IsInitialized = true;
        }
    }
}
