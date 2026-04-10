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
        readonly IReadOnlyList<GeneratorCell> _cityCells;

        public RoadNetworkGenerator(Settings settings, GeneratorGrid grid, IReadOnlyList<GeneratorCell> cityCells)
        {
            _settings = settings;
            _grid = grid;
            _cityCells = cityCells;
        }

        public void Execute()
        {
            foreach (var cell in _cityCells)
            {
                cell.HasRoad = true;
            }

            Connect(_grid, _cityCells);
        }

        static void Connect(GeneratorGrid mapGrid, IReadOnlyList<GeneratorCell> cityCells)
        {
            Queue<GeneratorCell> startings = new(cityCells);
            while (startings.TryDequeue(out GeneratorCell start))
            {
                HexaCoord startCoord = start.Coord;

                List<(GeneratorCell Cell, int Distance)> targets = new(cityCells.Count);
                targets.AddRange(cityCells.Where(c => c != start).Select(c => (c, HexaCoord.Distance(c.Coord, startCoord))));
                targets.Sort((l, r) => l.Distance.CompareTo(r.Distance));

                HexaPathResult result = new();
                foreach (var (goal, _) in targets)
                {
                    mapGrid.FindPath(start, goal, result, Access, Cost, Heuristic);

                    if (!result.IsSucceed)
                    {
                        throw new InvalidOperationException($"Failed to connect road from {start.Coord} to {goal.Coord}.");
                    }

                    foreach (var roadCell in result.ResultPath.Cast<GeneratorCell>())
                    {
                        roadCell.HasRoad = true;
                    }
                }
            }
        }

        static IEnumerable<HexaCell> Access(HexaCell current)
        {
            return current.EnumerateNeighbors().Cast<GeneratorCell>().Where(c => c.IsLand);
        }

        static int Cost(HexaCell current, HexaCell next)
        {
            GeneratorCell nextCell = (GeneratorCell)next;

            int cost = 100; // 기본 코스트.
            if (nextCell.HasRoad)
            {
                cost -= 20;
            }

            int roadCount = next.EnumerateNeighbors().Cast<GeneratorCell>().Count(c => c != current && c.HasRoad);
            cost -= roadCount * 10;

            return cost;
        }

        static int Heuristic(HexaCell goal, HexaCell next)
        {
            int distance = HexaCoord.Distance(goal.Coord, next.Coord);
            return distance;
        }

        public struct Settings
        {
            public static Settings Default => new();
        }
    }
}
