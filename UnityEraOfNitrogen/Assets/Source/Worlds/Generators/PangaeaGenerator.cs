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
        public List<GeneratorCell>? ResultOceanCells { get; private set; }

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
            List<GeneratorCell> oceanCells = new(_grid.Width * _grid.Height - landCells.Count);

            foreach (var cell in _grid.EnumerateCells())
            {
                cell.IsLand = false;
                cell.IsCoastlineLand = false;
                cell.IsNearOcean = false;

                if (landCells.Contains(cell))
                {
                    cell.IsLand = true;
                }
                else
                {
                    oceanCells.Add(cell);
                }
            }

            foreach (var landCell in landCellsList)
            {
                landCell.IsCoastlineLand = landCell.EnumerateNeighbors().Any(n => !n.IsLand);
            }
            foreach (var oceanCell in oceanCells)
            {
                oceanCell.IsNearOcean = oceanCell.EnumerateNeighbors().Any(n => n.IsLand);
            }

            ResultLandCells = landCellsList;
            ResultOceanCells = oceanCells;
        }

        static HashSet<GeneratorCell> GenerateLandmass(RandomStream random, GeneratorGrid mapGrid, double landRatioSetting)
        {
            HashSet<GeneratorCell> landCells = new();
            Queue<GeneratorCell> expansionFrontier = new();

            int centerX = mapGrid.Width / 2;
            int centerY = mapGrid.Height / 2;

            float noiseOffsetX = (float)(random.NextDouble() * 10000.0);
            float noiseOffsetY = (float)(random.NextDouble() * 10000.0);

            // 중앙 부근의 여러 시작점 생성.
            int seedCount = 3 + random.NextInt32(0, 4);
            for (int i = 0; i < seedCount; i++)
            {
                int offsetX = random.NextInt32(-mapGrid.Width / 6, mapGrid.Width / 6);
                int offsetY = random.NextInt32(-mapGrid.Height / 6, mapGrid.Height / 6);

                GeneratorCell? seedCell = mapGrid.GetCell(centerX + offsetX, centerY + offsetY);
                if (seedCell is not null && !landCells.Contains(seedCell))
                {
                    landCells.Add(seedCell);
                    expansionFrontier.Enqueue(seedCell);
                }
            }

            int targetLandCells = (int)(mapGrid.Width * mapGrid.Height * landRatioSetting);

            while (landCells.Count < targetLandCells && expansionFrontier.Count > 0)
            {
                GeneratorCell current = expansionFrontier.Dequeue();

                foreach (var neighbor in current.EnumerateNeighbors())
                {
                    if (landCells.Contains(neighbor))
                    {
                        continue;
                    }

                    float expandChance = CalculateExpandProbability(neighbor, landCells, mapGrid, noiseOffsetX, noiseOffsetY);
                    if (random.NextDouble() < expandChance)
                    {
                        landCells.Add(neighbor);
                        expansionFrontier.Enqueue(neighbor);
                    }
                }
            }

            return landCells;
        }

        static float CalculateExpandProbability(GeneratorCell cell, HashSet<GeneratorCell> landCells, GeneratorGrid mapGrid, float noiseOffsetX, float noiseOffsetY)
        {
            Vector2 centerIndex = new(mapGrid.Width * 0.5f, mapGrid.Height * 0.5f);
            Vector2 cellIndex = new(cell.Index.X, cell.Index.Y);

            // X와 Y 거리를 각각 -1.0 ~ 1.0로 정규화.
            float normalizedX = (cellIndex.x - centerIndex.x) / (mapGrid.Width * 0.5f);
            float normalizedY = (cellIndex.y - centerIndex.y) / (mapGrid.Height * 0.5f);
            float normalizedDistance = new Vector2(normalizedX, normalizedY).magnitude;

            // 맵 가장자리에 매우 근접하면 확장 차단.
            if (normalizedDistance > 0.95f)
            {
                return 0f;
            }

            // 거리에 따른 확률 감소 적용. (중심 0, 가장자리 1 기준)
            float baseChance = 1f - (normalizedDistance / 0.85f);

            // 인접 셀 보너스.
            int adjacentLandCount = cell.EnumerateNeighbors().Count(adj => landCells.Contains(adj));
            float adjacentBonus = adjacentLandCount * 0.15f;

            // 노이즈 보너스.
            float noiseScale = 0.1f;
            float noiseValue = Mathf.PerlinNoise(
                cellIndex.x * noiseScale + noiseOffsetX,
                cellIndex.y * noiseScale + noiseOffsetY
            );
            float noiseBonus = (noiseValue - 0.5f);

            return Mathf.Clamp01(baseChance + adjacentBonus + noiseBonus);
        }

        static void RemoveInlandWater(HashSet<GeneratorCell> landCells, GeneratorGrid mapGrid)
        {
            HashSet<GeneratorCell> globalOcean = new();
            Queue<GeneratorCell> queue = new();

            foreach (var cell in mapGrid.EnumerateCells())
            {
                if (!landCells.Contains(cell))
                {
                    if (cell.EnumerateNeighbors().Count() < 6)
                    {
                        queue.Enqueue(cell);
                        globalOcean.Add(cell);
                    }
                }
            }

            // Flood Fill.
            while (queue.Count > 0)
            {
                GeneratorCell current = queue.Dequeue();

                foreach (var neighbor in current.EnumerateNeighbors())
                {
                    if (!landCells.Contains(neighbor) && !globalOcean.Contains(neighbor))
                    {
                        globalOcean.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            List<GeneratorCell> cellsToFill = new();
            foreach (var cell in mapGrid.EnumerateCells())
            {
                if (!landCells.Contains(cell) && !globalOcean.Contains(cell))
                {
                    cellsToFill.Add(cell);
                }
            }

            foreach (var cell in cellsToFill)
            {
                landCells.Add(cell);
            }
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
