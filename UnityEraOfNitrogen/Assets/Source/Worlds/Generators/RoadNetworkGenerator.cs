// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.Infrastructure.HexaGrid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jih.Unity.EraOfNitrogen.Worlds.Generators
{
    class RoadNetworkGenerator
    {
        readonly Settings _settings;
        readonly GeneratorGrid _grid;
        readonly IReadOnlyList<GeneratorProvince> _provinces;

        public RoadNetworkGenerator(Settings settings, GeneratorGrid grid, IReadOnlyList<GeneratorProvince> provinces)
        {
            _settings = settings;
            _grid = grid;
            _provinces = provinces;
        }

        public void Execute()
        {
            foreach (var province in _provinces)
            {
                province.CityCell.HasRoad = true;
            }

            Connect(_grid, _provinces);
        }

        static void Connect(GeneratorGrid mapGrid, IReadOnlyList<GeneratorProvince> provinces)
        {
            if (provinces.Count < 2)
            {
                return;
            }

            HashSet<ProvincePair> processed = new(provinces.Sum(p => p.AdjacentProvinces.Count));

            Context context = new();
            HexaPathResult result = new();
            foreach (var province in provinces)
            {
                foreach (var adjacent in province.AdjacentProvinces)
                {
                    ProvincePair pair = new(province.Id, adjacent.Id);
                    if (!processed.Add(pair))
                    {
                        continue;
                    }

                    context.Reset(province, adjacent);

                    GeneratorCell start = province.CityCell, goal = adjacent.CityCell;
                    mapGrid.FindPath(start, goal, result, context.Access, context.Cost, context.Heuristic);

                    if (!result.IsSucceed)
                    {
                        continue;
                    }

                    foreach (var roadCell in result.ResultPath.Cast<GeneratorCell>())
                    {
                        roadCell.HasRoad = true;
                    }

                    province.ConnectedProvinces.Add(adjacent);
                    adjacent.ConnectedProvinces.Add(province);
                }
            }
        }

        readonly struct ProvincePair : IEquatable<ProvincePair>
        {
            public readonly uint Id0, Id1;

            public ProvincePair(uint id0, uint id1)
            {
                Id0 = Math.Min(id0, id1);
                Id1 = Math.Max(id0, id1);
            }

            public readonly override bool Equals(object? obj)
            {
                return obj is ProvincePair pair && Equals(pair);
            }
            public readonly bool Equals(ProvincePair other)
            {
                return Id0 == other.Id0 &&
                       Id1 == other.Id1;
            }

            public readonly override int GetHashCode()
            {
                return HashCode.Combine(Id0, Id1);
            }
        }

        class Context
        {
            public GeneratorProvince? Province0 { get; private set; }
            public GeneratorProvince? Province1 { get; private set; }

            public void Reset(GeneratorProvince province0, GeneratorProvince province1)
            {
                Province0 = province0;
                Province1 = province1;
            }

            public IEnumerable<HexaCell> Access(HexaCell current)
            {
                return current.EnumerateNeighbors()
                    .Cast<GeneratorCell>()
                    .Where(c => c.IsLand && (c.Province == Province0 || c.Province == Province1));
            }

            public int Cost(HexaCell /*current*/_0, HexaCell next)
            {
                GeneratorCell nextCell = (GeneratorCell)next;

                int cost = 200; // 기본 코스트.
                if (nextCell.HasRoad)
                {
                    cost -= 100;
                }

                //GeneratorGrid grid = (GeneratorGrid)next.Map;
                //HexaCoord nextCoord = next.Coord;

                //static void ApplyNeighborRoads(GeneratorGrid grid, HexaCoord centerCoord, ref int targetCost, int ring, int deltaScore)
                //{
                //    int bufferLength = centerCoord.GetRing(ring, Span<HexaCoord>.Empty);

                //    Span<HexaCoord> neighborCoords = stackalloc HexaCoord[bufferLength];
                //    centerCoord.GetRing(ring, neighborCoords);

                //    for (int i = 0; i < bufferLength; i++)
                //    {
                //        GeneratorCell? neighbor = grid.GetCell(neighborCoords[i]);
                //        if (neighbor is null ||
                //            !neighbor.HasRoad)
                //        {
                //            continue;
                //        }
                //        targetCost += deltaScore;
                //    }
                //}
                //ApplyNeighborRoads(grid, nextCoord, ref cost, 1, -20);
                //ApplyNeighborRoads(grid, nextCoord, ref cost, 2, -5);

                return cost;
            }

            public int Heuristic(HexaCell goal, HexaCell next)
            {
                int distance = HexaCoord.Distance(goal.Coord, next.Coord);
                distance *= 20;
                return distance;
            }
        }

        public struct Settings
        {
            public static Settings Default => new();
        }
    }
}
