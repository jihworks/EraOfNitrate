// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.Infrastructure;
using Jih.Unity.Infrastructure.HexaGrid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jih.Unity.EraOfNitrogen.Worlds.Generators
{
    class ProvinceGenerator
    {
        readonly Settings _settings;
        readonly RandomStream _random;
        readonly IReadOnlyList<GeneratorCell> _landCells;

        public List<GeneratorCell>? ResultCityCells { get; private set; }
        public List<GeneratorProvince>? ResultProvinces { get; private set; }

        public ProvinceGenerator(Settings settings, RandomStream random, IReadOnlyList<GeneratorCell> landCells)
        {
            _settings = settings;
            _random = random;
            _landCells = landCells;
        }

        public void Execute()
        {
            List<GeneratorCell> cityCells = GenerateCapitals(_random, _landCells, _settings.MaxProvinceCount, _settings.MinCityDistance);
            if (cityCells.Count <= 0)
            {
                return;
            }

            List<GeneratorProvince> provinces = GenerateProvinces(_landCells, cityCells);

            ResultCityCells = cityCells;
            ResultProvinces = provinces;
        }

        static List<GeneratorCell> GenerateCapitals(RandomStream random, IReadOnlyList<GeneratorCell> landCells, int maxProvinceCountSetting, int minCityDistanceSetting)
        {
            List<GeneratorCell> result = new();

            List<GeneratorCell> candidates = new(landCells.Count);
            // 해안선은 도시 후보에서 제외.
            candidates.AddRange(landCells.Where(l => !l.IsCoastlineLand));

            while (candidates.Count > 0 && result.Count < maxProvinceCountSetting)
            {
                bool anyFound = false;
                for (int retryCount = 0; retryCount < landCells.Count; retryCount++)
                {
                    int pickedIndex = random.NextInt32(0, candidates.Count);
                    GeneratorCell pickedCell = candidates[pickedIndex];
                    candidates.RemoveAt(pickedIndex);

                    if (CheckDistance(pickedCell, result, minCityDistanceSetting))
                    {
                        result.Add(pickedCell);
                        anyFound = true;
                        break;
                    }
                }

                if (!anyFound)
                {
                    break;
                }
            }

            return result;
        }

        static bool CheckDistance(GeneratorCell cell, List<GeneratorCell> cityCells, int minCityDistanceSetting)
        {
            if (cityCells.Contains(cell))
            {
                return false;
            }

            foreach (var cityCell in cityCells)
            {
                if (HexaCoord.Distance(cell.Coord, cityCell.Coord) < minCityDistanceSetting)
                {
                    return false;
                }
            }
            return true;
        }

        static List<GeneratorProvince> GenerateProvinces(IReadOnlyList<GeneratorCell> landCells, IReadOnlyList<GeneratorCell> cityCells)
        {
            List<GeneratorProvince> result = new(cityCells.Count);

            foreach (var cityCell in cityCells)
            {
                GeneratorProvince province = new(cityCell);

                province.Cells.Add(cityCell);
                cityCell.Province = province;

                result.Add(province);
            }

            // 도시는 제외. 이미 처리됨.
            foreach (var expandTarget in landCells.Where(l => !cityCells.Contains(l)))
            {
                // 보로노이 알고리즘으로 전파.
                GeneratorProvince province = FindNearestCapital(expandTarget, cityCells).Province ?? throw new InvalidOperationException();

                expandTarget.Province = province;
                province.Cells.Add(expandTarget);
            }

            return result;
        }

        static GeneratorCell FindNearestCapital(GeneratorCell cell, IReadOnlyList<GeneratorCell> cityCells)
        {
            HexaCoord cellCoord = cell.Coord;

            int nearestDistance = int.MaxValue;
            GeneratorCell? nearestCity = null;

            foreach (var city in cityCells)
            {
                int distance = HexaCoord.Distance(cellCoord, city.Coord);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCity = city;
                }
            }

            return nearestCity ?? throw new InvalidOperationException();
        }

        public struct Settings
        {
            public static Settings Default => new(4, int.MaxValue);

            public int MinCityDistance;
            public int MaxProvinceCount;

            public Settings(int minCityDistance, int maxProvinceCount)
            {
                MinCityDistance = minCityDistance;
                MaxProvinceCount = maxProvinceCount;
            }
        }
    }
}
