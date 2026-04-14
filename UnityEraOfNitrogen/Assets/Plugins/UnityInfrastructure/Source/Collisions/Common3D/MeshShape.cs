// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.Unity.Infrastructure.Geometries;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Jih.Unity.Infrastructure.Collisions.Common3D
{
    public class MeshShape : CollisionShape
    {
        /// <summary>
        /// Wheter this collision is fixed.
        /// </summary>
        /// <remarks>
        /// If <c>true</c>, cannot modify this collision.<br/>
        /// When call <see cref="Clear"/>, fixed state will be released.
        /// </remarks>
        public bool IsFrozen { get; private set; }

        public TrianglesCollection Triangles { get; }

        OrderablePartitioner<Tuple<int, int>>? _partitioner;

        Octree<CollisionTriangle>? _octree;

        public MeshShape(int trianglesCapacity = 8)
        {
            Triangles = new TrianglesCollection(this, trianglesCapacity);
        }

        public void Append(MeshShape meshCollision)
        {
            Triangles.AddRange(meshCollision.Triangles);
        }

        public void Append(MeshCollector meshCollector)
        {
            foreach (var subMesh in meshCollector.SubMeshes)
            {
                Append(meshCollector.Positions, subMesh.Indices);
            }
        }

        public void Append(IReadOnlyList<Vector3> vertices, IReadOnlyList<int> triangles)
        {
            List<CollisionTriangle> directList = Triangles.InnerList;
            directList.SecureCapacity(directList.Count + triangles.Count / 3);

            for (int i = 0; i < triangles.Count; i += 3)
            {
                int i0 = triangles[i];
                int i1 = triangles[i + 1];
                int i2 = triangles[i + 2];

                Vector3 v0 = vertices[i0];
                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];

                Triangles.Add(new CollisionTriangle(v0, v1, v2));
            }
        }

        protected override bool NeedUpdateBounds_Impl()
        {
            return base.NeedUpdateBounds_Impl() || Triangles.IsDirty;
        }
        protected override void UpdateBounds_Impl(ref bool isLocalBoundsDirty, ref bool isWorldTransformDirty)
        {
            if (isLocalBoundsDirty)
            {
                CheckFrozenEdit($"Edit {LocalBounds}");
            }
            if (isWorldTransformDirty)
            {
                CheckFrozenEdit($"Edit {WorldTransform}");
            }
            if (Triangles.IsDirty)
            {
                CheckFrozenEdit($"Edit {Triangles}");
            }

            // Direct access.
            List<CollisionTriangle> triangles = Triangles.InnerList;

            Bounds localBounds = BoundsEx.Empty;
            for (int t = 0; t < triangles.Count; t++)
            {
                localBounds = BoundsEx.Union(localBounds, triangles[t].Bounds);
            }
            LocalBounds = localBounds;
            WorldBounds = localBounds.GetApproxTransformed(WorldTransform);

            if (triangles.Count > ParallelismThreshold)
            {
                _partitioner = Partitioner.Create(0, triangles.Count);

                Parallel.ForEach(_partitioner, (range, state) => UpdateTriangles(range.Item1, range.Item2));
            }
            else
            {
                _partitioner = null;

                UpdateTriangles(0, triangles.Count);
            }


            void UpdateTriangles(int start, int end)
            {
                for (int t = start; t < end; t++)
                {
                    CollisionTriangle triangle = triangles[t];

                    Vector3 worldPoint0 = WorldTransform.MultiplyPoint(triangle.V0);
                    Vector3 worldPoint1 = WorldTransform.MultiplyPoint(triangle.V1);
                    Vector3 worldPoint2 = WorldTransform.MultiplyPoint(triangle.V2);

                    Vector3 min = Vector3.Min(Vector3.Min(worldPoint0, worldPoint1), worldPoint2);
                    Vector3 max = Vector3.Max(Vector3.Max(worldPoint0, worldPoint1), worldPoint2);

                    Bounds worldBounds = BoundsEx.CreateMinMax(min, max);
                    worldBounds = worldBounds.GetInflated(CollisionEx.BoundsEpsilon3);

                    triangle.WorldV0 = worldPoint0;
                    triangle.WorldV1 = worldPoint1;
                    triangle.WorldV2 = worldPoint2;
                    triangle.WorldBounds = worldBounds;

                    triangles[t] = triangle;
                }
            }

            isLocalBoundsDirty = false;
            isWorldTransformDirty = false;
            Triangles.IsDirty = false;
        }

        /// <summary>
        /// Make this collision to fixed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If fixing the collision, data structure will be optimized and provides better performance.<br/>
        /// Instead, cannot move this collision(<see cref="CollisionShape.WorldTransform"/>) and cannot add or remove new elements.(<see cref="Triangles"/>)
        /// </para>
        /// </remarks>
        public void Freeze(int octreeDepth = 2)
        {
            if (IsFrozen)
            {
                return;
            }

            UpdateBounds();

            // Direct access.
            List<CollisionTriangle> triangles = Triangles.InnerList;

            // Update to exact world bounds.
            Bounds worldBounds = BoundsEx.Empty;
            for (int i = 0; i < triangles.Count; i++)
            {
                CollisionTriangle triangle = triangles[i];

                worldBounds = BoundsEx.Union(worldBounds, triangle.WorldBounds);
            }
            WorldBounds = worldBounds;

            // Build octree.
            Octree<CollisionTriangle>.Release(ref _octree);
            _octree = Octree<CollisionTriangle>.Create(octreeDepth);
            _octree.Update(triangles, _partitioner, worldBounds, IsBoundsIntersects);

            IsFrozen = true;
        }

        public void Clear()
        {
            IsFrozen = false;
            Triangles.ClearInternal();
            ClearCollisionShapeBoundsAndTransform();
            _partitioner = null;
            Octree<CollisionTriangle>.Release(ref _octree);
        }

        protected override bool IntersectsWith_Impl(BoxShape other)
        {
            if (_octree is not null)
            {
                return IntersectsWithOctree(_octree, other);
            }
            else
            {
                return IntersectsWithDefault(other);
            }
        }

        bool IntersectsWithDefault(BoxShape other)
        {
            if (!WorldBounds.Intersects(other.WorldBounds))
            {
                return false;
            }

            // Direct access.
            List<CollisionTriangle> triangles = Triangles.InnerList;

            if (_partitioner is not null)
            {
                int count = 0;
                Parallel.ForEach(_partitioner, (range, state) =>
                {
                    if (Interlocked.CompareExchange(ref count, 0, 0) > 0)
                    {
                        // Already found. Abort.
                        return;
                    }
                    if (Process(range.Item1, range.Item2))
                    {
                        Interlocked.Increment(ref count);
                        return;
                    }
                });
                return count > 0;
            }
            else
            {
                return Process(0, triangles.Count);
            }


            bool Process(int start, int end)
            {
                for (int i = 0; i < triangles.Count; i++)
                {
                    CollisionTriangle triangle = triangles[i];

                    // Check if the triangle intersects with the box using SAT method
                    if (other.DoesTriangleIntersectBox(triangle))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        bool IntersectsWithOctree(Octree<CollisionTriangle> thisOctree, BoxShape other)
        {
            Bounds otherWorldBounds = other.WorldBounds;

            bool result = false;
            thisOctree.Search(bounds => bounds.Intersects(otherWorldBounds), leaf =>
            {
                List<CollisionTriangle> items = leaf.Items;

                for (int i = 0; i < items.Count; i++)
                {
                    CollisionTriangle triangle = items[i];

                    // Check if the triangle intersects with the box using SAT method
                    if (other.DoesTriangleIntersectBox(triangle))
                    {
                        result = true;
                        return false;
                    }
                }
                return true;
            });
            return result;
        }

        protected override bool IntersectsWith_Impl(MeshShape other)
        {
            if (_octree is not null && other._octree is not null)
            {
                return IntersectsWithOctrees(this, _octree, other, other._octree);
            }
            else if (_octree is null && other._octree is not null)
            {
                return IntersectsWithThisOctree(other, other._octree, this);
            }
            else if (_octree is not null && other._octree is null)
            {
                return IntersectsWithThisOctree(this, _octree, other);
            }
            else
            {
                return IntersectsWithDefault(other);
            }
        }

        bool IntersectsWithDefault(MeshShape other)
        {
            // Direct access.
            List<CollisionTriangle> triangles = Triangles.InnerList;
            List<CollisionTriangle> otherTriangles = other.Triangles.InnerList;

            if (otherTriangles.Count > triangles.Count)
            {
                // Calculate with has more triangles side.
                return other.IntersectsWithDefault(this);
            }

            if (!WorldBounds.Intersects(other.WorldBounds))
            {
                return false;
            }

            if (IsAnyInsideMesh(this, other))
            {
                return true;
            }

            if (_partitioner is not null)
            {
                int count = 0;
                Parallel.ForEach(_partitioner, (range, state) =>
                {
                    if (Interlocked.CompareExchange(ref count, 0, 0) > 0)
                    {
                        // Already found. Abort.
                        return;
                    }
                    if (Process(range.Item1, range.Item2))
                    {
                        Interlocked.Increment(ref count);
                        return;
                    }
                });
                return count > 0;
            }
            else
            {
                return Process(0, triangles.Count);
            }


            bool Process(int start, int end)
            {
                for (int i = start; i < end; i++)
                {
                    CollisionTriangle triangle0 = triangles[i];

                    for (int j = 0; j < otherTriangles.Count; j++)
                    {
                        CollisionTriangle triangle1 = otherTriangles[j];

                        if (IntersectsWith(triangle0, triangle1))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        static bool IntersectsWithThisOctree(MeshShape thisMesh, Octree<CollisionTriangle> thisOctree, MeshShape other)
        {
            // Direct access.
            List<CollisionTriangle> otherTriangles = other.Triangles.InnerList;
            
            bool result;
            if (other._partitioner is not null)
            {
                int count = 0;
                Parallel.ForEach(other._partitioner, (range, state) =>
                {
                    if (Interlocked.CompareExchange(ref count, 0, 0) > 0)
                    {
                        // Already found. Abort.
                        return;
                    }
                    if (Process(range.Item1, range.Item2))
                    {
                        Interlocked.Increment(ref count);
                        return;
                    }
                });
                result = count > 0;
            }
            else
            {
                result = Process(0, otherTriangles.Count);
            }

            if (result)
            {
                return true;
            }

            return IsAnyInsideMesh(thisMesh, other);


            bool Process(int min1, int max1)
            {
                for (int i1 = min1; i1 < max1; i1++)
                {
                    CollisionTriangle otherTriangle = otherTriangles[i1];

                    bool found = false;
                    thisOctree.Search(otherTriangle, IsBoundsIntersects, (item, leaf) =>
                    {
                        IReadOnlyList<CollisionTriangle> items0 = leaf.Items;
                        for (int i0 = 0; i0 < items0.Count; i0++)
                        {
                            CollisionTriangle thisTriangle = items0[i0];
                            if (IntersectsWith(thisTriangle, otherTriangle))
                            {
                                found = true;
                                return false;
                            }
                        }
                        return true;
                    });
                    if (found)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        static bool IntersectsWithOctrees(MeshShape thisMesh, Octree<CollisionTriangle> thisOctree, MeshShape otherMesh, Octree<CollisionTriangle> otherOctree)
        {
            bool result = false;
            thisOctree.Search(otherOctree, true, (thisLeaf, otherLeaf) =>
            {
                if (Process(thisMesh, otherMesh, thisLeaf, otherLeaf))
                {
                    result = true;
                    return false;
                }
                return true;
            });

            if (result)
            {
                return true;
            }

            return IsAnyInsideMesh(thisMesh, otherMesh);


            static bool Process(MeshShape thisMesh, MeshShape otherMesh, OctreeLeafNode<CollisionTriangle> leaf0, OctreeLeafNode<CollisionTriangle> leaf1)
            {
                IReadOnlyList<CollisionTriangle> items0 = leaf0.Items, items1 = leaf1.Items;

                for (int i0 = 0; i0 < items0.Count; i0++)
                {
                    CollisionTriangle thisTriangle = items0[i0];

                    for (int i1 = 0; i1 < items1.Count; i1++)
                    {
                        CollisionTriangle otherTriangle = items1[i1];

                        if (IntersectsWith(thisTriangle, otherTriangle))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        static bool IntersectsWith(CollisionTriangle triangle0, CollisionTriangle triangle1)
        {
            if (!triangle0.WorldBounds.Intersects(triangle1.WorldBounds))
            {
                return false;
            }

            Vector3 worldV0 = triangle0.WorldV0;
            Vector3 worldV1 = triangle0.WorldV1;
            Vector3 worldV2 = triangle0.WorldV2;

            Vector3 otherWorldV0 = triangle1.WorldV0;
            Vector3 otherWorldV1 = triangle1.WorldV1;
            Vector3 otherWorldV2 = triangle1.WorldV2;

            return MathEx.AreTrianglesIntersect(
                worldV0, worldV1, worldV2,
                otherWorldV0, otherWorldV1, otherWorldV2);
        }

        protected override bool Raycast_Impl(Vector3 rayOrigin, Vector3 rayDirection, out float t)
        {
            t = float.MaxValue;

            if (!WorldBounds.Raycast(rayOrigin, rayDirection))
            {
                return false;
            }

            bool hit = false;
            float hitT = t;
            if (_octree is not null)
            {
                _octree.Search(bounds => bounds.Raycast(rayOrigin, rayDirection), leaf =>
                {
                    List<CollisionTriangle> items = leaf.Items;

                    Raycast(items, rayOrigin, rayDirection, ref hit, ref hitT);

                    return true;
                });
            }
            else
            {
                Raycast(Triangles.InnerList, rayOrigin, rayDirection, ref hit, ref hitT);
            }

            t = hitT;
            return hit;
        }
        static void Raycast(IReadOnlyList<CollisionTriangle> triangles, Vector3 rayOrigin, Vector3 rayDirection, ref bool hit, ref float t)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                CollisionTriangle triangle = triangles[i];

                // First check if the ray intersects the triangle's bounding box (broad phase)
                if (!triangle.WorldBounds.Raycast(rayOrigin, rayDirection))
                {
                    continue;
                }

                // Then do the more expensive triangle intersection test (narrow phase)
                if (MathEx.AreRayTriangleIntersect(rayOrigin, rayDirection,
                    triangle.WorldV0, triangle.WorldV1, triangle.WorldV2, out float hitT))
                {
                    // If this intersection is closer than any previous one, update t
                    if (hitT < t)
                    {
                        t = hitT;
                        hit = true;
                    }
                }
            }
        }

        static bool IsAnyInsideMesh(MeshShape thisMesh, MeshShape otherMesh)
        {
            return IsOtherMeshInsideThisMesh(thisMesh, otherMesh) ||
                IsOtherMeshInsideThisMesh(otherMesh, thisMesh);
        }

        static bool IsOtherMeshInsideThisMesh(MeshShape thisMesh, MeshShape otherMesh)
        {
            // Direct access.
            List<CollisionTriangle> thisTriangles = thisMesh.Triangles.InnerList;
            List<CollisionTriangle> otherTriangles = otherMesh.Triangles.InnerList;

            if (thisTriangles.Count <= 0 || otherTriangles.Count <= 0)
            {
                return false;
            }

            if (otherMesh._partitioner is not null)
            {
                int outsideCount = 0;
                Parallel.ForEach(otherMesh._partitioner, (range, state) =>
                {
                    if (Interlocked.CompareExchange(ref outsideCount, 0, 0) > 0)
                    {
                        // Already found. Abort.
                        return;
                    }
                    if (AnyOutside(range.Item1, range.Item2, false))
                    {
                        Interlocked.Increment(ref outsideCount);
                        return;
                    }
                });
                return outsideCount <= 0;
            }
            else
            {
                return !AnyOutside(0, otherTriangles.Count, true);
            }


            bool AnyOutside(int start, int end, bool allowParallel)
            {
                for (int i = start; i < end; i++)
                {
                    CollisionTriangle triangle = otherTriangles[i];

                    Vector3 worldV0 = triangle.WorldV0;
                    if (!IsPointInsideMeshWorld(thisMesh, worldV0, allowParallel))
                    {
                        return true;
                    }

                    Vector3 worldV1 = triangle.WorldV1;
                    if (!IsPointInsideMeshWorld(thisMesh, worldV1, allowParallel))
                    {
                        return true;
                    }

                    Vector3 worldV2 = triangle.WorldV2;
                    if (!IsPointInsideMeshWorld(thisMesh, worldV2, allowParallel))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        static bool IsPointInsideMeshWorld(MeshShape mesh, Vector3 point, bool allowParallel)
        {
            // Direct access.
            List<CollisionTriangle> triangles = mesh.Triangles.InnerList;

            // Using raycasting method.
            Vector3 rayDirection = Vector3.up;

            int intersectionCount = 0;
            if (allowParallel && mesh._partitioner is not null)
            {
                Parallel.ForEach(mesh._partitioner, (range, state) => Process(range.Item1, range.Item2));
            }
            else
            {
                Process(0, triangles.Count);
            }

            // If intersect cound is odd number, it is inside.
            return (intersectionCount % 2) == 1;


            void Process(int start, int end)
            {
                for (int t = start; t < end; t++)
                {
                    CollisionTriangle triangle = triangles[t];

                    if (MathEx.AreRayTriangleIntersect(point, rayDirection,
                        triangle.WorldV0, triangle.WorldV1, triangle.WorldV2, out _))
                    {
                        Interlocked.Increment(ref intersectionCount);
                    }
                }
            }
        }

        void CheckFrozenEdit(string actionName)
        {
            if (IsFrozen)
            {
                throw new InvalidOperationException($"Cannot {actionName} when frozen.");
            }
        }

        /// <summary>
        /// For delegate.
        /// </summary>
        static bool IsBoundsIntersects(Bounds bounds, CollisionTriangle triangle)
        {
            return bounds.Intersects(triangle.WorldBounds);
        }

        public class TrianglesCollection : IList<CollisionTriangle>, IReadOnlyList<CollisionTriangle>
        {
            internal bool IsDirty { get; set; }

            public int Count => InnerList.Count;

            public int Capacity { get => InnerList.Capacity; set => InnerList.Capacity = value; }

            bool ICollection<CollisionTriangle>.IsReadOnly => ((ICollection<CollisionTriangle>)InnerList).IsReadOnly;

            public CollisionTriangle this[int index]
            {
                get => InnerList[index];
                set
                {
                    _owner.CheckFrozenEdit($"Indexer {nameof(TrianglesCollection)}");

                    if (InnerList[index] == value)
                    {
                        return;
                    }
                    InnerList[index] = value;
                    IsDirty = true;
                }
            }

            internal List<CollisionTriangle> InnerList { get; }

            readonly MeshShape _owner;

            public TrianglesCollection(MeshShape owner, int capacity)
            {
                _owner = owner;
                InnerList = new List<CollisionTriangle>(capacity);
            }

            public void Add(CollisionTriangle item)
            {
                _owner.CheckFrozenEdit($"{nameof(Add)} {nameof(TrianglesCollection)}");

                InnerList.Add(item);
                IsDirty = true;
            }

            public void AddRange(IEnumerable<CollisionTriangle> items)
            {
                _owner.CheckFrozenEdit($"{nameof(AddRange)} {nameof(TrianglesCollection)}");

                int count = InnerList.Count;
                InnerList.AddRange(items);
                IsDirty |= count != InnerList.Count;
            }

            public void Insert(int index, CollisionTriangle item)
            {
                _owner.CheckFrozenEdit($"{nameof(Insert)} {nameof(TrianglesCollection)}");

                InnerList.Insert(index, item);
                IsDirty = true;
            }

            public bool Remove(CollisionTriangle item)
            {
                _owner.CheckFrozenEdit($"{nameof(Add)} {nameof(TrianglesCollection)}");

                bool result = InnerList.Remove(item);
                IsDirty |= result;
                return result;
            }

            public void RemoveAt(int index)
            {
                _owner.CheckFrozenEdit($"{nameof(RemoveAt)} {nameof(TrianglesCollection)}");

                InnerList.RemoveAt(index);
                IsDirty = true;
            }

            public void Clear()
            {
                _owner.CheckFrozenEdit($"{nameof(Clear)} {nameof(TrianglesCollection)}");

                bool anyExists = InnerList.Count > 0;
                InnerList.Clear();
                IsDirty |= anyExists;
            }

            public void ClearInternal()
            {
                InnerList.Clear();
                IsDirty = false;
            }

            public bool Contains(CollisionTriangle item)
            {
                return InnerList.Contains(item);
            }

            public int IndexOf(CollisionTriangle item)
            {
                return InnerList.IndexOf(item);
            }

            public void CopyTo(CollisionTriangle[] array, int arrayIndex)
            {
                InnerList.CopyTo(array, arrayIndex);
            }

            public IEnumerator<CollisionTriangle> GetEnumerator()
            {
                return InnerList.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return InnerList.GetEnumerator();
            }
        }

        const int ParallelismThreshold = 128;
    }
}
