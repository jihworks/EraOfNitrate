// © 2026 Jong-il Hong
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>. 
//
// SPDX-License-Identifier: GPL-3.0-or-later

#nullable enable

using Jih.Unity.EraOfNitrogen.Worlds;
using Jih.Unity.Infrastructure;
using Jih.Unity.Infrastructure.Geometries;
using Jih.Unity.Infrastructure.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jih.Unity.EraOfNitrogen
{
    public class Assets : MonoBehaviour
    {
        static SingletonStorage<Assets> _instance;
        public static Assets Instance => _instance.Get();

        [Header("Terrain")]
        [SerializeField] Color32 _grasslandColor = new(0x91, 0xB9, 0x5B, 0xFF);
        [SerializeField] Color32 _rainforestColor = new(0x3B, 0x5D, 0x38, 0xFF);
        [SerializeField] Color32 _tundraColor = new(0xD0, 0xD7, 0xD9, 0xFF);
        [SerializeField] Color32 _steppeColor = new(0xC4, 0xA4, 0x6B, 0xFF);
        [SerializeField] Color32 _desertColor = new(0xEB, 0xC0, 0x7A, 0xFF);
        [SerializeField] Color32 _snowColor = new(0xF2, 0xF2, 0xF2, 0xFF);

        [Space(12f)]
        [SerializeField] Color32 _nearOceanColor = new(0x59, 0xC9, 0xD5, 0xFF);
        [SerializeField] Color32 _farOceanColor = new(0x2C, 0x4A, 0x73, 0xFF);

        [Space(12f)]
        [SerializeField] Material? _landMaterial;
        public Material LandMaterial => _landMaterial.ThrowIfNull(nameof(LandMaterial));

        [SerializeField] Material? _oceanMaterial;
        public Material OceanMaterial => _oceanMaterial.ThrowIfNull(nameof(OceanMaterial));

        [Header("Doodad")]
        [SerializeField] DoodadAssets[] _doodads = Array.Empty<DoodadAssets>();

        [Header("Road")]
        [SerializeField] RoadAssets[] _roads = Array.Empty<RoadAssets>();

        public Color32 GetColor(Biome biome)
        {
            return biome switch
            {
                Biome.Grassland => _grasslandColor,
                Biome.Rainforest => _rainforestColor,
                Biome.Tundra => _tundraColor,
                Biome.Steppe => _steppeColor,
                Biome.Desert => _desertColor,
                Biome.Snow => _snowColor,
                _ => throw new NotImplementedException(),
            };
        }

        public Color32 GetOceanColor(bool isNearOcean)
        {
            return isNearOcean ? _nearOceanColor : _farOceanColor;
        }

        public DoodadAssets GetDoodadAssets(DoodadType type)
        {
            return _doodads[type.ToIndex()];
        }

        public RoadAssets GetRoadAssets(RoadLevel level)
        {
            return _roads[level.ToIndex()];
        }

        public Assets()
        {
            _instance = new SingletonStorage<Assets>(this);
        }
    }

    [Serializable]
    public class DoodadAssets
    {
        [SerializeField] Mesh?[] _meshes = Array.Empty<Mesh?>();
        [SerializeField] SerializableMesh?[] _convexHulls = Array.Empty<SerializableMesh?>();

        [SerializeField] Material? _material;
        public Material Material => _material.ThrowIfNull(nameof(Material));

        public Mesh GetMesh(int index, out SerializableMesh convexHull)
        {
            convexHull = _convexHulls[index].ThrowIfNull(nameof(GetMesh));
            return _meshes[index].ThrowIfNull(nameof(GetMesh));
        }

        public int VariantToIndex(int variant)
        {
            return variant % _meshes.Length;
        }
    }

    [Serializable]
    public class RoadAssets
    {
        [SerializeField] float _meshBaseZRotation = 0f;
        public float MeshBaseZRotation => _meshBaseZRotation;
        [SerializeField] Material? _material;
        public Material Material => _material.ThrowIfNull(nameof(Material));
        [SerializeField] Texture2D? _mainTexture;
        public Texture2D MainTexture => _mainTexture.ThrowIfNull(nameof(MainTexture));

        [Space(12f)]
        [SerializeField] Mesh? _meshNone;
        [SerializeField] Mesh? _mesh0;
        [SerializeField] Mesh? _mesh01;
        [SerializeField] Mesh? _mesh02;
        [SerializeField] Mesh? _mesh03;
        [SerializeField] Mesh? _mesh012;
        [SerializeField] Mesh? _mesh013;
        [SerializeField] Mesh? _mesh024;
        [SerializeField] Mesh? _mesh035;
        [SerializeField] Mesh? _mesh0123;
        [SerializeField] Mesh? _mesh0234;
        [SerializeField] Mesh? _mesh0235;
        [SerializeField] Mesh? _mesh01234;
        [SerializeField] Mesh? _mesh012345;

        Dictionary<RoadBranch, (Mesh Mesh, MeshCollector Collector)>? _map;

        public Mesh GetMesh(RoadBranch branch, out MeshCollector collector)
        {
            if (_map is null)
            {
                _map = new Dictionary<RoadBranch, (Mesh Mesh, MeshCollector Collector)>(14);

                void Add(RoadBranch key, Mesh? mesh)
                {
                    mesh.ThrowIfNull(out Mesh meshN, $"Road Mesh {key}");

                    MeshCollector collector = MeshCollector.Load(meshN, AdditionalAttributes.None);

                    _map.Add(key, (meshN, collector));
                }
                Add(RoadBranch.None, _meshNone);
                Add(RoadBranch.Branch0, _mesh0);
                Add(RoadBranch.Branch01, _mesh01);
                Add(RoadBranch.Branch02, _mesh02);
                Add(RoadBranch.Branch03, _mesh03);
                Add(RoadBranch.Branch012, _mesh012);
                Add(RoadBranch.Branch013, _mesh013);
                Add(RoadBranch.Branch024, _mesh024);
                Add(RoadBranch.Branch035, _mesh035);
                Add(RoadBranch.Branch0123, _mesh0123);
                Add(RoadBranch.Branch0234, _mesh0234);
                Add(RoadBranch.Branch0235, _mesh0235);
                Add(RoadBranch.Branch01234, _mesh01234);
                Add(RoadBranch.Branch012345, _mesh012345);
            }

            var (mesh, resultCollector) = _map[branch];

            collector = resultCollector;
            return mesh;
        }
    }
}
