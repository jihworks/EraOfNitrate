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
            foreach (var ci in EnumerateChunks())
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

            VertexData[] cwVerticesBuffer = new VertexData[6];

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

                CollectCellHexagon(cell, meshCollector, biomeColor, cwVerticesBuffer);
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

        public List<OceanChunkResult> BuildOcean()
        {
            if (!World.IsInitialized)
            {
                throw new InvalidOperationException("초기화되지 않은 월드로부터 메시를 빌드할 수 없음.");
            }

            const float ExpandLength = 1000f;

            WorldGrid grid = World.MapGrid;

            List<OceanChunkResult> chunks = new(_chunkCountX * _chunkCountY);
            foreach (var ci in EnumerateChunks())
            {
                List<WorldCell> cells = new(ChunkSize * ChunkSize);

                foreach (var index in ci.Cells)
                {
                    WorldCell? cell = grid.GetCell(index);
                    if (cell is null)
                    {
                        continue;
                    }
                    if (cell.Tile.IsLand) // 바다인지 확인.
                    {
                        continue;
                    }

                    cells.Add(cell);
                }

                if (cells.Count > 0)
                {
                    OceanChunkResult oceanChunk = new(new MeshCollector(AdditionalAttributes.Color));
                    chunks.Add(oceanChunk);

                    CollectCellBasedOceanChunk(Assets, cells, oceanChunk);
                }
            }

            Color oceanColor = Assets.GetOceanColor(false);

            {
                static void Append(MeshCollector collector, HexaVertex westVertex, HexaVertex centerVertex, HexaVertex eastVertex, Vector3 expandDir, float expandLength, Color color, bool flip)
                {
                    Vector3 wPoint = HexaToUnity(westVertex.Coord);
                    Vector3 cPoint = HexaToUnity(centerVertex.Coord);
                    Vector3 ePoint = HexaToUnity(eastVertex.Coord);

                    Vector3 expandDelta = expandDir * expandLength;

                    Vector3 wePoint = wPoint + expandDelta;
                    Vector3 cePoint = cPoint + expandDelta;
                    Vector3 eePoint = ePoint + expandDelta;

                    VertexData wD = new(wPoint, color);
                    VertexData cD = new(cPoint, color);
                    VertexData eD = new(ePoint, color);

                    VertexData weD = new(wePoint, color);
                    VertexData ceD = new(cePoint, color);
                    VertexData eeD = new(eePoint, color);

                    if (flip)
                    {
                        collector.AppendQuad(wD, weD, cD, ceD);
                        collector.AppendQuad(cD, ceD, eD, eeD);
                    }
                    else
                    {
                        collector.AppendQuad(weD, wD, ceD, cD);
                        collector.AppendQuad(ceD, cD, eeD, eD);
                    }
                }

                MeshCollector upperCollector = new(AdditionalAttributes.Color);
                MeshCollector lowerCollector = new(AdditionalAttributes.Color);
                for (int x = 0; x < grid.Width; x++)
                {
                    {
                        WorldCell cell = grid[new HexaIndex(x, 0)];

                        HexaVertex v300 = cell.GetVertex(HexaVertexPosition.D300);
                        HexaVertex v0 = cell.GetVertex(HexaVertexPosition.D0);
                        HexaVertex v60 = cell.GetVertex(HexaVertexPosition.D60);

                        Append(upperCollector, v300, v0, v60, Vector3.forward, ExpandLength, oceanColor, false);
                    }
                    {
                        WorldCell cell = grid[new HexaIndex(x, grid.Height - 1)];

                        HexaVertex v240 = cell.GetVertex(HexaVertexPosition.D240);
                        HexaVertex v180 = cell.GetVertex(HexaVertexPosition.D180);
                        HexaVertex v120 = cell.GetVertex(HexaVertexPosition.D120);

                        Append(lowerCollector, v240, v180, v120, Vector3.back, ExpandLength, oceanColor, true);
                    }
                }
                chunks.Add(new OceanChunkResult(upperCollector));
                chunks.Add(new OceanChunkResult(lowerCollector));
            }
            {
                static void Append(MeshCollector collector, HexaVertex northVertex, HexaVertex southVertex, Vector3 expandDir, float expandLength, Color color, bool flip)
                {
                    Vector3 nPoint = HexaToUnity(northVertex.Coord);
                    Vector3 sPoint = HexaToUnity(southVertex.Coord);

                    Vector3 expandDelta = expandDir * expandLength;

                    Vector3 nePoint = nPoint + expandDelta;
                    Vector3 sePoint = sPoint + expandDelta;

                    VertexData nD = new(nPoint, color);
                    VertexData sD = new(sPoint, color);

                    VertexData neD = new(nePoint, color);
                    VertexData seD = new(sePoint, color);

                    if (flip)
                    {
                        collector.AppendQuad(seD, neD, sD, nD);
                    }
                    else
                    {
                        collector.AppendQuad(neD, seD, nD, sD);
                    }
                }

                MeshCollector leftCollector = new(AdditionalAttributes.Color);
                MeshCollector rightCollector = new(AdditionalAttributes.Color);
                for (int y = 0; y < grid.Height; y++)
                {
                    {
                        WorldCell cell = grid[new HexaIndex(0, y)];

                        HexaVertex v300 = cell.GetVertex(HexaVertexPosition.D300);
                        HexaVertex v240 = cell.GetVertex(HexaVertexPosition.D240);

                        Append(leftCollector, v300, v240, Vector3.left, ExpandLength, oceanColor, false);

                        if (y < grid.Height - 1)
                        {
                            HexaVertex v180 = cell.GetVertex(HexaVertexPosition.D180);

                            Append(leftCollector, v240, v180, Vector3.left, ExpandLength, oceanColor, false);
                        }
                    }
                    {
                        WorldCell cell = grid[new HexaIndex(grid.Width - 1, y)];

                        HexaVertex v60 = cell.GetVertex(HexaVertexPosition.D60);
                        HexaVertex v120 = cell.GetVertex(HexaVertexPosition.D120);

                        Append(rightCollector, v60, v120, Vector3.right, ExpandLength, oceanColor, true);

                        if (y < grid.Height - 1)
                        {
                            HexaVertex v180 = cell.GetVertex(HexaVertexPosition.D180);

                            Append(rightCollector, v120, v180, Vector3.right, ExpandLength, oceanColor, true);
                        }
                    }
                }
                chunks.Add(new OceanChunkResult(leftCollector));
                chunks.Add(new OceanChunkResult(rightCollector));
            }
            {
                static void Append(MeshCollector collector, HexaVertex vertex, Vector3 expandDirX, Vector3 expandDirY, float expandLength, Color color, bool flipX, bool flipY)
                {
                    Vector3 deltaX = expandDirX * expandLength;
                    Vector3 deltaY = expandDirY * expandLength;

                    Vector3 rbPoint = HexaToUnity(vertex.Coord);
                    Vector3 lbPoint = rbPoint + deltaX;
                    Vector3 ltPoint = lbPoint + deltaY;
                    Vector3 rtPoint = ltPoint - deltaX;

                    VertexData ltD = new(ltPoint, color);
                    VertexData rtD = new(rtPoint, color);
                    VertexData lbD = new(lbPoint, color);
                    VertexData rbD = new(rbPoint, color);

                    if (!flipX && !flipY)
                    {
                        collector.AppendQuad(ltD, lbD, rtD, rbD);
                    }
                    else if (flipX && !flipY)
                    {
                        collector.AppendQuad(rtD, rbD, ltD, lbD);
                    }
                    else if (!flipX && flipY)
                    {
                        collector.AppendQuad(lbD, ltD, rbD, rtD);
                    }
                    else
                    {
                        collector.AppendQuad(rbD, rtD, lbD, ltD);
                    }
                }

                MeshCollector ltCollector = new(AdditionalAttributes.Color);
                MeshCollector rtCollector = new(AdditionalAttributes.Color);
                MeshCollector lbCollector = new(AdditionalAttributes.Color);
                MeshCollector rbCollector = new(AdditionalAttributes.Color);
                {
                    HexaCell cell = grid[new HexaIndex(0, 0)];
                    HexaVertex vertex = cell.GetVertex(HexaVertexPosition.D300);
                    Append(ltCollector, vertex, Vector3.left, Vector3.forward, ExpandLength, oceanColor, false, false);
                }
                {
                    HexaCell cell = grid[new HexaIndex(grid.Width - 1, 0)];
                    HexaVertex vertex = cell.GetVertex(HexaVertexPosition.D60);
                    Append(rtCollector, vertex, Vector3.right, Vector3.forward, ExpandLength, oceanColor, true, false);
                }
                {
                    HexaCell cell = grid[new HexaIndex(0, grid.Height - 1)];
                    HexaVertex vertex = cell.GetVertex(HexaVertexPosition.D240);
                    Append(lbCollector, vertex, Vector3.left, Vector3.back, ExpandLength, oceanColor, false, true);
                }
                {
                    HexaCell cell = grid[new HexaIndex(grid.Width - 1, grid.Height - 1)];
                    HexaVertex vertex = cell.GetVertex(HexaVertexPosition.D120);
                    Append(rbCollector, vertex, Vector3.right, Vector3.back, ExpandLength, oceanColor, true, true);
                }
                chunks.Add(new OceanChunkResult(ltCollector));
                chunks.Add(new OceanChunkResult(rtCollector));
                chunks.Add(new OceanChunkResult(lbCollector));
                chunks.Add(new OceanChunkResult(rbCollector));
            }

            return chunks;
        }

        static void CollectCellBasedOceanChunk(Assets assets, IReadOnlyList<WorldCell> cells, in OceanChunkResult chunk)
        {
            MeshCollector meshCollector = chunk.MeshCollector;

            VertexData[] cwVerticesBuffer = new VertexData[6];

            foreach (var cell in cells)
            {
                Tile tile = cell.Tile;
                if (tile.IsLand)
                {
                    throw new InvalidOperationException($"바다가 아닌 타일 {cell.Coord} 로부터 바다 메시를 생성할 수 없음.");
                }

                Color32 oceanColor = assets.GetOceanColor(tile.IsNearOcean);

                CollectCellHexagon(cell, meshCollector, oceanColor, cwVerticesBuffer);
            }
        }

        public List<GameObject> Spawn(IReadOnlyList<OceanChunkResult> oceanResults, Transform? parent)
        {
            Material oceanMaterial = Assets.OceanMaterial;
            Material[] materials = new Material[] { oceanMaterial, };

            List<GameObject> meshObjs = new(oceanResults.Count);

            int number = 1;
            foreach (var chunk in oceanResults)
            {
                string name = $"Ocean Mesh " + number++;

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
                List<RoadElementResult> roadBlocks = new(province.LandTiles.Count);
                result.Add(province, roadBlocks);

                foreach (var tile in province.LandTiles)
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
                    Vector3 unityLocation = HexaToUnity(coord);

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
                p => p.LandTiles.SelectMany(
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

            const float GapThickness = 0.03f;
            const float OuterSolidThickness = 0.03f;
            const float InnerSolidThickness = 0.01f;
            const float InnerFallOffThickness = 0.04f;

            List<ProvinceBorderResult> result = new(World.Provinces.Count);
            foreach (var province in World.Provinces)
            {
                List<(WorldCell Cell, HexaEdge Edge)> borderEdges = new();
                foreach (var tile in province.LandTiles.Concat(province.OceanTiles))
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
                // R = 외부선 컬러, G = 내부선 컬러.
                Color outerSolidColor = new(1f, 0f, 0f, 1f);
                Color innerSolidColor = new(0f, 1f, 0f, 1f);
                Color innerFalloffColor = new(0f, 1f, 0f, 0f);
                foreach (var (cell, edge) in borderEdges)
                {
                    Vector3 cellCenter = HexaToUnity(cell.Coord);

                    edge.GetCwOrder(cell, out HexaVertex vertex0, out HexaVertex vertex1);
                    Vector3 v0Point = HexaToUnity(vertex0.Coord);
                    Vector3 v1Point = HexaToUnity(vertex1.Coord);

                    Vector3 v0ToCenterDir = (cellCenter - v0Point).normalized;
                    Vector3 v1ToCenterDir = (cellCenter - v1Point).normalized;

                    Vector3 outerStart0 = v0Point + v0ToCenterDir * GapThickness;
                    Vector3 outerStart1 = v1Point + v1ToCenterDir * GapThickness;

                    Vector3 outerEnd0 = outerStart0 + v0ToCenterDir * OuterSolidThickness;
                    Vector3 outerEnd1 = outerStart1 + v1ToCenterDir * OuterSolidThickness;

                    Vector3 innerMid0 = outerEnd0 + v0ToCenterDir * InnerSolidThickness;
                    Vector3 innerMid1 = outerEnd1 + v1ToCenterDir * InnerSolidThickness;

                    Vector3 innerEnd0 = innerMid0 + v0ToCenterDir * InnerFallOffThickness;
                    Vector3 innerEnd1 = innerMid1 + v1ToCenterDir * InnerFallOffThickness;

                    VertexData outerStart0D = new(outerStart0, outerSolidColor);
                    VertexData outerStart1D = new(outerStart1, outerSolidColor);

                    VertexData outerEnd0D = new(outerEnd0, outerSolidColor);
                    VertexData outerEnd1D = new(outerEnd1, outerSolidColor);

                    VertexData innerStart0D = new(outerEnd0, innerSolidColor);
                    VertexData innerStart1D = new(outerEnd1, innerSolidColor);

                    VertexData innerMid0D = new(innerMid0, innerSolidColor);
                    VertexData innerMid1D = new(innerMid1, innerSolidColor);

                    VertexData innerEnd0D = new(innerEnd0, innerFalloffColor);
                    VertexData innerEnd1D = new(innerEnd1, innerFalloffColor);

                    collector.AppendQuad(innerEnd0D, innerEnd1D, innerMid0D, innerMid1D);
                    collector.AppendQuad(innerMid0D, innerMid1D, innerStart0D, innerStart1D);

                    collector.AppendQuad(outerEnd0D, outerEnd1D, outerStart0D, outerStart1D);
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

                        Vector3 v0Point = HexaToUnity(vertex0.Coord);
                        Vector3 v1Point = HexaToUnity(vertex1.Coord);

                        Vector3 rightPoint = HexaToUnity(rightCell.Coord);
                        Vector3 leftPoint = HexaToUnity(leftCell.Coord);

                        Vector3 centerDir = (v1Point - v0Point).normalized;
                        Vector3 rightDir = (rightPoint - v0Point).normalized;
                        Vector3 leftDir = (leftPoint - v0Point).normalized;

                        Vector3 outerStartR = v0Point + rightDir * GapThickness;
                        Vector3 outerStartC = v0Point + centerDir * GapThickness;
                        Vector3 outerStartL = v0Point + leftDir * GapThickness;

                        Vector3 outerEndR = outerStartR + rightDir * OuterSolidThickness;
                        Vector3 outerEndC = outerStartC + centerDir * OuterSolidThickness;
                        Vector3 outerEndL = outerStartL + leftDir * OuterSolidThickness;

                        Vector3 innerMidR = outerEndR + rightDir * InnerSolidThickness;
                        Vector3 innerMidC = outerEndC + centerDir * InnerSolidThickness;
                        Vector3 innerMidL = outerEndL + leftDir * InnerSolidThickness;

                        Vector3 innerEndR = innerMidR + rightDir * InnerFallOffThickness;
                        Vector3 innerEndC = innerMidC + centerDir * InnerFallOffThickness;
                        Vector3 innerEndL = innerMidL + leftDir * InnerFallOffThickness;

                        VertexData outerStartRD = new(outerStartR, outerSolidColor);
                        VertexData outerStartCD = new(outerStartC, outerSolidColor);
                        VertexData outerStartLD = new(outerStartL, outerSolidColor);

                        VertexData outerEndRD = new(outerEndR, outerSolidColor);
                        VertexData outerEndCD = new(outerEndC, outerSolidColor);
                        VertexData outerEndLD = new(outerEndL, outerSolidColor);

                        VertexData innerStartRD = new(outerEndR, innerSolidColor);
                        VertexData innerStartCD = new(outerEndC, innerSolidColor);
                        VertexData innerStartLD = new(outerEndL, innerSolidColor);

                        VertexData innerMidRD = new(innerMidR, innerSolidColor);
                        VertexData innerMidCD = new(innerMidC, innerSolidColor);
                        VertexData innerMidLD = new(innerMidL, innerSolidColor);

                        VertexData innerEndRD = new(innerEndR, innerFalloffColor);
                        VertexData innerEndCD = new(innerEndC, innerFalloffColor);
                        VertexData innerEndLD = new(innerEndL, innerFalloffColor);

                        collector.AppendQuad(innerEndRD, innerEndCD, innerMidRD, innerMidCD);
                        collector.AppendQuad(innerMidRD, innerMidCD, innerStartRD, innerStartCD);
                        collector.AppendQuad(outerEndRD, outerEndCD, outerStartRD, outerStartCD);

                        collector.AppendQuad(innerEndCD, innerEndLD, innerMidCD, innerMidLD);
                        collector.AppendQuad(innerMidCD, innerMidLD, innerStartCD, innerStartLD);
                        collector.AppendQuad(outerEndCD, outerEndLD, outerStartCD, outerStartLD);
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

        IEnumerable<ChunkIndex> EnumerateChunks()
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

            for (int cy = 0; cy < _chunkCountY; cy++)
            {
                int baseGridY = cy * ChunkSize;

                for (int cx = 0; cx < _chunkCountX; cx++)
                {
                    int baseGridX = cx * ChunkSize;

                    yield return new ChunkIndex(cx, cy, baseGridX, baseGridY, Enumerate(baseGridX, baseGridY));
                }
            }
        }

        static void CollectCellHexagon(WorldCell cell, MeshCollector collector, Color color, VertexData[] cwVerticesBuffer)
        {
            Vector3 centerLocation = HexaToUnity(cell.Coord);

            VertexData center = new(centerLocation, color);

            for (int v = 0; v < 6; v++)
            {
                HexaVertex vertex = cell.GetVertex((HexaVertexPosition)v);

                Vector3 vertexLocation = HexaToUnity(vertex.Coord);

                cwVerticesBuffer[v] = new VertexData(vertexLocation, color);
            }

            collector.AppendNGon(center, cwVerticesBuffer);
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

        public readonly struct OceanChunkResult
        {
            public readonly MeshCollector MeshCollector;

            public OceanChunkResult(MeshCollector meshCollector)
            {
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
