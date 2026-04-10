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
    public class WorldGenerator
    {
        public World? ResultWorld { get; private set; }

        public void Execute()
        {
            RandomStream random = new();

            GeneratorGrid grid = new(128, 128);

            PangaeaGenerator pangaeaGenerator = new(PangaeaGenerator.Settings.Default, grid, random);
            pangaeaGenerator.Execute();
            if (pangaeaGenerator.ResultLandCells is null)
            {
                throw new InvalidOperationException();
            }

            List<GeneratorCell> landCells = pangaeaGenerator.ResultLandCells;

            ProvinceGenerator provinceGenerator = new(ProvinceGenerator.Settings.Default, random, landCells);
            provinceGenerator.Execute();
            if (provinceGenerator.ResultCityCells is null ||
                provinceGenerator.ResultProvinces is null)
            {
                throw new InvalidOperationException();
            }

            List<GeneratorCell> cityCells = provinceGenerator.ResultCityCells;
            List<GeneratorProvince> provinces = provinceGenerator.ResultProvinces; 

            RoadNetworkGenerator roadNetworkGenerator = new(RoadNetworkGenerator.Settings.Default, grid, cityCells);
            roadNetworkGenerator.Execute();

            ResultWorld = new World(grid, random.Seed, provinces);
        }
    }
}
