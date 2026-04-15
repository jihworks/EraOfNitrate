// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

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
        [JsonIgnore] Map? _map;
        [JsonIgnore] Map Map => _map.ThrowIfNull(nameof(Map));

        [JsonIgnore] public int Width => Map.Width;
        [JsonIgnore] public int Height => Map.Height;

        [JsonIgnore] public int RandomSeed => Map.RandomSeed;
        [JsonProperty] public long RandomPosition { get; private set; }

        [JsonProperty(nameof(Provinces))] readonly List<Province> _provinces = new();
        [JsonIgnore] public IReadOnlyList<Province> Provinces => _provinces;

        [JsonProperty(nameof(OceanTiles))] readonly List<Tile> _oceanTiles = new();
        [JsonIgnore] public IReadOnlyList<Tile> OceanTiles => _oceanTiles;

        [JsonIgnore, MemberNotNullWhen(true,
            nameof(_randomStream),
            nameof(_worldGrid))]
        public bool IsInitialized { get; private set; }

        [JsonIgnore] RandomStream? _randomStream;
        [JsonIgnore] public RandomStream RandomStream => _randomStream.ThrowIfNull(nameof(RandomStream));

        [JsonIgnore] WorldGrid? _worldGrid;
        [JsonIgnore] public WorldGrid MapGrid => _worldGrid.ThrowIfNull(nameof(MapGrid));

        // 노트 260415 참고.
        [JsonIgnore] readonly CollisionWorld _collisionWorld = new(cellSize: 0.5f);
        [JsonIgnore] public CollisionWorld CollisionWorld => _collisionWorld;

        [JsonConstructor]
        public World()
        {
        }

        public void Bind(Map map)
        {
            _map = map;

            RandomPosition = 0;

            if (_provinces.Count <= 0)
            {
                for (int i = 0; i < map.Provinces.Count; i++)
                {
                    _provinces.Add(new Province());
                }
            }
            for (int i = 0; i < map.Provinces.Count; i++)
            {
                _provinces[i].Bind(map.Provinces[i]);
            }

            if (_oceanTiles.Count <= 0)
            {
                for (int i = 0; i < map.OceanTiles.Count; i++)
                {
                    _oceanTiles.Add(new Tile());
                }
            }
            for (int i = 0; i < map.OceanTiles.Count; i++)
            {
                _oceanTiles[i].Bind(map.OceanTiles[i]);
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

            _worldGrid = new WorldGrid(Width, Height, tiles);

            foreach (var province in _provinces)
            {
                foreach (var provinceTile in province.Tiles)
                {
                    provinceTile.Initialize(this, province, _worldGrid[(HexaIndex)(HexaCoord)provinceTile.Coord]);
                }

                province.Initialize(this, provinceMap);
            }
            foreach (var oceanTile in _oceanTiles)
            {
                oceanTile.Initialize(this, null, _worldGrid[(HexaIndex)(HexaCoord)oceanTile.Coord]);
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
