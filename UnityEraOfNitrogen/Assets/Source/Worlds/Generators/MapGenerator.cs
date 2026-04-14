// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.Infrastructure;
using System;
using System.Collections.Generic;

namespace Jih.Unity.EraOfNitrogen.Worlds.Generators
{
    public class MapGenerator
    {
        public Map? ResultMap { get; private set; }

        public void Execute(int? seed = null)
        {
            RandomStream random = new(seed ?? Environment.TickCount);

            UnityEngine.Debug.Log("== 맵 생성 시작");

            System.Diagnostics.Stopwatch stopwatch = new();
            stopwatch.Start();

            GeneratorGrid grid = new(128, 64);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"그리드 생성: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();

            PangaeaGenerator pangaeaGenerator = new(PangaeaGenerator.Settings.Default, grid, random);
            pangaeaGenerator.Execute();
            if (pangaeaGenerator.ResultLandCells is null ||
                pangaeaGenerator.ResultOceanCells is null)
            {
                throw new InvalidOperationException("판게아 생성 실패.");
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log($"판게아 생성: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();

            List<GeneratorCell> landCells = pangaeaGenerator.ResultLandCells;
            List<GeneratorCell> oceanCells = pangaeaGenerator.ResultOceanCells;

            ProvinceGenerator provinceGenerator = new(ProvinceGenerator.Settings.Default, random, landCells);
            provinceGenerator.Execute();
            if (provinceGenerator.ResultCityCells is null ||
                provinceGenerator.ResultProvinces is null)
            {
                throw new InvalidOperationException("프로빈스 생성 실패.");
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log($"프로빈스 생성: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();

            //List<GeneratorCell> cityCells = provinceGenerator.ResultCityCells;
            List<GeneratorProvince> provinces = provinceGenerator.ResultProvinces;

            BiomeGenerator biomeGenerator = new(BiomeGenerator.Settings.Default, random, provinces);
            biomeGenerator.Execute();
            stopwatch.Stop();
            UnityEngine.Debug.Log($"바이옴 생성: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();

            DoodadGenerator doodadGenerator = new(random, provinces);
            doodadGenerator.Execute();
            stopwatch.Stop();
            UnityEngine.Debug.Log($"두대드 생성: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();

            RoadNetworkGenerator roadNetworkGenerator = new(RoadNetworkGenerator.Settings.Default, grid, provinces);
            roadNetworkGenerator.Execute();
            stopwatch.Stop();
            UnityEngine.Debug.Log($"도로 생성: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();

            ResultMap = new Map(grid, random.Seed, provinces, oceanCells);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"인스턴스 생성: {stopwatch.ElapsedMilliseconds}ms");

            UnityEngine.Debug.Log("== 맵 생성 완료");
        }
    }
}
