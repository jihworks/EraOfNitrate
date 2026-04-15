// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.EraOfNitrogen.Worlds.Generators;
using Jih.Unity.Infrastructure.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jih.Unity.EraOfNitrogen.Worlds
{
    [JsonObject]
    public class Map
    {
        [JsonProperty] public int Width { get; private set; }
        [JsonProperty] public int Height { get; private set; }
        [JsonProperty] public int RandomSeed { get; private set; }

        [JsonProperty(nameof(Provinces))] readonly List<MapProvince> _provinces;
        [JsonIgnore] public IReadOnlyList<MapProvince> Provinces => _provinces;

        [JsonProperty(nameof(OceanTiles))] readonly List<MapTile> _oceanTiles;
        [JsonIgnore] public IReadOnlyList<MapTile> OceanTiles => _oceanTiles;

        [JsonConstructor]
        private Map()
        {
            _provinces = null!;
            _oceanTiles = null!;
        }

        public Map(GeneratorGrid generatorGrid, int mapSeed, IReadOnlyList<GeneratorProvince> generatorProvinces, IReadOnlyList<GeneratorCell> generatorOceanCells)
        {
            Width = generatorGrid.Width;
            Height = generatorGrid.Height;
            RandomSeed = mapSeed;

            _provinces = new List<MapProvince>(generatorProvinces.Count);
            foreach (var generatorProvince in generatorProvinces)
            {
                MapProvince province = new(generatorProvince);
                _provinces.Add(province);
            }

            _oceanTiles = new List<MapTile>(generatorOceanCells.Count);
            foreach (var generatorCell in generatorOceanCells)
            {
                MapTile oceanTile = new(generatorCell);
                _oceanTiles.Add(oceanTile);
            }
        }
    }

    [JsonObject]
    public class MapProvince
    {
        [JsonProperty] public uint Id { get; private set; }
        [JsonProperty] public Biome Biome { get; private set; }
        [JsonProperty] public MapTile CityTile { get; private set; }

        [JsonProperty(nameof(Tiles))] readonly List<MapTile> _tiles;
        /// <summary>
        /// <see cref="CityTile"/> 포함.
        /// </summary>
        [JsonIgnore] public IReadOnlyList<MapTile> Tiles => _tiles;

        [JsonProperty(nameof(AdjacentProvinceIds))] readonly List<uint> _adjacentProvinceIds;
        [JsonIgnore] public IReadOnlyList<uint> AdjacentProvinceIds => _adjacentProvinceIds;

        [JsonProperty(nameof(ConnectedProvinceIds))] readonly List<uint> _connectedProvinceIds;
        [JsonIgnore] public IReadOnlyList<uint> ConnectedProvinceIds => _connectedProvinceIds;

        [JsonConstructor]
        private MapProvince()
        {
            CityTile = null!;
            _tiles = null!;
            _adjacentProvinceIds = null!;
            _connectedProvinceIds = null!;
        }

        public MapProvince(GeneratorProvince generatorProvince)
        {
            Id = generatorProvince.Id;

            Biome = generatorProvince.Biome;

            MapTile? cityTile = null;

            _tiles = new(generatorProvince.Cells.Count);
            foreach (var cell in generatorProvince.Cells)
            {
                MapTile tile = new(cell);
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

            List<GeneratorProvince> connectedProvinces = generatorProvince.ConnectedProvinces;
            _connectedProvinceIds = new(connectedProvinces.Count);
            _connectedProvinceIds.AddRange(connectedProvinces.Select(p => p.Id));
        }
    }

    [JsonObject]
    public class MapTile
    {
        [JsonProperty] public TileCoord Coord { get; private set; }

        [JsonProperty] public bool IsLand { get; private set; }
        [JsonProperty] public bool IsCoastlineLand { get; private set; }
        [JsonProperty] public bool IsNearOcean { get; private set; }
        [JsonProperty] public bool HasRoad { get; private set; }

        [JsonProperty(nameof(Doodads))] readonly List<MapDoodad> _doodads;
        [JsonIgnore] public IReadOnlyList<MapDoodad> Doodads => _doodads;

        [JsonConstructor]
        private MapTile()
        {
            _doodads = null!;
        }

        public MapTile(GeneratorCell cell)
        {
            Coord = cell.Coord;
            IsLand = cell.IsLand;
            IsCoastlineLand = cell.IsCoastlineLand;
            IsNearOcean = cell.IsNearOcean;
            HasRoad = cell.HasRoad;

            _doodads = new List<MapDoodad>(cell.Doodads.Count);
            foreach (var doodad in cell.Doodads)
            {
                _doodads.Add(new MapDoodad(doodad));
            }
        }
    }

    [JsonObject]
    public class MapDoodad
    {
        [JsonProperty] public DoodadType Type { get; private set; }
        [JsonProperty] public int Variant { get; private set; }

        [JsonProperty] public JsonSaveVector3 UnityLocation { get; private set; }
        [JsonProperty] public float UnityRotationY { get; private set; }
        [JsonProperty] public float UnityScale { get; private set; }

        [JsonConstructor]
        private MapDoodad()
        {
        }

        public MapDoodad(GeneratorDoodad doodad)
        {
            Type = doodad.Type;
            Variant = doodad.Variant;
            UnityLocation = doodad.UnityLocation;
            UnityRotationY = doodad.UnityRotationY;
            UnityScale = doodad.UnityScale;
        }
    }
}
