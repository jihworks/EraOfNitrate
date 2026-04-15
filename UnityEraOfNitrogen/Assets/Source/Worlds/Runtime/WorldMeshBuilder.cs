// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.Infrastructure;
using Jih.Unity.Infrastructure.Collisions.Common3D;
using Jih.Unity.Infrastructure.Geometries;
using Jih.Unity.Infrastructure.HexaGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static Jih.Unity.EraOfNitrogen.CoordinateSpaceEx;

namespace Jih.Unity.EraOfNitrogen.Worlds.Runtime
{
    public class WorldMeshBuilder
    {
        public World World { get; }
        public Assets Assets { get; }

        readonly int _chunkCountX, _chunkCountY;

        public WorldMeshBuilder(World world, Assets? assets = null)
        {
            World = world;
            Assets = assets != null ? assets : Assets.Instance;

            WorldGrid grid = World.MapGrid;
            _chunkCountX = grid.Width.CeilDivision(ChunkSize);
            _chunkCountY = grid.Height.CeilDivision(ChunkSize);
        }

        public List<LandChunkResult> BuildLand()
        {
            if (!World.IsInitialized)
            {
                throw new InvalidOperationException("초기화되지 않은 월드로부터 메시를 빌드할 수 없음.");
            }

            WorldGrid grid = World.MapGrid;

            List<LandChunkResult> chunks = new(_chunkCountX * _chunkCountY);
            foreach (var ci in EnumerateChunks(_chunkCountX, _chunkCountY))
            {
                List<WorldCell> cells = new(ChunkSize * ChunkSize);

                foreach (var index in ci.Cells)
                {
                    WorldCell? cell = grid.GetCell(index);
                    if (cell is null)
                    {
                        continue;
                    }
                    if (!cell.Tile.IsLand) // 땅인지 확인.
                    {
                        continue;
                    }

                    cells.Add(cell);
                }

                if (cells.Count > 0)
                {
                    chunks.Add(new LandChunkResult(ci.X, ci.Y, ci.BaseGridX, ci.BaseGridY, cells, new MeshCollector(AdditionalAttributes.Color)));
                }
            }

            foreach (var chunk in chunks)
            {
                CollectLandChunk(Assets, chunk);
            }

            return chunks;
        }

        static void CollectLandChunk(Assets assets, in LandChunkResult chunk)
        {
            MeshCollector meshCollector = chunk.MeshCollector;

            VertexData[] cwCornersBuffer = new VertexData[6];

            foreach (var cell in chunk.Cells)
            {
                Tile tile = cell.Tile;
                if (!tile.IsLand)
                {
                    throw new InvalidOperationException($"땅이 아닌 타일 {cell.Coord} 로부터 땅 메시를 생성할 수 없음.");
                }

                if (!tile.IsInitialized)
                {
                    throw new InvalidOperationException($"초기화되지 않은 타일 {cell.Coord} 로부터 땅 메시를 생성할 수 없음.");
                }

                Province? province = tile.Province ?? throw new InvalidOperationException($"땅 타일 {cell.Coord} 인데 프로빈스가 없음.");

                Color32 biomeColor = assets.GetColor(province.Biome);

                Vector2 screenCenterLocation = HexaToScreen(cell.Coord);
                Vector3 unityCenterLocation = ScreenToUnity(screenCenterLocation);

                VertexData center = new(unityCenterLocation, biomeColor);

                for (int v = 0; v < 6; v++)
                {
                    HexaVertex vertex = cell.GetVertex((HexaVertexPosition)v);

                    Vector3 unityVertexLocation = ScreenToUnity(HexaToScreen(vertex.Coord));

                    cwCornersBuffer[v] = new VertexData(unityVertexLocation, biomeColor);
                }

                meshCollector.AppendNGon(center, cwCornersBuffer);
            }
        }

        public List<GameObject> Spawn(IReadOnlyList<LandChunkResult> landResults, Transform? parent)
        {
            Material landMaterial = Assets.LandMaterial;
            Material[] materials = new Material[] { landMaterial, };

            List<GameObject> meshObjs = new(landResults.Count);

            foreach (var chunk in landResults)
            {
                string name = $"Land Mesh {chunk.ChunkX} x {chunk.ChunkY}";

                Mesh mesh = chunk.MeshCollector.ToTrianglesMesh(true, true);
                mesh.name = name;

                GameObject meshObj = new() { name = name, };
                meshObjs.Add(meshObj);

                if (parent != null)
                {
                    meshObj.transform.SetParent(parent, false);
                }

                MeshFilter meshFilter = meshObj.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;

                MeshRenderer meshRenderer = meshObj.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterials = materials;
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            }

            return meshObjs;
        }

        public Dictionary<Province, List<RoadElementResult>> BuildRoads()
        {
            if (!World.IsInitialized)
            {
                throw new InvalidOperationException("초기화되지 않은 월드로부터 도로를 빌드할 수 없음.");
            }

            Dictionary<Province, List<RoadElementResult>> result = new(World.Provinces.Count);

            foreach (var province in World.Provinces)
            {
                List<RoadElementResult> roadBlocks = new(province.Tiles.Count);
                result.Add(province, roadBlocks);

                foreach (var tile in province.Tiles)
                {
                    if (!tile.HasRoad)
                    {
                        continue;
                    }

                    if (!tile.IsInitialized)
                    {
                        throw new InvalidOperationException("초기화되지 않은 타일로부터 도로를 빌드할 수 없음.");
                    }

                    RoadBranch branch = RoadBranch.None;
                    for (int b = 0; b < RoadBranch.MaxCount; b++)
                    {
                        HexaNeighborPosition position = (HexaNeighborPosition)b;
                        WorldCell? neighbor = tile.Cell.GetNeighbor(position);
                        if (neighbor is null ||
                            !neighbor.Tile.HasRoad)
                        {
                            continue;
                        }
                        branch[b] = true;
                    }

                    if (branch == RoadBranch.None)
                    {
                        roadBlocks.Add(new RoadElementResult(province, tile, tile.Cell, branch, branch, 0));
                        continue;
                    }

                    int cwShiftCount = -1;
                    RoadBranch meshBranch = branch;
                    for (int s = 0; s < RoadBranch.MaxCount; s++)
                    {
                        if (RoadBranch.Branches.Contains(meshBranch))
                        {
                            cwShiftCount = s;
                            break;
                        }

                        meshBranch >>= 1;
                    }
                    if (cwShiftCount < 0)
                    {
                        throw new InvalidOperationException($"도로 브랜치 {branch} 에 대한 메시 브랜치를 찾지 못함.");
                    }

                    roadBlocks.Add(new RoadElementResult(province, tile, tile.Cell, branch, meshBranch, cwShiftCount));
                }
            }

            return result;
        }

        public List<RoadElement> Spawn(KeyValuePair<Province, List<RoadElementResult>> roadResults, Transform? parent)
        {
            List<RoadElement> result = new(roadResults.Value.Count);

            RoadAssets assets = Assets.GetRoadAssets(RoadLevel.Dirt/*TODO: 테스트 용 도로 레벨.*/);

            Material material = assets.Material;
            Material[] materials = new Material[] { material, };

            Texture2D mainTexture = assets.MainTexture;

            MaterialPropertyBlock propertyBlock = new();
            propertyBlock.SetTexture(ShaderIds.MainTexure, mainTexture);

            foreach (var roadBlock in roadResults.Value)
            {
                Mesh mesh = assets.GetMesh(roadBlock.MeshBranch, out MeshCollector meshCollector);

                GameObject meshObj;
                Matrix4x4 worldMatrix;
                {
                    float zRotation = assets.MeshBaseZRotation;
                    // CW로 회전하여 찾았으므로, CCW로 회전해야 함.
                    zRotation += roadBlock.CwShiftCount * -60f;

                    Quaternion rotation = Quaternion.AngleAxis(zRotation, Vector3.up);

                    HexaCoord coord = roadBlock.Cell.Coord;
                    Vector2 screenLocation = HexaToScreen(coord);
                    Vector3 unityLocation = ScreenToUnity(screenLocation);

                    meshObj = new GameObject() { name = "Road Block " + coord, };

                    if (parent != null)
                    {
                        meshObj.transform.SetParent(parent, false);
                    }
                    meshObj.transform.SetLocalPositionAndRotation(unityLocation, rotation);
                    worldMatrix = meshObj.transform.localToWorldMatrix;

                    MeshFilter meshFilter = meshObj.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = mesh;

                    MeshRenderer meshRenderer = meshObj.AddComponent<MeshRenderer>();
                    meshRenderer.sharedMaterials = materials;
                    meshRenderer.SetPropertyBlock(propertyBlock);
                    meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                }

                MeshShape collisionShape;
                {
                    collisionShape = new MeshShape();

                    collisionShape.Append(meshCollector);
                    collisionShape.WorldTransform = worldMatrix;
                    //collisionShape.Freeze(); // 노트 260415 참고.
                }

                RoadElement roadElement = new(roadBlock.Tile, new List<GameObject>() { meshObj, }, collisionShape);
                result.Add(roadElement);

                roadBlock.Tile.Spawned(roadElement);
            }

            return result;
        }

        public List<DoodadClusterResult> BuildDoodads()
        {
            if (!World.IsInitialized)
            {
                throw new InvalidOperationException("초기화되지 않은 월드로부터 두대드를 빌드할 수 없음.");
            }

            int VariantToIndex(DoodadType type, int variant)
            {
                DoodadAssets doodadAssets = Assets.GetDoodadAssets(type);
                return doodadAssets.VariantToIndex(variant);
            }

            List<DoodadClusterResult> result = new(World.Provinces.Count * 3);

            foreach (var provinceGroup in World.Provinces.SelectMany(
                p => p.Tiles.SelectMany(
                    t => t.Doodads.Select(
                        d => (Province: p, Tile: t, Doodad: d, d.Type, Index: VariantToIndex(d.Type, d.Variant)))))
                .GroupBy(x => x.Province))
            {
                foreach (var typeGroup in provinceGroup.GroupBy(x => x.Type))
                {
                    foreach (var indexGroup in typeGroup.GroupBy(x => x.Index))
                    {
                        List<Doodad> doodads = indexGroup.Select(x => x.Doodad).ToList();

                        result.Add(new DoodadClusterResult(provinceGroup.Key, typeGroup.Key, indexGroup.Key, doodads));
                    }
                }
            }

            return result;
        }

        public List<DoodadCluster> Spawn(IReadOnlyList<DoodadClusterResult> doodadResults)
        {
            List<DoodadCluster> result = new(doodadResults.Count);

            foreach (var group in doodadResults)
            {
                Province province = group.Province;
                IReadOnlyList<Doodad> doodads = group.Doodads;

                DoodadAssets assets = Assets.GetDoodadAssets(group.Type);

                Mesh mesh = assets.GetMesh(group.Index, out SerializableMesh convexHull);
                Material[] materials = new Material[] { assets.Material, };

                List<DoodadTransform> transforms = doodads.Select(d => d.GetTransform()).ToList();

                DoodadCluster cluster = new(province, mesh, convexHull, materials, transforms);
                result.Add(cluster);

                cluster.RegisterCollisions(World.CollisionWorld);

                for (int i = 0; i < cluster.Elements.Count; i++)
                {
                    doodads[i].Spawned(cluster.Elements[i]);
                }
                province.Spwaned(cluster);
            }

            return result;
        }

        public List<ProvinceBorderResult> BuildProvinceBorders()
        {
            if (!World.IsInitialized)
            {
                throw new InvalidOperationException("초기화되지 않은 월드로부터 프로빈스 경계를 빌드할 수 없음.");
            }

            const float SolidThickness = 0.03f;
            const float FallOffThickness = 0.06f;

            List<ProvinceBorderResult> result = new(World.Provinces.Count);
            foreach (var province in World.Provinces)
            {
                List<(WorldCell Cell, HexaEdge Edge)> borderEdges = new();
                foreach (var tile in province.Tiles)
                {
                    WorldCell cell = tile.Cell;

                    for (int p = 0; p < 6; p++)
                    {
                        HexaNeighborPosition position = (HexaNeighborPosition)p;

                        WorldCell? neighbor = cell.GetNeighbor(position);
                        if (neighbor is not null &&
                            neighbor.Tile.Province == province)
                        {
                            continue;
                        }

                        HexaEdge edge = cell.GetEdge(position.ConvertToEdge());
                        borderEdges.Add((cell, edge));
                    }
                }

                MeshCollector collector = new(AdditionalAttributes.Color);

                // 일자 부분.
                foreach (var (cell, edge) in borderEdges)
                {
                    Vector3 cellCenter = ScreenToUnity(HexaToScreen(cell.Coord));

                    edge.GetCwOrder(cell, out HexaVertex vertex0, out HexaVertex vertex1);
                    Vector3 v0Point = ScreenToUnity(HexaToScreen(vertex0.Coord));
                    Vector3 v1Point = ScreenToUnity(HexaToScreen(vertex1.Coord));

                    Vector3 v0ToCenterDir = (cellCenter - v0Point).normalized;
                    Vector3 v1ToCenterDir = (cellCenter - v1Point).normalized;

                    Vector3 v0MidPoint = v0Point + v0ToCenterDir * SolidThickness;
                    Vector3 v1MidPoint = v1Point + v1ToCenterDir * SolidThickness;

                    Vector3 v0EndPoint = v0MidPoint + v0ToCenterDir * FallOffThickness;
                    Vector3 v1EndPoint = v1MidPoint + v1ToCenterDir * FallOffThickness;

                    // R = 솔리드 컬러, G = 폴오프 컬러.
                    VertexData v0SolidD = new(v0Point, new Color(1f, 0f, 0f, 1f));
                    VertexData v1SolidD = new(v1Point, new Color(1f, 0f, 0f, 1f));

                    VertexData v0MidSolidD = new(v0MidPoint, new Color(1f, 0f, 0f, 1f));
                    VertexData v1MidSolidD = new(v1MidPoint, new Color(1f, 0f, 0f, 1f));

                    VertexData v0MidFalloffD = new(v0MidPoint, new Color(0f, 1f, 0f, 1f));
                    VertexData v1MidFalloffD = new(v1MidPoint, new Color(0f, 1f, 0f, 1f));

                    VertexData v0EndFalloffD = new(v0EndPoint, new Color(0f, 1f, 0f, 0f));
                    VertexData v1EndFalloffD = new(v1EndPoint, new Color(0f, 1f, 0f, 0f));

                    collector.AppendQuad(v0EndFalloffD, v1EndFalloffD, v0MidFalloffD, v1MidFalloffD);
                    collector.AppendQuad(v0MidSolidD, v1MidSolidD, v0SolidD, v1SolidD);
                }
                // 바깥쪽 꺾인 부분.
                HashSet<HexaCell> borderCells = new(borderEdges.Count);
                HashSet<HexaVertex> borderVertices = new(borderEdges.Count * 2);
                foreach (var (cell, edge) in borderEdges)
                {
                    borderCells.Add(cell);

                    borderVertices.Add(edge.Vertex0);
                    borderVertices.Add(edge.Vertex1);
                }
                foreach (var vertex in borderVertices)
                {
                    // 엣지의 양쪽 셀 모두가 현재 경계 셀인 경우.
                    foreach (var edge in vertex.EnumerateEdges())
                    {
                        if (edge.EnumerateCells().Count(c => borderCells.Contains(c)) < 2)
                        {
                            continue;
                        }

                        HexaVertex vertex0, vertex1;
                        HexaCell rightCell, leftCell;
                        if (edge.Vertex0 == vertex)
                        {
                            vertex0 = edge.Vertex0;
                            vertex1 = edge.Vertex1;

                            rightCell = edge.RightCell;
                            leftCell = edge.LeftCellStrict;
                        }
                        else // 반대로 연결된 경우, 뒤집음.
                        {
                            vertex0 = edge.Vertex1;
                            vertex1 = edge.Vertex0;

                            rightCell = edge.LeftCellStrict;
                            leftCell = edge.RightCell;
                        }

                        Vector3 v0Point = ScreenToUnity(HexaToScreen(vertex0.Coord));
                        Vector3 v1Point = ScreenToUnity(HexaToScreen(vertex1.Coord));

                        Vector3 rightPoint = ScreenToUnity(HexaToScreen(rightCell.Coord));
                        Vector3 leftPoint = ScreenToUnity(HexaToScreen(leftCell.Coord));

                        Vector3 centerDir = (v1Point - v0Point).normalized;
                        Vector3 rightDir = (rightPoint - v0Point).normalized;
                        Vector3 leftDir = (leftPoint - v0Point).normalized;

                        Vector3 rightMidPoint = v0Point + rightDir * SolidThickness;
                        Vector3 rightEndPoint = rightMidPoint + rightDir * FallOffThickness;

                        Vector3 centerMidPoint = v0Point + centerDir * SolidThickness;
                        Vector3 centerEndPoint = centerMidPoint + centerDir * FallOffThickness;

                        Vector3 leftMidPoint = v0Point + leftDir * SolidThickness;
                        Vector3 leftEndPoint = leftMidPoint + leftDir * FallOffThickness;

                        VertexData v0SolidD = new(v0Point, new Color(1f, 0f, 0f, 1f));

                        VertexData rightMidSolidD = new(rightMidPoint, new Color(1f, 0f, 0f, 1f));
                        VertexData rightMidFalloffD = new(rightMidPoint, new Color(0f, 1f, 0f, 1f));
                        VertexData rightEndFalloffD = new(rightEndPoint, new Color(0f, 1f, 0f, 0f));

                        VertexData centerMidSolidD = new(centerMidPoint, new Color(1f, 0f, 0f, 1f));
                        VertexData centerMidFalloffD = new(centerMidPoint, new Color(0f, 1f, 0f, 1f));
                        VertexData centerEndFalloffD = new(centerEndPoint, new Color(0f, 1f, 0f, 0f));

                        VertexData leftMidSolidD = new(leftMidPoint, new Color(1f, 0f, 0f, 1f));
                        VertexData leftMidFalloffD = new(leftMidPoint, new Color(0f, 1f, 0f, 1f));
                        VertexData leftEndFalloffD = new(leftEndPoint, new Color(0f, 1f, 0f, 0f));

                        collector.AppendQuad(rightEndFalloffD, centerEndFalloffD, rightMidFalloffD, centerMidFalloffD);
                        collector.AppendQuad(centerEndFalloffD, leftEndFalloffD, centerMidFalloffD, leftMidFalloffD);

                        collector.AppendQuad(rightMidSolidD, centerMidSolidD, v0SolidD, leftMidSolidD);
                    }
                }

                result.Add(new ProvinceBorderResult(province, collector));
            }

            return result;
        }

        public List<GameObject> Spawn(IReadOnlyList<ProvinceBorderResult> provinceBorderResults, Transform? parent)
        {
            Material borderMaterial = Assets.ProvinceBorderMaterial;

            List<GameObject> result = new(provinceBorderResults.Count);
            foreach (var boderResult in provinceBorderResults)
            {
                string name = $"Province {boderResult.Province.Id} Border";

                Mesh mesh = boderResult.MeshCollector.ToTrianglesMesh(false, false);
                mesh.name = name;

                GameObject gameObject = new() { name = name, };

                if (parent != null)
                {
                    gameObject.transform.SetParent(parent, false);
                }

                MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;

                MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = borderMaterial;
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;

                result.Add(gameObject);
            }

            return result;
        }

        readonly struct ChunkIndex
        {
            public readonly int X, Y;
            public readonly int BaseGridX, BaseGridY;
            public readonly IEnumerable<HexaIndex> Cells;

            public ChunkIndex(int x, int y, int baseGridX, int baseGridY, IEnumerable<HexaIndex> cells)
            {
                X = x;
                Y = y;
                BaseGridX = baseGridX;
                BaseGridY = baseGridY;
                Cells = cells;
            }
        }

        static IEnumerable<ChunkIndex> EnumerateChunks(int chunkCountX, int chunkCountY)
        {
            static IEnumerable<HexaIndex> Enumerate(int baseGridX, int baseGridY)
            {
                for (int dy = 0; dy < ChunkSize; dy++)
                {
                    int gridY = baseGridY + dy;

                    for (int dx = 0; dx < ChunkSize; dx++)
                    {
                        int gridX = baseGridX + dx;

                        yield return new HexaIndex(gridX, gridY);
                    }
                }
            }

            for (int cy = 0; cy < chunkCountY; cy++)
            {
                int baseGridY = cy * ChunkSize;

                for (int cx = 0; cx < chunkCountX; cx++)
                {
                    int baseGridX = cx * ChunkSize;

                    yield return new ChunkIndex(cx, cy, baseGridX, baseGridY, Enumerate(baseGridX, baseGridY));
                }
            }
        }

        public readonly struct LandChunkResult
        {
            public readonly int ChunkX, ChunkY;
            public readonly int GridX, GridY;
            public readonly IReadOnlyList<WorldCell> Cells;
            public readonly MeshCollector MeshCollector;

            public LandChunkResult(int chunkX, int chunkY, int gridX, int gridY, IReadOnlyList<WorldCell> cells, MeshCollector meshCollector)
            {
                ChunkX = chunkX;
                ChunkY = chunkY;
                GridX = gridX;
                GridY = gridY;
                Cells = cells;
                MeshCollector = meshCollector;
            }
        }

        public readonly struct RoadElementResult
        {
            public readonly Province Province;
            public readonly Tile Tile;
            public readonly WorldCell Cell;
            public readonly RoadBranch Branch;
            public readonly RoadBranch MeshBranch;
            public readonly int CwShiftCount;

            public RoadElementResult(Province province, Tile tile, WorldCell cell, RoadBranch branch, RoadBranch meshBranch, int cwShiftCount)
            {
                Province = province;
                Tile = tile;
                Cell = cell;
                Branch = branch;
                MeshBranch = meshBranch;
                CwShiftCount = cwShiftCount;
            }
        }

        public readonly struct DoodadClusterResult
        {
            public readonly Province Province;
            public readonly DoodadType Type;
            public readonly int Index;
            public readonly IReadOnlyList<Doodad> Doodads;

            public DoodadClusterResult(Province province, DoodadType type, int index, IReadOnlyList<Doodad> doodads)
            {
                Province = province;
                Type = type;
                Index = index;
                Doodads = doodads;
            }
        }

        public readonly struct ProvinceBorderResult
        {
            public readonly Province Province;
            public readonly MeshCollector MeshCollector;

            public ProvinceBorderResult(Province province, MeshCollector meshCollector)
            {
                Province = province;
                MeshCollector = meshCollector;
            }
        }

        const int ChunkSize = 16;
    }
}
