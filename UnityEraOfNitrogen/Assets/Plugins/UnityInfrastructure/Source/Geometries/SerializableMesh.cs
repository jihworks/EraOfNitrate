// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jih.Unity.Infrastructure.Geometries
{
    public class SerializableMesh : ScriptableObject
    {
        [SerializeField] List<Vector3> _vertices = new();
        public List<Vector3> Vertices => _vertices;

        [SerializeField] List<SerializableSubMesh> _subMeshes = new();
        public List<SerializableSubMesh> SubMeshes => _subMeshes;

        public void SetVertices(List<Vector3> vertices)
        {
            _vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
        }
        public void SetVertices(IEnumerable<Vector3> vertices)
        {
            _vertices.Clear();
            _vertices.AddRange(vertices);
        }

        public void SetSubMeshes(SerializableSubMesh subMesh)
        {
            _subMeshes.Clear();
            _subMeshes.Add(subMesh);
        }
        public void SetSubMeshes(List<SerializableSubMesh> subMeshes)
        {
            _subMeshes = subMeshes ?? throw new ArgumentNullException(nameof(subMeshes));
        }
        public void SetSubMeshes(IEnumerable<SerializableSubMesh> subMeshes)
        {
            _subMeshes.Clear();
            _subMeshes.AddRange(subMeshes);
        }

        public Mesh ToUnityMesh()
        {
            Mesh mesh = new();
            mesh.SetVertices(_vertices);

            for (int i = 0; i < _subMeshes.Count; i++)
            {
                mesh.SetTriangles(_subMeshes[i].Indices, i);
            }

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        public static SerializableMesh FromUnityMesh(Mesh unityMesh)
        {
            SerializableMesh serializableMesh = CreateInstance<SerializableMesh>();

            // Copy vertices
            serializableMesh._vertices.Clear();
            serializableMesh._vertices.AddRange(unityMesh.vertices);

            // Copy submeshes
            serializableMesh._subMeshes.Clear();
            for (int i = 0; i < unityMesh.subMeshCount; i++)
            {
                SerializableSubMesh subMesh = new();
                subMesh.SetIndices(unityMesh.GetTriangles(i));
                serializableMesh._subMeshes.Add(subMesh);
            }

            return serializableMesh;
        }
    }

    [Serializable]
    public class SerializableSubMesh
    {
        [SerializeField] List<int> _indices = new();
        public List<int> Indices => _indices;

        public void SetIndices(List<int> indices)
        {
            _indices = indices ?? throw new ArgumentNullException(nameof(indices));
        }
        public void SetIndices(IEnumerable<int> indices)
        {
            _indices.Clear();
            _indices.AddRange(indices);
        }
    }
}
