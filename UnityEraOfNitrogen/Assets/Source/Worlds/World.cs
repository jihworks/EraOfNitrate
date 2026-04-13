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
using Jih.Unity.Infrastructure.Collisions.Common3D;
using Jih.Unity.Infrastructure.HexaGrid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Jih.Unity.EraOfNitrogen.Worlds
{
    [JsonObject]
    public class World
    {
        [JsonProperty] public int Width { get; private set; }
        [JsonProperty] public int Height { get; private set; }

        [JsonProperty] public int RandomSeed { get; private set; }
        [JsonProperty] public long RandomPosition { get; private set; }

        [JsonProperty(nameof(Provinces))] readonly List<Province> _provinces;
        [JsonIgnore] public IReadOnlyList<Province> Provinces => _provinces;

        [JsonProperty(nameof(OceanTiles))] readonly List<Tile> _oceanTiles;
        [JsonIgnore] public IReadOnlyList<Tile>? OceanTiles => _oceanTiles;

        [JsonIgnore, MemberNotNullWhen(true,
            nameof(_randomStream),
            nameof(_mapGrid))]
        public bool IsInitialized { get; private set; }

        [JsonIgnore] RandomStream? _randomStream;
        [JsonIgnore] public RandomStream RandomStream => _randomStream.ThrowIfNull(nameof(RandomStream));

        [JsonIgnore] MapGrid? _mapGrid;
        [JsonIgnore] public MapGrid MapGrid => _mapGrid.ThrowIfNull(nameof(MapGrid));

        [JsonIgnore] readonly CollisionWorld _collisionWorld = new(cellSize: 1f);

        [JsonConstructor]
        private World()
        {
            _provinces = null!;
            _oceanTiles = null!;
        }

        public World(GeneratorGrid generatorGrid, int mapSeed, IReadOnlyList<GeneratorProvince> generatorProvinces, IReadOnlyList<GeneratorCell> generatorOceanCells)
        {
            Width = generatorGrid.Width;
            Height = generatorGrid.Height;
            RandomSeed = mapSeed;
            RandomPosition = 0;

            _provinces = new List<Province>(generatorProvinces.Count);
            foreach (var generatorProvince in generatorProvinces)
            {
                Province province = new(generatorProvince);
                _provinces.Add(province);
            }

            _oceanTiles = new List<Tile>(generatorOceanCells.Count);
            foreach (var generatorCell in generatorOceanCells)
            {
                Tile oceanTile = new(generatorCell);
                _oceanTiles.Add(oceanTile);
            }
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            _randomStream = new RandomStream()
            {
                Position = RandomPosition,
            };

            Dictionary<uint, Province> provinceMap = new(_provinces.Count);
            foreach (var province in _provinces)
            {
                if (!provinceMap.TryAdd(province.Id, province))
                {
                    throw new InvalidOperationException($"프로빈스 ID {province.Id} 가 중복됨.");
                }
            }

            Tile[,] tiles = new Tile[Height, Width];
            foreach (var province in _provinces)
            {
                foreach (var provinceTile in province.Tiles)
                {
                    HexaCoord coord = provinceTile.Coord;
                    HexaIndex index = (HexaIndex)coord;

                    ref Tile tile = ref tiles[index.Y, index.X];
                    if (tile is not null)
                    {
                        throw new InvalidOperationException($"{coord}에서 프로빈스 타일이 중복됨.");
                    }

                    tile = provinceTile;
                }
            }
            foreach (var oceanTile in _oceanTiles)
            {
                HexaCoord coord = oceanTile.Coord;
                HexaIndex index = (HexaIndex)coord;

                ref Tile tile = ref tiles[index.Y, index.X];
                if (tile is not null)
                {
                    throw new InvalidOperationException($"{coord}에서 바다 타일이 중복됨.");
                }

                tile = oceanTile;
            }

            _mapGrid = new MapGrid(Width, Height, tiles);

            foreach (var province in _provinces)
            {
                foreach (var provinceTile in province.Tiles)
                {
                    provinceTile.Initialize(this, province, _mapGrid[(HexaIndex)(HexaCoord)provinceTile.Coord]);
                }

                province.Initialize(this, provinceMap);
            }
            foreach (var oceanTile in _oceanTiles)
            {
                oceanTile.Initialize(this, null, _mapGrid[(HexaIndex)(HexaCoord)oceanTile.Coord]);
            }

            IsInitialized = true;
        }

        [OnSerializing]
        void OnSerializingMethod(StreamingContext context)
        {
            RandomPosition = RandomStream?.Position ?? 0L;
        }
    }
}
