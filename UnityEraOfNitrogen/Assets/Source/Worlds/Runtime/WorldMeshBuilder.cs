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

        public List<LandChunk> BuildLand()
        {
            if (!World.IsInitialized)
            {
                throw new InvalidOperationException("초기화되지 않은 월드로부터 메시를 빌드할 수 없음.");
            }

            WorldGrid grid = World.MapGrid;

            List<LandChunk> chunks = new(_chunkCountX * _chunkCountY);
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
                    chunks.Add(new LandChunk(ci.X, ci.Y, ci.BaseGridX, ci.BaseGridY, cells, new MeshCollector(AdditionalAttributes.Color)));
                }
            }

            foreach (var chunk in chunks)
            {
                CollectLandChunk(Assets, chunk);
            }

            return chunks;
        }

        static void CollectLandChunk(Assets assets, in LandChunk chunk)
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

                    Vector3 unityVertexOffset = ScreenToUnity(HexaToScreen(vertex.Coord));

                    cwCornersBuffer[v] = new VertexData(unityCenterLocation + unityVertexOffset, biomeColor);
                }

                meshCollector.AppendNGon(center, cwCornersBuffer);
            }
        }

        public List<GameObject> Spawn(IReadOnlyList<LandChunk> chunks, Transform? parent)
        {
            Material landMaterial = Assets.LandMaterial;
            Material[] materials = new Material[] { landMaterial, };

            List<GameObject> meshObjs = new(chunks.Count);

            foreach (var chunk in chunks)
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

        public Dictionary<Province, List<RoadBlock>> BuildRoads()
        {
            if (!World.IsInitialized)
            {
                throw new InvalidOperationException("초기화되지 않은 월드로부터 도로를 빌드할 수 없음.");
            }

            Dictionary<Province, List<RoadBlock>> result = new(World.Provinces.Count);

            foreach (var province in World.Provinces)
            {
                List<RoadBlock> roadBlocks = new(province.Tiles.Count);
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
                        roadBlocks.Add(new RoadBlock(province, tile, tile.Cell, branch, branch, 0));
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

                    roadBlocks.Add(new RoadBlock(province, tile, tile.Cell, branch, meshBranch, cwShiftCount));
                }
            }

            return result;
        }

        public List<RoadElement> Spawn(KeyValuePair<Province, List<RoadBlock>> roadBlocks, Transform? parent)
        {
            List<RoadElement> result = new(roadBlocks.Value.Count);

            RoadAssets assets = Assets.GetRoadAssets(RoadLevel.Dirt/*TODO: 테스트 용 도로 레벨.*/);

            Material material = assets.Material;
            Material[] materials = new Material[] { material, };

            Texture2D mainTexture = assets.MainTexture;

            MaterialPropertyBlock propertyBlock = new();
            propertyBlock.SetTexture(ShaderIds.MainTexure, mainTexture);

            foreach (var roadBlock in roadBlocks.Value)
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
                    collisionShape.Freeze();
                }

                RoadElement roadElement = new(roadBlock.Tile, new List<GameObject>() { meshObj, }, collisionShape);
                result.Add(roadElement);

                roadBlock.Tile.Spawned(roadElement);
            }

            return result;
        }

        public List<DoodadGroup> BuildDoodads()
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

            List<DoodadGroup> result = new(World.Provinces.Count * 3);

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

                        result.Add(new DoodadGroup(provinceGroup.Key, typeGroup.Key, indexGroup.Key, doodads));
                    }
                }
            }

            return result;
        }

        public List<DoodadCluster> Spawn(IReadOnlyList<DoodadGroup> doodadGroups)
        {
            List<DoodadCluster> result = new(doodadGroups.Count);

            foreach (var group in doodadGroups)
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

        public readonly struct LandChunk
        {
            public readonly int ChunkX, ChunkY;
            public readonly int GridX, GridY;
            public readonly IReadOnlyList<WorldCell> Cells;
            public readonly MeshCollector MeshCollector;

            public LandChunk(int chunkX, int chunkY, int gridX, int gridY, IReadOnlyList<WorldCell> cells, MeshCollector meshCollector)
            {
                ChunkX = chunkX;
                ChunkY = chunkY;
                GridX = gridX;
                GridY = gridY;
                Cells = cells;
                MeshCollector = meshCollector;
            }
        }

        public readonly struct RoadBlock
        {
            public readonly Province Province;
            public readonly Tile Tile;
            public readonly WorldCell Cell;
            public readonly RoadBranch Branch;
            public readonly RoadBranch MeshBranch;
            public readonly int CwShiftCount;

            public RoadBlock(Province province, Tile tile, WorldCell cell, RoadBranch branch, RoadBranch meshBranch, int cwShiftCount)
            {
                Province = province;
                Tile = tile;
                Cell = cell;
                Branch = branch;
                MeshBranch = meshBranch;
                CwShiftCount = cwShiftCount;
            }
        }

        public readonly struct DoodadGroup
        {
            public readonly Province Province;
            public readonly DoodadType Type;
            public readonly int Index;
            public readonly IReadOnlyList<Doodad> Doodads;

            public DoodadGroup(Province province, DoodadType type, int index, IReadOnlyList<Doodad> doodads)
            {
                Province = province;
                Type = type;
                Index = index;
                Doodads = doodads;
            }
        }

        const int ChunkSize = 16;
    }
}
