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

        [JsonIgnore] public bool IsInitialized { get; private set; }

        [JsonIgnore, MemberNotNullWhen(true, nameof(IsInitialized))] public RandomStream? RandomStream { get; private set; }

        [JsonIgnore] List<Tile>? _oceanTiles;
        [JsonIgnore, MemberNotNullWhen(true, nameof(IsInitialized))] public IReadOnlyList<Tile>? OceanTiles => _oceanTiles;

        [JsonIgnore, MemberNotNullWhen(true, nameof(IsInitialized))] public MapGrid? MapGrid { get; private set; }

        [JsonConstructor]
        private World()
        {
            _provinces = null!;
        }

        public World(GeneratorGrid generatorGrid, int mapSeed, IReadOnlyList<GeneratorProvince> generatorProvinces)
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
        }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            RandomStream = new RandomStream()
            {
                Position = RandomPosition,
            };

            Tile[,] tiles = new Tile[Height, Width];
            int provinceTileCount = 0;
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
                    provinceTileCount++;
                }
            }
            _oceanTiles = new(Width * Height - provinceTileCount);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    ref Tile tile = ref tiles[y, x];
                    if (tile is not null)
                    {
                        continue;
                    }

                    Tile oceanTile = new((HexaCoord)new HexaIndex(x, y), false);
                    _oceanTiles.Add(oceanTile);

                    tile = oceanTile;
                }
            }

            MapGrid = new MapGrid(Width, Height, tiles);

            foreach (var province in _provinces)
            {
                foreach (var provinceTile in province.Tiles)
                {
                    provinceTile.Initialize(province, MapGrid[(HexaIndex)(HexaCoord)provinceTile.Coord]);
                }
            }
            foreach (var oceanTile in _oceanTiles)
            {
                oceanTile.Initialize(null, MapGrid[(HexaIndex)(HexaCoord)oceanTile.Coord]);
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
