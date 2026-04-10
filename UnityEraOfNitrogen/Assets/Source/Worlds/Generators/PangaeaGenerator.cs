// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jih.Unity.EraOfNitrogen.Worlds.Generators
{
    class PangaeaGenerator
    {
        readonly Settings _settings;
        readonly GeneratorGrid _grid;
        readonly RandomStream _random;

        public List<GeneratorCell>? ResultLandCells { get; private set; }

        public PangaeaGenerator(Settings settings, GeneratorGrid grid, RandomStream random)
        {
            _settings = settings;
            _grid = grid;
            _random = random;
        }

        public void Execute()
        {
            HashSet<GeneratorCell> landCells = GenerateLandmass(_random, _grid, _settings.LandRatio);

            RemoveInlandWater(landCells, _grid);

            List<GeneratorCell> landCellsList = new(landCells);

            foreach (var cell in _grid.EnumerateCells())
            {
                cell.IsLand = false;
                cell.IsCoastlineLand = false;
            }
            foreach (var landCell in landCellsList)
            {
                landCell.IsLand = true;
            }
            foreach (var landCell in landCellsList)
            {
                landCell.IsCoastlineLand = landCell.EnumerateNeighbors().Any(n => !n.IsLand);
            }

            ResultLandCells = landCellsList;
        }

        static HashSet<GeneratorCell> GenerateLandmass(RandomStream random, GeneratorGrid mapGrid, double landRatioSetting)
        {
            HashSet<GeneratorCell> landCells = new();
            int centerX = mapGrid.Width / 2;
            int centerY = mapGrid.Height / 2;

            // 시작점 설정.
            GeneratorCell? centerCell = mapGrid.GetCell(centerX, centerY);
            if (centerCell is not null)
            {
                landCells.Add(centerCell);
            }

            int targetLandCells = (int)(mapGrid.Width * mapGrid.Height * landRatioSetting);
            Queue<GeneratorCell> expansionFrontier = new();
            expansionFrontier.Enqueue(centerCell!);

            while (landCells.Count < targetLandCells && expansionFrontier.Count > 0)
            {
                GeneratorCell current = expansionFrontier.Dequeue();

                foreach (var neighbor in current.EnumerateNeighbors())
                {
                    if (landCells.Contains(neighbor))
                    {
                        continue;
                    }

                    float expandChance = CalculateExpandProbability(neighbor, landCells, mapGrid);
                    if (random.NextDouble() < expandChance)
                    {
                        landCells.Add(neighbor);
                        expansionFrontier.Enqueue(neighbor);
                    }
                }
            }

            return landCells;
        }

        static float CalculateExpandProbability(GeneratorCell cell, HashSet<GeneratorCell> landCells, GeneratorGrid mapGrid)
        {
            // 중앙에서 멀어질 수록 확률 감소.
            Vector2Int centerIndex = new(mapGrid.Width / 2, mapGrid.Height / 2);
            Vector2Int cellIndex = new(cell.Index.X, cell.Index.Y);
            float distanceFromCenter = (cellIndex - cellIndex).magnitude;

            float maxDistance = new Vector2Int(mapGrid.Width, mapGrid.Height).magnitude * 0.5f;
            float baseChance = 1f - (distanceFromCenter / maxDistance);

            // 인접 셀이 땅이면 확률 증가.
            int adjacentLandCount = cell.EnumerateNeighbors().Count(adj => landCells.Contains(adj));
            float adjacentBonus = adjacentLandCount * 0.1f;

            return Mathf.Clamp01(baseChance + adjacentBonus);
        }

        static void RemoveInlandWater(HashSet<GeneratorCell> landCells, GeneratorGrid mapGrid)
        {
            HashSet<GeneratorCell> waterCells = new(mapGrid.EnumerateCells().Where(c => !landCells.Contains(c)));
            HashSet<GeneratorCell> processedWater = new();

            while (waterCells.Count > 0)
            {
                HashSet<GeneratorCell> waterGroup = FindConnectedWaterCells(waterCells.First(), waterCells);

                bool isInland = IsWaterGroupInland(waterGroup, landCells);

                // 내륙 호수인 경우, 땅으로 전환.
                if (isInland)
                {
                    foreach (var cell in waterGroup)
                    {
                        landCells.Add(cell);
                        waterCells.Remove(cell);
                    }
                }
                else
                {
                    foreach (var cell in waterGroup)
                    {
                        waterCells.Remove(cell);
                        processedWater.Add(cell);
                    }
                }
            }
        }

        static HashSet<GeneratorCell> FindConnectedWaterCells(GeneratorCell start, HashSet<GeneratorCell> waterCells)
        {
            HashSet<GeneratorCell> connected = new();
            Queue<GeneratorCell> queue = new();

            queue.Enqueue(start);
            connected.Add(start);

            while (queue.Count > 0)
            {
                GeneratorCell current = queue.Dequeue();

                foreach (var neighbor in current.EnumerateNeighbors())
                {
                    if (waterCells.Contains(neighbor) && !connected.Contains(neighbor))
                    {
                        connected.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return connected;
        }

        static bool IsWaterGroupInland(HashSet<GeneratorCell> waterGroup, HashSet<GeneratorCell> landCells)
        {
            foreach (var waterCell in waterGroup)
            {
                // 보더 셀인 경우 바다.
                if (waterCell.EnumerateNeighbors().Count() < 6)
                {
                    return false;
                }
            }

            // 워터 그룹에서 보더 셀 수집.
            HashSet<GeneratorCell> borderCells = new();
            foreach (var waterCell in waterGroup)
            {
                foreach (var neighbor in waterCell.EnumerateNeighbors())
                {
                    if (!waterGroup.Contains(neighbor))
                    {
                        borderCells.Add(waterCell);
                        break;
                    }
                }
            }

            // 모든 워터 그룹의 보더 셀이 땅에 연접해 있다면 내륙 호수.
            foreach (var borderCell in borderCells)
            {
                foreach (var neighbor in borderCell.EnumerateNeighbors())
                {
                    if (!landCells.Contains(neighbor) && !waterGroup.Contains(neighbor))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public struct Settings
        {
            public static Settings Default => new(0.7);

            public double LandRatio;

            public Settings(double landRatio)
            {
                LandRatio = landRatio;
            }
        }
    }
}
