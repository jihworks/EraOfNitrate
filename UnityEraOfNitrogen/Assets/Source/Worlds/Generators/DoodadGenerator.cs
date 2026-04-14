// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.Infrastructure;
using Jih.Unity.Infrastructure.HexaGrid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Jih.Unity.EraOfNitrogen.CoordinateSpaceEx;

namespace Jih.Unity.EraOfNitrogen.Worlds.Generators
{
    class DoodadGenerator
    {
        readonly RandomStream _random;
        readonly IReadOnlyList<GeneratorProvince> _provinces;

        readonly Dictionary<Biome, DoodadRule> _rules = new()
        {
            { Biome.Grassland, new DoodadRule(DoodadType.Broadleaf, retryCount: 1, minDistance: 1.6f, borderPushDistance: 0.2f) },
            { Biome.Rainforest, new DoodadRule(DoodadType.PalmTree, retryCount: 4, minDistance: 0.8f, borderPushDistance: 0.2f) },
            { Biome.Tundra, new DoodadRule(DoodadType.Needleleaf, retryCount: 2, minDistance: 0.3f, borderPushDistance: 0.2f) },
            { Biome.Desert, new DoodadRule(DoodadType.Cactus, retryCount: 1, minDistance: 4.8f, borderPushDistance: 0.2f) },
            { Biome.Steppe, new DoodadRule(type: 0, retryCount: 0, minDistance: 0f, borderPushDistance: 0f) }, // 배치 안함
            { Biome.Snow, new DoodadRule(type: 0, retryCount: 0, minDistance: 0f, borderPushDistance: 0f) }    // 배치 안함
        };

        readonly int _minScale100 = 24, _maxScale100 = 32;

        public DoodadGenerator(RandomStream random, IReadOnlyList<GeneratorProvince> provinces)
        {
            _random = random;
            _provinces = provinces;
        }

        public void Execute()
        {
            foreach (var province in _provinces)
            {
                if (!_rules.TryGetValue(province.Biome, out DoodadRule rule) || rule.RetryCount == 0)
                {
                    continue;
                }

                List<GeneratorDoodad> provinceDoodads = new();

                foreach (var cell in province.Cells)
                {
                    // 바다 제외
                    if (!cell.IsLand)
                    {
                        continue;
                    }

                    // 셀 내부 로컬 배치
                    GenerateInCell(cell, provinceDoodads, rule);
                }
            }
        }

        void GenerateInCell(GeneratorCell cell, List<GeneratorDoodad> provinceDoodads, DoodadRule rule)
        {
            Vector3 unityCenter = ScreenToUnity(HexaToScreen(cell.Coord));

            for (int i = 0; i < rule.RetryCount; i++)
            {
                Vector2 screenOffset = GenerateRandomOffsetInHexaCell();
                Vector3 unityLocation = unityCenter + ScreenToUnity(screenOffset);

                if (!IsValidPosition(cell, provinceDoodads, unityLocation, rule.MinDistance, rule.BorderPushDistance))
                {
                    continue;
                }

                GeneratorDoodad doodad = new(rule.Type,
                    _random.NextInt32(0, 100), // 배리에이션 번호. 넉넉한 범위의 아무 난수나 할당.
                    unityLocation,
                    _random.NextInt32(0, 360), // Y 회전
                    _random.NextInt32(_minScale100, _maxScale100 + 1) / 100f); // 스케일

                cell.Doodads.Add(doodad);
                provinceDoodads.Add(doodad);
            }
        }

        // 무게중심 좌표계 사용.
        Vector2 GenerateRandomOffsetInHexaCell()
        {
            HexaVertexPosition vertexPosition0 = (HexaVertexPosition)_random.NextInt32(0, 6);
            HexaVertexPosition vertexPosition1 = vertexPosition0.Next();

            Vector2 v0 = MathEx.RadiusVector(vertexPosition0.GetRadiusDegrees().ToRadians());
            Vector2 v1 = MathEx.RadiusVector(vertexPosition1.GetRadiusDegrees().ToRadians());

            float u = (float)_random.NextDouble();
            float v = (float)_random.NextDouble();

            if (u + v > 1f)
            {
                u = 1f - u;
                v = 1f - v;
            }

            return (v0 * u) + (v1 * v);
        }

        bool IsValidPosition(GeneratorCell cell, List<GeneratorDoodad> provinceDoodads, Vector3 unityLocation, float minDistance, float borderPushDistance)
        {
            // A. 현재 프로빈스 내부에 배치된 두대드 체크.
            float minDistSq = minDistance.Sq();
            foreach (var d in provinceDoodads)
            {
                if ((d.UnityLocation - unityLocation).sqrMagnitude < minDistSq)
                {
                    return false;
                }
            }

            // B. 프로빈스 보더 체크.
            float borderPushDistSq = borderPushDistance.Sq();
            for (int p = 0; p < 6; p++)
            {
                HexaNeighborPosition neighborPosition = (HexaNeighborPosition)p;

                GeneratorCell? neighbor = (GeneratorCell?)cell.GetNeighbor(neighborPosition);
                if (neighbor is not null &&
                    // 동일 프로빈스인 경우 통과.
                    neighbor.Province == cell.Province)
                {
                    continue;
                }

                HexaEdge edge = cell.GetEdge(neighborPosition.ConvertToEdge());
                HexaVertex v0 = edge.Vertex0, v1 = edge.Vertex1;
                Vector3 unityLocation0 = ScreenToUnity(HexaToScreen(v0.Coord));
                Vector3 unityLocation1 = ScreenToUnity(HexaToScreen(v1.Coord));

                Vector3 unityClosestPoint = MathEx.GetClosestPointOnLine(unityLocation0, unityLocation1, unityLocation);
                if ((unityClosestPoint - unityLocation).sqrMagnitude < borderPushDistSq)
                {
                    return false;
                }
            }

            return true;
        }

        struct DoodadRule
        {
            public DoodadType Type;
            public int RetryCount;
            public float MinDistance;
            public float BorderPushDistance;

            public DoodadRule(DoodadType type, int retryCount, float minDistance, float borderPushDistance)
            {
                Type = type;
                RetryCount = retryCount;
                MinDistance = minDistance;
                BorderPushDistance = borderPushDistance;
            }
        }
    }
}
