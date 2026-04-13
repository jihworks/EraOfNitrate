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
using Jih.Unity.Infrastructure.Rendering;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jih.Unity.EraOfNitrogen.Worlds.Runtime
{
    public class DoodadCluster : IDisposable
    {
        readonly List<DoodadElement> _elements;
        public IReadOnlyList<DoodadElement> Elements => _elements;

        readonly IndirectInstancingBatch _instancingBatch;
        readonly List<DoodadCollision> _collisions;

        public DoodadCluster(Mesh mesh, SerializableMesh convexHull, Material[] materials, IReadOnlyList<DoodadTransform> transforms)
        {
            int count = transforms.Count;

            _instancingBatch = new IndirectInstancingBatch(mesh, materials, count);

            _elements = new List<DoodadElement>(count);
            for (int i = 0; i < count; i++)
            {
                _elements.Add(new DoodadElement(this, i, transforms[i]));
            }

            for (int i = 0; i < count; i++)
            {
                _instancingBatch.InstanceTransforms.Add(_elements[i].Matrix);
            }

            {
                List<Vector3> convexHullPoints = convexHull.Vertices;
                List<int> convexHullIndices = convexHull.SubMeshes[0].Indices;
                int convexHullTriangleCount = convexHullIndices.Count / 3;

                _collisions = new List<DoodadCollision>(count);
                for (int i = 0; i < count; i++)
                {
                    DoodadCollision collision = new(convexHullTriangleCount);

                    collision.Append(convexHullPoints, convexHullIndices);
                    collision.WorldTransform = _elements[i].Matrix;
                    collision.Freeze();

                    _collisions.Add(collision);
                }
            }
        }

        public void Update()
        {
            _instancingBatch.Update();
        }

        public void RegisterCollisions(CollisionWorld collisionWorld)
        {
            foreach (var collision in _collisions)
            {
                collisionWorld.Register(collision);
            }
        }

        void SetInstancingMatrix(int index, in Matrix4x4 matrix)
        {
            _instancingBatch.InstanceTransforms[index] = matrix;
        }

        void SetCollisionEnabled(int index, bool enabled)
        {
            _collisions[index].IsEnabled = enabled;
        }

        public void Dispose()
        {
            _instancingBatch.Dispose();
        }

        public abstract class Element
        {
            public DoodadCluster Cluster { get; }
            public int Index { get; }

            public DoodadTransform Transform { get; }
            public Matrix4x4 Matrix { get; }

            bool _isVisible = true;
            public bool IsVisible
            {
                get => _isVisible;
                set
                {
                    if (_isVisible == value)
                    {
                        return;
                    }
                    _isVisible = value;
                    OnIsVisibleChanged();
                }
            }

            public Element(DoodadCluster cluster, int index, DoodadTransform transform)
            {
                Cluster = cluster;
                Index = index;
                Transform = transform;
                Matrix = transform.GetMatrix();
            }

            void OnIsVisibleChanged()
            {
                Cluster.SetInstancingMatrix(Index, _isVisible ? Matrix : _hideMatrix);
                Cluster.SetCollisionEnabled(Index, _isVisible);
            }
        }

        static readonly Matrix4x4 _hideMatrix = Matrix4x4.Scale(Vector3.zero);
    }

    public sealed class DoodadElement : DoodadCluster.Element
    {
        public DoodadElement(DoodadCluster cluster, int index, DoodadTransform originalTransform)
            : base(cluster, index, originalTransform)
        {
        }
    }

    public struct DoodadTransform
    {
        public Vector3 UnityLocation;
        public float UnityRotationY;
        public float UnityScale;

        public DoodadTransform(Vector3 unityLocation, float unityRotationY, float unityScale)
        {
            UnityLocation = unityLocation;
            UnityRotationY = unityRotationY;
            UnityScale = unityScale;
        }

        public readonly Matrix4x4 GetMatrix()
        {
            return Matrix4x4.TRS(
                UnityLocation,
                Quaternion.AngleAxis(UnityRotationY, Vector3.up),
                VectorEx.CreateUniform3(UnityScale));
        }
    }
}
