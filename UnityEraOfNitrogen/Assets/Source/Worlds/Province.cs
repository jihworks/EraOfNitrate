// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.EraOfNitrogen.Worlds.Generators;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Jih.Unity.EraOfNitrogen.Worlds
{
    [JsonObject]
    public class Province
    {
        [JsonProperty] public Tile CityTile { get; private set; }

        [JsonProperty(nameof(Tiles))] readonly List<Tile> _tiles;
        [JsonIgnore] public IReadOnlyList<Tile> Tiles => _tiles;

        [JsonProperty(nameof(Citizens))] readonly List<Citizen> _citizens;
        [JsonIgnore] public IReadOnlyList<Citizen> Citizens => _citizens;

        [JsonConstructor]
        private Province()
        {
            CityTile = null!;
            _tiles = null!;
            _citizens = null!;
        }

        public Province(GeneratorProvince generatorProvince)
        {
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

            _citizens = new List<Citizen>();
        }
    }
}
