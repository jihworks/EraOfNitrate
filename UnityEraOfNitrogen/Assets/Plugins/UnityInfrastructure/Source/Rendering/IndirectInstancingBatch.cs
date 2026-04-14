// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Jih.Unity.Infrastructure.Rendering
{
    /// <summary>
    /// GPU instancing with indirect method.
    /// </summary>
    /// <remarks>
    /// The target hardware must support GPU instancing AND indirect draw-call.<br/>
    /// Check <see cref="SystemInfo.supportsInstancing"/> and <see cref="SystemInfo.supportsIndirectArgumentsBuffer"/> for details about hardware compatibility.<br/>
    /// <br/>
    /// The <see cref="Materials"/> must use corresponding shader graph or shader code.<br/>
    /// Check 'Indirect Instancing.shadersubgraph' for shader graphs or 'Indirect Instancing.hlsl' for shader codes in 'Assets' folder.
    /// </remarks>
    public class IndirectInstancingBatch : IDisposable
    {
        public Mesh Mesh { get; }
        public IReadOnlyList<Material?> Materials { get; }

        public int TransformsBufferIncreaseLength { get; }

        public TransformsCollection InstanceTransforms { get; }

        public Bounds TotalBounds { get; private set; }

        public ShadowCastingMode ShadowCastingMode { get; set; } = ShadowCastingMode.On;
        public bool ReceiveShadows { get; set; } = true;

        public bool IsDisposed { get; private set; }

        MaterialPropertyBlock? _materialPropertyBlock;
        Material?[]? _renderMaterials;
        readonly GraphicsBuffer.IndirectDrawIndexedArgs[] _argsArr;
        GraphicsBuffer? _transformsBuffer, _argsBuffer;

        public IndirectInstancingBatch(Mesh mesh, IReadOnlyList<Material> materials, int transformsListCapacity = 8, int transformsBlockIncreaseLength = 64)
        {
            if (transformsBlockIncreaseLength <= 0)
            {
                throw new ArgumentException("Transforms block increase length must be positive value.");
            }
            Mesh = mesh;
            Materials = materials;
            InstanceTransforms = new TransformsCollection(transformsListCapacity);
            TransformsBufferIncreaseLength = transformsBlockIncreaseLength;

            int subMeshCount = Mesh.subMeshCount;
            _argsArr = new GraphicsBuffer.IndirectDrawIndexedArgs[subMeshCount];

#if DEBUG
            if (materials.Count < subMeshCount)
            {
                Debug.LogWarning($"Mesh '{mesh.name}' has {subMeshCount} sub-meshes but instancing batch received {materials.Count} materials. Missing part will not render.");
            }
            else
            {
                for (int s = 0; s < subMeshCount; s++)
                {
                    Material? material = materials[s];
                    if (material == null)
                    {
                        Debug.LogWarning($"Instancing batch with mesh '{mesh.name}' received null material for sub-mesh {s}. That part will not render.");
                    }
                }
            }
#endif
        }

        public void Update()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            int transformCount = InstanceTransforms.Count;
            int subMeshCount = Mesh.subMeshCount;

            if (transformCount <= 0 || subMeshCount <= 0)
            {
                return;
            }

            _materialPropertyBlock ??= new MaterialPropertyBlock();

            if (_renderMaterials is null)
            {
                _renderMaterials = new Material?[subMeshCount];

                int renderCount = Math.Min(subMeshCount, Materials.Count);
                for (int i = 0; i < renderCount; i++)
                {
                    _renderMaterials[i] = Materials[i];
                }
            }

            GraphicsBuffer transformsBufferN = SecureTransformsBufferLength(ref _transformsBuffer, transformCount, TransformsBufferIncreaseLength, out bool transformsAllocated);
            GraphicsBuffer argsBufferN = SecureArgsBufferLength(ref _argsBuffer, subMeshCount, out bool argsAllocated);

            if (InstanceTransforms.IsDirty)
            {
                // Get total bounds with fast approximated method.

                Bounds localBounds = Mesh.bounds;
                Vector3 localCenter = localBounds.center;

                Vector3 min = new(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 max = new(float.MinValue, float.MinValue, float.MinValue);

                float maxScaleSq = 0f;
                for (int i = 0; i < transformCount; i++)
                {
                    Matrix4x4 transform = InstanceTransforms[i];

                    Vector3 worldCenter = transform.MultiplyPoint(localCenter);
                    min = Vector3.Min(min, worldCenter);
                    max = Vector3.Max(max, worldCenter);

                    // Extract biggest value among X, Y, Z scales.
                    float scaleXSq = new Vector3(transform.m00, transform.m10, transform.m20).sqrMagnitude;
                    float scaleYSq = new Vector3(transform.m01, transform.m11, transform.m21).sqrMagnitude;
                    float scaleZSq = new Vector3(transform.m02, transform.m12, transform.m22).sqrMagnitude;

                    float currMaxScaleSq = MathEx.Max(scaleXSq, scaleYSq, scaleZSq);
                    if (currMaxScaleSq > maxScaleSq)
                    {
                        maxScaleSq = currMaxScaleSq;
                    }
                }

                Bounds totalBounds = BoundsEx.CreateMinMax(min, max);

                // Expand by max scale * local bounds diagonal length.
                float radius = localBounds.extents.magnitude * Mathf.Sqrt(maxScaleSq);
                totalBounds.Expand(radius * 2f);

                TotalBounds = totalBounds;
            }

            if (transformsAllocated)
            {
                _materialPropertyBlock.SetBuffer(IndirectInstancingEx.InstanceTransformsId, _transformsBuffer);
            }

            if (InstanceTransforms.IsDirty || transformsAllocated)
            {
                transformsBufferN.SetData(InstanceTransforms.InnerList);
            }

            if (InstanceTransforms.IsDirty || argsAllocated)
            {
                for (int s = 0; s < subMeshCount; s++)
                {
                    SubMeshDescriptor descriptor = Mesh.GetSubMesh(s);

                    ref GraphicsBuffer.IndirectDrawIndexedArgs args = ref _argsArr[s];
                    args.indexCountPerInstance = (uint)descriptor.indexCount;
                    args.instanceCount = (uint)transformCount;
                    args.startIndex = (uint)descriptor.indexStart;
                    args.baseVertexIndex = (uint)descriptor.baseVertex;
                    args.startInstance = 0;
                }

                argsBufferN.SetData(_argsArr);
            }

            InstanceTransforms.IsDirty = false;

            for (int s = 0; s < subMeshCount; s++)
            {
                Material? material = _renderMaterials[s];
                if (material == null)
                {
                    continue;
                }

                RenderParams renderParams = new(material)
                {
                    worldBounds = TotalBounds,
                    matProps = _materialPropertyBlock,

                    shadowCastingMode = ShadowCastingMode,
                    receiveShadows = ReceiveShadows,
                };

                int commandOffset = s * GraphicsBuffer.IndirectDrawIndexedArgs.size;
                Graphics.RenderMeshIndirect(renderParams, Mesh, _argsBuffer, 1, commandOffset);
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            if (_transformsBuffer is not null)
            {
                _transformsBuffer.Release();
                _transformsBuffer = null;
            }
            if (_argsBuffer is not null)
            {
                _argsBuffer.Release();
                _argsBuffer = null;
            }

            _renderMaterials = null;
            _materialPropertyBlock = null;

            IsDisposed = true;
        }

        static GraphicsBuffer SecureArgsBufferLength(ref GraphicsBuffer? argsBuffer, int targetLength, out bool newlyAllocated)
        {
            if (argsBuffer is not null && argsBuffer.count >= targetLength)
            {
                newlyAllocated = false;
                return argsBuffer;
            }

            argsBuffer?.Release();

            argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, targetLength, GraphicsBuffer.IndirectDrawIndexedArgs.size);
            newlyAllocated = true;
            return argsBuffer;
        }

        static GraphicsBuffer SecureTransformsBufferLength(ref GraphicsBuffer? transformsBuffer, int targetCount, int increaseLength, out bool newlyAllocated)
        {
            if (transformsBuffer is not null && transformsBuffer.count >= targetCount)
            {
                newlyAllocated = false;
                return transformsBuffer;
            }

            transformsBuffer?.Release();

            int bufferLength = targetCount.CeilToMultiple(increaseLength);
            transformsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, bufferLength, StructureSize);
            newlyAllocated = true;
            return transformsBuffer;
        }

        public class TransformsCollection : IList<Matrix4x4>, IReadOnlyList<Matrix4x4>
        {
            internal bool IsDirty { get; set; }

            internal List<Matrix4x4> InnerList { get; }

            public Matrix4x4 this[int index]
            {
                get => InnerList[index];
                set
                {
                    if (InnerList[index] == value)
                    {
                        return;
                    }
                    InnerList[index] = value;
                    IsDirty = true;
                }
            }

            public int Count => InnerList.Count;

            bool ICollection<Matrix4x4>.IsReadOnly => ((ICollection<Matrix4x4>)InnerList).IsReadOnly;

            public TransformsCollection(int capacity)
            {
                InnerList = new List<Matrix4x4>(capacity);
            }

            public void Add(Matrix4x4 item)
            {
                InnerList.Add(item);
                IsDirty = true;
            }

            public void AddRange(IEnumerable<Matrix4x4> items)
            {
                int count = InnerList.Count;
                InnerList.AddRange(items);
                IsDirty |= count != InnerList.Count;
            }

            public void Insert(int index, Matrix4x4 item)
            {
                InnerList.Insert(index, item);
                IsDirty = true;
            }

            public bool Remove(Matrix4x4 item)
            {
                bool result = InnerList.Remove(item);
                IsDirty |= result;
                return result;
            }

            public void RemoveAt(int index)
            {
                InnerList.RemoveAt(index);
                IsDirty = true;
            }

            public void Clear()
            {
                bool anyExisted = InnerList.Count > 0;
                InnerList.Clear();
                IsDirty |= anyExisted;
            }

            public bool Contains(Matrix4x4 item)
            {
                return InnerList.Contains(item);
            }

            public int IndexOf(Matrix4x4 item)
            {
                return InnerList.IndexOf(item);
            }

            public void CopyTo(Matrix4x4[] array, int arrayIndex)
            {
                InnerList.CopyTo(array, arrayIndex);
            }

            public IEnumerator<Matrix4x4> GetEnumerator()
            {
                return InnerList.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return InnerList.GetEnumerator();
            }
        }

        // One Matrix4x4
        const int StructureSize = sizeof(float) * 16;
    }
}
