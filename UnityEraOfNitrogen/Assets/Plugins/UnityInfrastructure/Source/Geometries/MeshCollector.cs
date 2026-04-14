// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using static Codice.Client.Common.EventTracking.TrackFeatureUseEvent.Features.DesktopGUI.Filters;

namespace Jih.Unity.Infrastructure.Geometries
{
    /// <remarks>
    /// <b>NOT</b> thread-safe.<br/>
    /// <br/>
    /// Currently, focusing on vertex attributes only. The bindposes are not supported.<br/>
    /// <br/>
    /// There is <c>NO</c> coordinate conversion. The class will handle the data as-is.
    /// </remarks>
    public class MeshCollector
    {
        public enum LoadImperfectStrategy
        {
            RemoveAndUnsetFlag,
            ThrowException,
            FillWithFallback,
        }

        public enum AppendSubMeshStrategy
        {
            MatchedOnly,
            AddSubMeshes,
            ThrowException,
        }

        public static MeshCollector Load(Mesh mesh, AdditionalAttributes additionalAttributes, LoadImperfectStrategy imperfectStrategy = LoadImperfectStrategy.RemoveAndUnsetFlag, VertexData fallback = default, int subMeshLod = 0)
        {
            if (!mesh.isReadable)
            {
                throw new ArgumentException($"The mesh '{mesh.name}' is not readable but trying to load.");
            }
            if (subMeshLod < 0 || mesh.lodCount <= subMeshLod)
            {
                throw new ArgumentOutOfRangeException(nameof(subMeshLod));
            }

            int subMeshCount = mesh.subMeshCount;
            if (subMeshCount <= 0)
            {
                throw new ArgumentException($"The mesh '{mesh.name} has not any sub-mesh to load.");
            }

            int vertexCount = mesh.vertexCount;

            List<Vector3> positions = new(vertexCount);
            mesh.GetVertices(positions);

            if (positions.Count < vertexCount)
            {
                switch (imperfectStrategy)
                {
                    case LoadImperfectStrategy.RemoveAndUnsetFlag:
                    case LoadImperfectStrategy.ThrowException:
                        throw new InvalidOperationException($"Mesh '{mesh.name}' has imperfect positions.");

                    case LoadImperfectStrategy.FillWithFallback:
                        positions.AddMultiple(fallback.Position, vertexCount - positions.Count);
                        break;

                    default: throw new NotImplementedException();
                }
            }

            List<Color>? colors = null;
            if (additionalAttributes.Has(AdditionalAttributes.Color))
            {
                colors = new List<Color>(vertexCount);
                mesh.GetColors(colors);

                if (colors.Count < vertexCount)
                {
                    switch (imperfectStrategy)
                    {
                        case LoadImperfectStrategy.RemoveAndUnsetFlag:
                            colors = null;
                            additionalAttributes = additionalAttributes.Unset(AdditionalAttributes.Color);
                            break;

                        case LoadImperfectStrategy.ThrowException:
                            throw new InvalidOperationException($"Mesh '{mesh.name}' has imperfect colors.");

                        case LoadImperfectStrategy.FillWithFallback:
                            colors.AddMultiple(fallback.Color, vertexCount - colors.Count);
                            break;

                        default: throw new NotImplementedException();
                    }
                }
            }

            UVSet?[] uvSets = new UVSet[MaxUVSetCount];
            for (int u = 0; u < MaxUVSetCount; u++)
            {
                AdditionalAttributes uvSetFlag = (AdditionalAttributes)((uint)AdditionalAttributes.UV0 << u);
                if (!additionalAttributes.Has(uvSetFlag))
                {
                    continue;
                }

                UVSet uvSet = new(vertexCount);
                uvSets[u] = uvSet;

                List<Vector2> texCoords = uvSet._texCoords;
                mesh.GetUVs(u, texCoords);

                if (texCoords.Count < vertexCount)
                {
                    switch (imperfectStrategy)
                    {
                        case LoadImperfectStrategy.RemoveAndUnsetFlag:
                            uvSets[u] = null;
                            additionalAttributes = additionalAttributes.Unset(uvSetFlag);
                            break;

                        case LoadImperfectStrategy.ThrowException:
                            throw new InvalidOperationException($"Mesh '{mesh.name}' has imperfect UV channel {u}.");

                        case LoadImperfectStrategy.FillWithFallback:
                            texCoords.AddMultiple(fallback.GetUV(u), vertexCount - texCoords.Count);
                            break;

                        default: throw new NotImplementedException();
                    }
                }
            }

            List<Vector3>? normals = null;
            if (additionalAttributes.Has(AdditionalAttributes.Normal))
            {
                normals = new List<Vector3>(vertexCount);
                mesh.GetNormals(normals);

                if (normals.Count < vertexCount)
                {
                    switch (imperfectStrategy)
                    {
                        case LoadImperfectStrategy.RemoveAndUnsetFlag:
                            normals = null;
                            additionalAttributes = additionalAttributes.Unset(AdditionalAttributes.Normal);
                            break;

                        case LoadImperfectStrategy.ThrowException:
                            throw new InvalidOperationException($"Mesh '{mesh.name}' has imperfect normals.");

                        case LoadImperfectStrategy.FillWithFallback:
                            normals.AddMultiple(fallback.Normal, vertexCount - normals.Count);
                            break;

                        default: throw new NotImplementedException();
                    }
                }
            }

            List<Vector4>? tangents = null;
            if (additionalAttributes.Has(AdditionalAttributes.Tangent))
            {
                tangents = new List<Vector4>(vertexCount);
                mesh.GetTangents(tangents);

                if (tangents.Count < vertexCount)
                {
                    switch (imperfectStrategy)
                    {
                        case LoadImperfectStrategy.RemoveAndUnsetFlag:
                            tangents = null;
                            additionalAttributes = additionalAttributes.Unset(AdditionalAttributes.Tangent);
                            break;

                        case LoadImperfectStrategy.ThrowException:
                            throw new InvalidOperationException($"Mesh '{mesh.name}' has imperfect tangents.");

                        case LoadImperfectStrategy.FillWithFallback:
                            tangents.AddMultiple(fallback.Tangent, vertexCount - tangents.Count);
                            break;

                        default: throw new NotImplementedException();
                    }
                }
            }

            List<IReadOnlyList<BoneWeight1>>? boneWeights = null;
            if (additionalAttributes.Has(AdditionalAttributes.BoneWeight))
            {
                boneWeights = new List<IReadOnlyList<BoneWeight1>>(vertexCount);

                NativeArray<byte> srcCounts = mesh.GetBonesPerVertex();
                NativeArray<BoneWeight1> srcWeights = mesh.GetAllBoneWeights();

                int srcBoneCount = 0;
                for (int i = 0; i < srcCounts.Length; i++)
                {
                    srcBoneCount += srcCounts[i];
                }

                if (srcCounts.Length < vertexCount || srcWeights.Length < srcBoneCount)
                {
                    switch (imperfectStrategy)
                    {
                        case LoadImperfectStrategy.RemoveAndUnsetFlag:
                            boneWeights = null;
                            additionalAttributes = additionalAttributes.Unset(AdditionalAttributes.BoneWeight);
                            break;

                        case LoadImperfectStrategy.ThrowException:
                            throw new InvalidOperationException($"Mesh '{mesh.name}' has imperfect bone weights.");

                        case LoadImperfectStrategy.FillWithFallback:
                            boneWeights.AddMultiple(fallback.BoneWeightList ?? Array.Empty<BoneWeight1>(), vertexCount - boneWeights.Count);
                            break;

                        default: throw new NotImplementedException();
                    }
                }
                else
                {
                    int w = 0;
                    for (int i = 0; i < vertexCount; i++)
                    {
                        int boneCount = srcCounts[i];

                        List<BoneWeight1> weightList = new(boneCount);
                        boneWeights.Add(weightList);

                        for (int j = 0; j < boneCount; j++)
                        {
                            weightList.Add(srcWeights[w++]);
                        }
                    }
                }
            }

            List<SubMesh> subMeshes = new(subMeshCount);

            for (int s = 0; s < subMeshCount; s++)
            {
                SubMeshDescriptor descriptor = mesh.GetSubMesh(s);
                // Need not to check topology.

                SubMesh subMesh = new(descriptor.indexCount);
                subMeshes.Add(subMesh);

                mesh.GetIndices(subMesh._indices, s, subMeshLod, applyBaseVertex: true);
            }

            return new MeshCollector(additionalAttributes, positions, colors, uvSets, normals, tangents, boneWeights, subMeshes, 0);
        }

        /// <summary>
        /// Flags representing vertex attributes to collect other than positions.
        /// </summary>
        public AdditionalAttributes AdditionalAttributes { get; }

        readonly List<Vector3> _positions;
        public IReadOnlyList<Vector3> Positions => _positions;

        readonly List<Color>? _colors;
        public IReadOnlyList<Color>? Colors => _colors;

        readonly UVSet?[] _uvSets = new UVSet?[MaxUVSetCount];
        public IReadOnlyList<UVSet?> UVSets => _uvSets;

        readonly List<Vector3>? _normals;
        public IReadOnlyList<Vector3>? Normals => _normals;

        readonly List<Vector4>? _tangents;
        public IReadOnlyList<Vector4>? Tangents => _tangents;

        readonly List<IReadOnlyList<BoneWeight1>>? _boneWeightLists;
        public IReadOnlyList<IReadOnlyList<BoneWeight1>>? BoneWeightLists => _boneWeightLists;

        readonly List<SubMesh> _subMeshes = new();
        public IReadOnlyList<SubMesh> SubMeshes => _subMeshes;

        public int CurrentSubMeshIndex { get; private set; }
        public SubMesh CurrentSubMesh => _subMeshes[CurrentSubMeshIndex];

        private MeshCollector(AdditionalAttributes additionalAttributes, List<Vector3> positions, List<Color>? colors, UVSet?[] uvSets, List<Vector3>? normals, List<Vector4>? tangents, List<IReadOnlyList<BoneWeight1>>? boneWeightLists, List<SubMesh> subMeshes, int currentSubMeshIndex)
        {
            AdditionalAttributes = additionalAttributes;
            _positions = positions;
            _colors = colors;
            _uvSets = uvSets;
            _normals = normals;
            _tangents = tangents;
            _boneWeightLists = boneWeightLists;
            _subMeshes = subMeshes;
            CurrentSubMeshIndex = currentSubMeshIndex;
        }

        /// <param name="additionalAttributes">Flags representing vertex attributes to collect other than positions.</param>
        /// <param name="subMeshCount">At least 1.</param>
        /// <param name="vertexCapacity">Initial capacity of vertex attribute buffers. The buffers are <c>NOT</c> fixed size.</param>
        /// <param name="indexCapacity">Initial capacity of index buffer of sub-meshes. The buffers are <c>NOT</c> fixed size.</param>
        public MeshCollector(AdditionalAttributes additionalAttributes, int subMeshCount = 1, int vertexCapacity = 3, int indexCapacity = 3)
        {
            if (vertexCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(vertexCapacity), "Vertex capacity cannot be negative.");
            }
            if (indexCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(indexCapacity), "Index capacity cannot be negative.");
            }
            if (subMeshCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(subMeshCount), "Sub mesh count cannot be less than 1.");
            }

            AdditionalAttributes = additionalAttributes;

            _positions = new List<Vector3>(vertexCapacity);
            if (additionalAttributes.Has(AdditionalAttributes.Color))
            {
                _colors = new List<Color>(vertexCapacity);
            }
            for (int i = 0; i < MaxUVSetCount; i++)
            {
                AdditionalAttributes checkingFlag = (AdditionalAttributes)((uint)AdditionalAttributes.UV0 << i);

                if (additionalAttributes.Has(checkingFlag))
                {
                    _uvSets[i] = new UVSet(vertexCapacity);
                }
            }
            if (additionalAttributes.Has(AdditionalAttributes.Normal))
            {
                _normals = new List<Vector3>(vertexCapacity);
            }
            if (additionalAttributes.Has(AdditionalAttributes.Tangent))
            {
                _tangents = new List<Vector4>(vertexCapacity);
            }
            if (additionalAttributes.Has(AdditionalAttributes.BoneWeight))
            {
                _boneWeightLists = new List<IReadOnlyList<BoneWeight1>>(vertexCapacity);
            }

            for (int i = 0; i < subMeshCount; i++)
            {
                _subMeshes.Add(new SubMesh(indexCapacity));
            }

            CurrentSubMeshIndex = 0;
        }

        public void SetCurrentSubMeshIndex(int index)
        {
            if (index < 0 || _subMeshes.Count <= index)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            CurrentSubMeshIndex = index;
        }

        public void AddSubMesh(int indexCapacity = 3, bool makeCurrent = true)
        {
            if (indexCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(indexCapacity), "Index capacity cannot be negative.");
            }

            _subMeshes.Add(new SubMesh(indexCapacity));

            if (makeCurrent)
            {
                CurrentSubMeshIndex = _subMeshes.Count - 1;
            }
        }
        public void RemoveSubMesh(int index)
        {
            if (index < 0 || _subMeshes.Count <= index)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            _subMeshes.RemoveAt(index);
        }

        public void EditPositions(int startIndex, int count, EditDelegate<Vector3> edit)
        {
            EditImpl("positions", _positions, startIndex, count, edit, null);
        }
        public void EditColors(int startIndex, int count, EditDelegate<Color> edit)
        {
            EditImpl("colors", _colors, startIndex, count, edit, null);
        }
        public void EditNormals(int startIndex, int count, EditDelegate<Vector3> edit)
        {
            EditImpl("normals", _normals, startIndex, count, edit, null);
        }
        public void EditTangents(int startIndex, int count, EditDelegate<Vector4> edit)
        {
            EditImpl("tangents", _tangents, startIndex, count, edit, null);
        }
        public void EditBoneWeights(int startIndex, int count, EditDelegate<IReadOnlyList<BoneWeight1>> edit)
        {
            EditImpl("bone weights", _boneWeightLists, startIndex, count, edit, list => list ?? Array.Empty<BoneWeight1>());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void EditImpl<T>(string context, List<T>? targetList, int index, int count, EditDelegate<T> edit, Func<T, T>? secureValue)
        {
            if (targetList is null)
            {
                throw new InvalidOperationException($"Cannot edit {context}. Because it is not collecting attribute.");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0 || targetList.Count < index + count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            for (int i = 0; i < count; i++)
            {
                int currIndex = index + i;
                if (!edit(currIndex, targetList[currIndex], out T result))
                {
                    break;
                }
                targetList[currIndex] = secureValue is not null ? secureValue(result) : result;
            }
        }

        public void Append(MeshCollector other, AppendSubMeshStrategy subMeshStrategy = AppendSubMeshStrategy.MatchedOnly, VertexData fallback = default)
        {
            if (other == this)
            {
                throw new InvalidOperationException("Cannot append itself.");
            }

            int otherVertexCount = other._positions.Count;

            int baseIndex = _positions.Count;
            _positions.AddAll(other._positions);

            if (_colors is not null)
            {
                if (other._colors is not null)
                {
                    _colors.AddAll(other._colors);
                }
                else
                {
                    _colors.AddMultiple(fallback.Color, otherVertexCount);
                }
            }

            for (int u = 0; u < MaxUVSetCount; u++)
            {
                UVSet? destUVSet = _uvSets[u];
                if (destUVSet is null)
                {
                    continue;
                }

                List<Vector2> destTexCoords = destUVSet._texCoords;

                UVSet? srcUVSet = other._uvSets[u];
                if (srcUVSet is not null)
                {
                    destTexCoords.AddAll(srcUVSet._texCoords);
                }
                else
                {
                    destTexCoords.AddMultiple(fallback.GetUV(u), otherVertexCount);
                }
            }

            if (_normals is not null)
            {
                if (other._normals is not null)
                {
                    _normals.AddAll(other._normals);
                }
                else
                {
                    _normals.AddMultiple(fallback.Normal, otherVertexCount);
                }
            }

            if (_tangents is not null)
            {
                if (other._tangents is not null)
                {
                    _tangents.AddAll(other._tangents);
                }
                else
                {
                    _tangents.AddMultiple(fallback.Tangent, otherVertexCount);
                }
            }

            if (_boneWeightLists is not null)
            {
                if (other._boneWeightLists is not null)
                {
                    _boneWeightLists.AddAll(other._boneWeightLists);
                }
                else
                {
                    _boneWeightLists.AddMultiple(fallback.BoneWeightList ?? Array.Empty<BoneWeight1>(), otherVertexCount);
                }
            }

            if (subMeshStrategy is AppendSubMeshStrategy.AddSubMeshes)
            {
                foreach (var srcSubMesh in other.SubMeshes)
                {
                    List<int> srcIndices = srcSubMesh._indices;

                    SubMesh destSubMesh = new(srcIndices.Count);
                    _subMeshes.Add(destSubMesh);

                    List<int> destIndices = destSubMesh._indices;
                    for (int x = 0; x < srcIndices.Count; x++)
                    {
                        destIndices.Add(baseIndex + srcIndices[x]);
                    }
                }
            }
            else
            {
                for (int s = 0; s < _subMeshes.Count; s++)
                {
                    SubMesh destSubMesh = _subMeshes[s];

                    if (s < other._subMeshes.Count)
                    {
                        SubMesh srcSubMesh = other._subMeshes[s];
                        List<int> srcIndices = srcSubMesh._indices;

                        List<int> destIndices = destSubMesh._indices;
                        destIndices.SecureCapacity(destIndices.Count + srcIndices.Count);

                        for (int x = 0; x < srcIndices.Count; x++)
                        {
                            destIndices.Add(baseIndex + srcIndices[x]);
                        }
                        continue;
                    }

                    if (subMeshStrategy is AppendSubMeshStrategy.MatchedOnly)
                    {
                        break;
                    }
                    else if (subMeshStrategy is AppendSubMeshStrategy.ThrowException)
                    {
                        throw new InvalidOperationException($"Source mesh missing submesh {s}.");
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        public void Append(SerializableMesh other, AppendSubMeshStrategy subMeshStrategy = AppendSubMeshStrategy.MatchedOnly, VertexData fallback = default)
        {
            int otherVertexCount = other.Vertices.Count;

            int baseIndex = _positions.Count;
            _positions.AddAll(other.Vertices);

            _colors?.AddMultiple(fallback.Color, otherVertexCount);

            for (int u = 0; u < MaxUVSetCount; u++)
            {
                UVSet? destUVSet = _uvSets[u];
                if (destUVSet is null)
                {
                    continue;
                }

                List<Vector2> destTexCoords = destUVSet._texCoords;
                destTexCoords.AddMultiple(fallback.GetUV(u), otherVertexCount);
            }

            _normals?.AddMultiple(fallback.Normal, otherVertexCount);

            _tangents?.AddMultiple(fallback.Tangent, otherVertexCount);

            _boneWeightLists?.AddMultiple(fallback.BoneWeightList ?? Array.Empty<BoneWeight1>(), otherVertexCount);

            if (subMeshStrategy is AppendSubMeshStrategy.AddSubMeshes)
            {
                foreach (var srcSubMesh in other.SubMeshes)
                {
                    List<int> srcIndices = srcSubMesh.Indices;

                    SubMesh destSubMesh = new(srcIndices.Count);
                    _subMeshes.Add(destSubMesh);

                    List<int> destIndices = destSubMesh._indices;
                    for (int x = 0; x < srcIndices.Count; x++)
                    {
                        destIndices.Add(baseIndex + srcIndices[x]);
                    }
                }
            }
            else
            {
                for (int s = 0; s < _subMeshes.Count; s++)
                {
                    SubMesh destSubMesh = _subMeshes[s];

                    if (s < other.SubMeshes.Count)
                    {
                        SerializableSubMesh srcSubMesh = other.SubMeshes[s];
                        List<int> srcIndices = srcSubMesh.Indices;

                        List<int> destIndices = destSubMesh._indices;
                        destIndices.SecureCapacity(destIndices.Count + srcIndices.Count);

                        for (int x = 0; x < srcIndices.Count; x++)
                        {
                            destIndices.Add(baseIndex + srcIndices[x]);
                        }
                        continue;
                    }

                    if (subMeshStrategy is AppendSubMeshStrategy.MatchedOnly)
                    {
                        break;
                    }
                    else if (subMeshStrategy is AppendSubMeshStrategy.ThrowException)
                    {
                        throw new InvalidOperationException($"Source mesh missing submesh {s}.");
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        /// <summary>
        /// Append a CW triangle.
        /// </summary>
        /// <remarks>
        /// <code>
        /// 0 - 1
        ///  \ /
        ///   2
        /// </code>
        /// </remarks>
        public RangeInt AppendTriangle(in VertexData v0, in VertexData v1, in VertexData v2)
        {
            int baseIndex = _positions.Count;
            AddVertex(v0);
            AddVertex(v1);
            AddVertex(v2);

            SubMesh subMesh = CurrentSubMesh;
            List<int> indices = subMesh._indices;
            indices.Add(baseIndex);
            indices.Add(baseIndex + 1);
            indices.Add(baseIndex + 2);

            return new RangeInt(baseIndex, 3);
        }

        /// <summary>
        /// Append two CW triangles for a quad.
        /// </summary>
        /// <remarks>
        /// <code>
        /// 0 - 2
        /// | / |
        /// 1 - 3
        /// </code>
        /// </remarks>
        public RangeInt AppendQuad(in VertexData v0, in VertexData v1, in VertexData v2, in VertexData v3)
        {
            int baseIndex = _positions.Count;
            AddVertex(v0);
            AddVertex(v1);
            AddVertex(v2);
            AddVertex(v3);

            SubMesh subMesh = CurrentSubMesh;
            List<int> indices = subMesh._indices;
            indices.Add(baseIndex);
            indices.Add(baseIndex + 2);
            indices.Add(baseIndex + 1);
            indices.Add(baseIndex + 1);
            indices.Add(baseIndex + 2);
            indices.Add(baseIndex + 3);

            return new RangeInt(baseIndex, 4);
        }

        /// <summary>
        /// Append CW triangles for a N-gon.
        /// </summary>
        /// <remarks>
        /// Example for a hexagon.<br/>
        /// <c>c</c> means center vertex and <c>1~6</c> means CW vertices on the N-gon.
        /// <code>
        ///   1 - 2
        ///  / \ / \
        /// 6 - c - 3
        ///  \ / \ /
        ///   5 - 4
        /// </code>
        /// </remarks>
        public RangeInt AppendNGon(in VertexData center, IEnumerable<VertexData> cwVertices)
        {
            int centerIndex = _positions.Count;
            AddVertex(center);

            int baseIndex = _positions.Count;
            int count = 0;
            foreach (var v in cwVertices)
            {
                AddVertex(v);
                count++;
            }

            if (count < 2)
            {
                throw new ArgumentException($"N-gon's vertices count must not be less than 2. Got {count}.", nameof(cwVertices));
            }

            SubMesh subMesh = CurrentSubMesh;
            List<int> indices = subMesh._indices;
            for (int i = 0; i < count - 1; i++)
            {
                indices.Add(centerIndex);
                indices.Add(baseIndex + i);
                indices.Add(baseIndex + i + 1);
            }
            indices.Add(centerIndex);
            indices.Add(baseIndex + count - 1);
            indices.Add(baseIndex);

            return new RangeInt(centerIndex, count + 1);
        }

        /// <param name="recalculateNormals">If normals are collected, this flag has no effect.</param>
        /// <param name="recalculateTangents">If tangents are collected, this flag has no effect.</param>
        /// <remarks>
        /// This method does not close the returned <see cref="Mesh"/>. It means the <see cref="Mesh.isReadable"/> is <c>true</c>.<br/>
        /// The caller may need to call <see cref="Mesh.UploadMeshData(bool)"/> with <c>true</c> to close the <see cref="Mesh"/>.
        /// </remarks>
        public Mesh ToTrianglesMesh(bool recalculateNormals, bool recalculateTangents, bool force32BitIndices = false)
        {
            IndexFormat indexFormat;
            if (force32BitIndices)
            {
                indexFormat = IndexFormat.UInt32;
            }
            else
            {
                indexFormat = _positions.Count > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;
            }

            Mesh mesh = new()
            {
                indexFormat = indexFormat,
                subMeshCount = _subMeshes.Count,
            };

            mesh.SetVertices(_positions);

            for (int i = 0; i < _subMeshes.Count; i++)
            {
                mesh.SetIndices(_subMeshes[i]._indices, MeshTopology.Triangles, i);
            }

            if (_colors is not null)
            {
                mesh.SetColors(_colors);
            }

            for (int i = 0; i < MaxUVSetCount; i++)
            {
                UVSet? uv = _uvSets[i];
                if (uv is null)
                {
                    continue;
                }

                mesh.SetUVs(i, uv._texCoords);
            }

            if (_normals is not null)
            {
                mesh.SetNormals(_normals);
            }
            else if (recalculateNormals)
            {
                mesh.RecalculateNormals();
            }

            if (_tangents is not null)
            {
                mesh.SetTangents(_tangents);
            }
            else if (recalculateTangents)
            {
                mesh.RecalculateTangents();
            }

            if (_boneWeightLists is not null)
            {
                int totalWeightCount = 0;
                for (int i = 0; i < _boneWeightLists.Count; i++)
                {
                    totalWeightCount += _boneWeightLists[i].Count;
                }

                NativeArray<byte> counts = new(_boneWeightLists.Count, Allocator.Temp);
                try
                {
                    NativeArray<BoneWeight1> weights = new(totalWeightCount, Allocator.Temp);
                    try
                    {
                        int w = 0;
                        for (int i = 0; i < _boneWeightLists.Count; i++)
                        {
                            IReadOnlyList<BoneWeight1> list = _boneWeightLists[i];

                            if (list.Count > byte.MaxValue)
                            {
                                throw new InvalidOperationException($"Bone weight count cannot bigger than 255(byte.MaxValue). It is Unity's limitation.");
                            }
                            counts[i] = (byte)list.Count;

                            for (int j = 0; j < list.Count; j++)
                            {
                                weights[w++] = list[j];
                            }
                        }

                        mesh.SetBoneWeights(counts, weights);
                    }
                    finally
                    {
                        weights.Dispose();
                    }
                }
                finally
                {
                    counts.Dispose();
                }
            }

            return mesh;
        }

        public void Clear()
        {
            _positions.Clear();
            _colors?.Clear();
            for (int i = 0; i < MaxUVSetCount; i++)
            {
                _uvSets[i]?._texCoords.Clear();
            }
            _normals?.Clear();
            _tangents?.Clear();
            _boneWeightLists?.Clear();

            _subMeshes.RemoveRange(1, _subMeshes.Count - 1);
            _subMeshes[0]._indices.Clear();

            CurrentSubMeshIndex = 0;
        }

        public void SecureVertexCapacity(int desiredCapacity)
        {
            _positions.SecureCapacity(desiredCapacity);
            _colors?.SecureCapacity(desiredCapacity);
            for (int i = 0; i < MaxUVSetCount; i++)
            {
                _uvSets[i]?.SecureTexCoordCapacity(desiredCapacity);
            }
            _normals?.SecureCapacity(desiredCapacity);
            _tangents?.SecureCapacity(desiredCapacity);
            _boneWeightLists?.SecureCapacity(desiredCapacity);
        }

        public void SecureIndexCapacity(int desiredCapacity)
        {
            for (int s = 0; s < _subMeshes.Count; s++)
            {
                _subMeshes[s].SecureIndexCapacity(desiredCapacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddVertex(in VertexData vertexData)
        {
            _positions.Add(vertexData.Position);
            _colors?.Add(vertexData.Color);
            for (int i = 0; i < MaxUVSetCount; i++)
            {
                _uvSets[i]?._texCoords.Add(vertexData.GetUV(i));
            }
            _normals?.Add(vertexData.Normal);
            _tangents?.Add(vertexData.Tangent);
            _boneWeightLists?.Add(vertexData.BoneWeightList ?? Array.Empty<BoneWeight1>());
        }

        public class UVSet
        {
            internal readonly List<Vector2> _texCoords;
            public IReadOnlyList<Vector2> TexCoords => _texCoords;

            internal UVSet(int capacity)
            {
                _texCoords = new List<Vector2>(capacity);
            }

            public void EditTexCoords(int startIndex, int count, EditDelegate<Vector2> edit)
            {
                EditImpl("tex coords", _texCoords, startIndex, count, edit, null);
            }

            public void SecureTexCoordCapacity(int desiredCapacity)
            {
                _texCoords.SecureCapacity(desiredCapacity);
            }
        }
        public class SubMesh
        {
            internal readonly List<int> _indices;
            public IReadOnlyList<int> Indices => _indices;

            internal SubMesh(int capacity)
            {
                _indices = new List<int>(capacity);
            }

            public void SecureIndexCapacity(int desiredCapacity)
            {
                _indices.SecureCapacity(desiredCapacity);
            }
        }

        /// <returns>Return <c>false</c> to abort. The <paramref name="result"/> will be ignored when <c>false</c> returned.</returns>
        public delegate bool EditDelegate<T>(int index, T source, out T result);

        public const int MaxUVSetCount = 8;
    }

    public struct VertexData
    {
        public Vector3 Position;
        public Color Color;
        public Vector2 UV0, UV1, UV2, UV3, UV4, UV5, UV6, UV7;
        public Vector3 Normal;
        public Vector4 Tangent;
        public IReadOnlyList<BoneWeight1>? BoneWeightList;

        public VertexData(Vector3 position) : this()
        {
            Position = position;
        }
        public VertexData(Vector3 position, Color color) : this()
        {
            Position = position;
            Color = color;
        }
        public VertexData(Vector3 position, Vector2 uv0) : this()
        {
            Position = position;
            UV0 = uv0;
        }
        public VertexData(Vector3 position, Color color, Vector2 uv0) : this()
        {
            Position = position;
            Color = color;
            UV0 = uv0;
        }
        public VertexData(Vector3 position, Vector2 uv0, Vector3 normal) : this()
        {
            Position = position;
            UV0 = uv0;
            Normal = normal;
        }
        public VertexData(Vector3 position, Color color, Vector2 uv0, Vector3 normal) : this()
        {
            Position = position;
            Color = color;
            UV0 = uv0;
            Normal = normal;
        }

        public readonly Vector2 GetUV(int i)
        {
            return i switch
            {
                0 => UV0,
                1 => UV1,
                2 => UV2,
                3 => UV3,
                4 => UV4,
                5 => UV5,
                6 => UV6,
                7 => UV7,
                _ => throw new ArgumentOutOfRangeException(nameof(i)),
            };
        }

        public void SetUV(int i, Vector2 value)
        {
            switch (i)
            {
                case 0: UV0 = value; break;
                case 1: UV1 = value; break;
                case 2: UV2 = value; break;
                case 3: UV3 = value; break;
                case 4: UV4 = value; break;
                case 5: UV5 = value; break;
                case 6: UV6 = value; break;
                case 7: UV7 = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(i));
            }
        }
    }

    [Flags]
    public enum AdditionalAttributes : uint
    {
        None = 0,
        Color = 1 << 1,
        UV0 = 1 << 8,
        UV1 = 1 << 9,
        UV2 = 1 << 10,
        UV3 = 1 << 11,
        UV4 = 1 << 12,
        UV5 = 1 << 13,
        UV6 = 1 << 14,
        UV7 = 1 << 15,
        Normal = 1 << 24,
        Tangent = 1 << 25,
        BoneWeight = 1 << 26,
        All = 0xffffffff,
    }

    public static class MeshCollectorEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has(this AdditionalAttributes left, AdditionalAttributes right)
        {
            return (left & right) != 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AdditionalAttributes Set(this AdditionalAttributes left, AdditionalAttributes right)
        {
            return left | right;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AdditionalAttributes Unset(this AdditionalAttributes left, AdditionalAttributes right)
        {
            return left & (~right);
        }
    }
}
