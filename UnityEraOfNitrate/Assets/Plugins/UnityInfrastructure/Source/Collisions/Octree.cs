// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.Unity.Infrastructure.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Jih.Unity.Infrastructure.Collisions
{
    public class Octree<T>
    {
        public Bounds Bounds { get; private set; } = new(Vector3.zero, Vector3.zero);

        List<Octree<T>>? _children;
        public bool IsLeaf => _children is null;

        /// <summary>
        /// Root is <c>0</c>, increases when go down the tree.
        /// </summary>
        public int Depth { get; private set; }

        List<T>? _items;
        /// <remarks>
        /// Not <c>null</c> when this octree is leaf AND had been updated.
        /// </remarks>
        /// <seealso cref="GetLeafItems"/>
        public IReadOnlyList<T>? Items => _items;

        /// <exception cref="InvalidOperationException">When this octree is not a leaf <b>or</b> <see cref="Items"/> is <c>null</c> even though this octree is leaf because never updated.</exception>
        public IReadOnlyList<T> GetLeafItems()
        {
            if (!IsLeaf)
            {
                throw new InvalidOperationException("This octree is not a leaf but getting items.");
            }
            if (_items is null)
            {
                throw new InvalidOperationException("This octree is leaf but items list is null.");
            }
            return _items;
        }

        protected virtual void Reset()
        {
            Bounds = new Bounds(Vector3.zero, Vector3.zero);
            Depth = 0;

            if (_items is not null)
            {
                _items.Clear();
                _itemListPool.Release(_items);
                _items = null;
            }

            if (_children is not null)
            {
                for (int i = 0; i < _children.Count; i++)
                {
                    _children[i].Reset();
                }
                _children.Clear();
                _octreeListPool.Release(_children);
                _children = null;
            }
        }

        public static OctreeRoot<T> Create(int targetDepth)
        {
            if (targetDepth <= 0)
            {
                throw new ArgumentException("Octree depth cannot be 0 or negative.");
            }

            OctreeRoot<T> root = _octreeRootPool.Get();
            Populate(root, 0, targetDepth);

            root.UpdateLeaves();
            return root;


            static void Populate(Octree<T> parent, int depth, int targetDepth)
            {
                parent.Depth = depth;

                if (depth == targetDepth)
                {
                    return;
                }

                List<Octree<T>> children = _octreeListPool.Get();
                parent._children = children;

                for (int i = 0; i < 8; i++)
                {
                    Octree<T> child = _octreePool.Get();
                    children.Add(child);

                    Populate(child, depth + 1, targetDepth);
                }
            }
        }

        /// <param name="root">Octree to update.</param>
        /// <param name="itemSources">Items to hold by the <paramref name="root"/>.</param>
        /// <param name="sourcePartitioner">Partitioner for looping all of <paramref name="itemSources"/> with parallelism. If <c>null</c>, will use standard <c>for</c> loop.</param>
        /// <param name="sourcesTotalBounds">A bounds which must contains all of <paramref name="itemSources"/>.</param>
        /// <remarks>
        /// This method is garbage-free.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Throws when an item was not added to any octree.<br/>
        /// This may occur when the <paramref name="sourcesTotalBounds"/> does not contains all of <paramref name="itemSources"/>. Then, the item wasn't passed any <paramref name="isItemBoundsIntersects"/>.
        /// </exception>
        public static void Update(OctreeRoot<T> root, IReadOnlyList<T> itemSources, OrderablePartitioner<Tuple<int, int>>? sourcePartitioner, Bounds sourcesTotalBounds, IsItemBoundsIntersectsDelegate isItemBoundsIntersects)
        {
            IReadOnlyList<Octree<T>> leaves = root.Leaves;

            UpdateBounds(root, sourcesTotalBounds);

            for (int i = 0; i < leaves.Count; i++)
            {
                Octree<T> leaf = leaves[i];

                if (leaf._items is null)
                {
                    leaf._items = _itemListPool.Get();
                }
                else
                {
                    leaf._items.Clear();
                }
            }

            if (sourcePartitioner is not null)
            {
                Parallel.ForEach(sourcePartitioner, (range, state) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        T item = itemSources[i];

                        bool anyAdded = false;
                        for (int l = 0; l < leaves.Count; l++)
                        {
                            Octree<T> leaf = leaves[l];

                            if (!isItemBoundsIntersects(leaf.Bounds, item))
                            {
                                continue;
                            }

                            List<T> leafItems = leaf._items ?? throw new InvalidOperationException();

                            lock (leafItems)
                            {
                                leafItems.Add(item);
                            }
                            anyAdded = true;
                        }

                        if (!anyAdded)
                        {
                            throw new InvalidOperationException("An item was not added to any octree.");
                        }
                    }
                });
            }
            else
            {
                for (int i = 0; i < itemSources.Count; i++)
                {
                    T item = itemSources[i];

                    bool anyAdded = false;
                    for (int l = 0; l < leaves.Count; l++)
                    {
                        Octree<T> leaf = leaves[l];

                        if (!isItemBoundsIntersects(leaf.Bounds, item))
                        {
                            continue;
                        }

                        List<T> leafItems = leaf._items ?? throw new InvalidOperationException();

                        leafItems.Add(item);
                        anyAdded = true;
                    }

                    if (!anyAdded)
                    {
                        throw new InvalidOperationException("An item was not added to any octree.");
                    }
                }
            }
        }

        /// <summary>Search octree versus octree.</summary>
        /// <param name="allowSwap">
        /// Whether allowing swap <paramref name="root0"/> and <paramref name="root1"/> to increase performance.<br/>
        /// If <c>true</c>, input arguments order for <paramref name="search"/> can be different with this method input arguments.
        /// </param>
        public static void Search(OctreeRoot<T> root0, OctreeRoot<T> root1, bool allowSwap, OctreeSearchDelegate search)
        {
            OctreeRoot<T> searchRoot = root0, searchOther = root1;
            if (allowSwap &&
                root1.Leaves.Count > root0.Leaves.Count)
            {
                // Swapping for performance.
                // The search will be performed in the tree with more leaves (deeper tree).
                searchRoot = root1;
                searchOther = root0;
            }

            for (int i = 0; i < searchOther.Leaves.Count; i++)
            {
                Octree<T> otherLeaf = searchOther.Leaves[i];
                if (!PopulateSearch(searchRoot, otherLeaf, search))
                {
                    return;
                }
            }
        }
        static bool PopulateSearch(Octree<T> root, Octree<T> otherLeaf, OctreeSearchDelegate search)
        {
            if (!root.Bounds.Intersects(otherLeaf.Bounds))
            {
                return true;
            }

            if (root.IsLeaf && !search(root, otherLeaf))
            {
                return false;
            }

            if (root._children is not null)
            {
                for (int c = 0; c < root._children.Count; c++)
                {
                    Octree<T> child = root._children[c];
                    if (!PopulateSearch(child, otherLeaf, search))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Search octree versus item.
        /// </summary>
        /// <param name="item">Searching item.</param>
        public static void Search(OctreeRoot<T> root, T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects, ItemSearchDelegate search)
        {
            PopulateSearch(root, item, isItemBoundsIntersects, search);
        }
        static bool PopulateSearch(Octree<T> root, T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects, ItemSearchDelegate search)
        {
            if (!isItemBoundsIntersects(root.Bounds, item))
            {
                return true;
            }

            if (root.IsLeaf && !search(item, root))
            {
                return false;
            }

            if (root._children is not null)
            {
                for (int c = 0; c < root._children.Count; c++)
                {
                    Octree<T> child = root._children[c];
                    if (!PopulateSearch(child, item, isItemBoundsIntersects, search))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Search octree but the caller have to handle concrete mechanism.
        /// </summary>
        public static void Search(OctreeRoot<T> root, IsBoundsIntersectsDelegate isBoundsIntersects, SearchDelegate search)
        {
            PopulateSearch(root, isBoundsIntersects, search);
        }
        static bool PopulateSearch(Octree<T> root, IsBoundsIntersectsDelegate isBoundsIntersects, SearchDelegate search)
        {
            if (!isBoundsIntersects(root.Bounds))
            {
                return true;
            }

            if (root.IsLeaf && !search(root))
            {
                return false;
            }

            if (root._children is not null)
            {
                for (int c = 0; c < root._children.Count; c++)
                {
                    Octree<T> child = root._children[c];
                    if (!PopulateSearch(child, isBoundsIntersects, search))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void Release(ref OctreeRoot<T>? root)
        {
            if (root is null)
            {
                return;
            }
            _octreeRootPool.Release(root);
            root = null;
        }

        static void UpdateBounds(Octree<T> root, Bounds source)
        {
            root.Bounds = source;
            Populate(root);
            return;

            static void Populate(Octree<T> parent)
            {
                List<Octree<T>>? children = parent._children;
                if (children is null)
                {
                    return;
                }

                UpdateBounds(children, parent.Bounds);

                for (int i = 0; i < children.Count; i++)
                {
                    Octree<T> child = children[i];
                    Populate(child);
                }
            }
        }

        static void UpdateBounds(IReadOnlyList<Octree<T>> octrees, Bounds source)
        {
            Vector3 halfSize = source.size * 0.5f;

            Vector3 epsilon = CollisionEx.BoundsEpsilon3;

            Vector3 min = source.min;
            Bounds bounds = default;
            bounds.SetMinMax(min, min + halfSize);
            bounds.Expand(epsilon);
            octrees[0].Bounds = bounds;

            min = source.min;
            min.x += halfSize.x;
            bounds = default;
            bounds.SetMinMax(min, min + halfSize);
            bounds.Expand(epsilon);
            octrees[1].Bounds = bounds;

            min = source.min;
            min.z += halfSize.z;
            bounds = default;
            bounds.SetMinMax(min, min + halfSize);
            bounds.Expand(epsilon);
            octrees[2].Bounds = bounds;

            min = source.min;
            min.x += halfSize.x;
            min.z += halfSize.z;
            bounds = default;
            bounds.SetMinMax(min, min + halfSize);
            bounds.Expand(epsilon);
            octrees[3].Bounds = bounds;


            min = source.min;
            min.y += halfSize.y;
            bounds = default;
            bounds.SetMinMax(min, min + halfSize);
            bounds.Expand(epsilon);
            octrees[4].Bounds = bounds;

            min = source.min;
            min.y += halfSize.y;
            min.x += halfSize.x;
            bounds = default;
            bounds.SetMinMax(min, min + halfSize);
            bounds.Expand(epsilon);
            octrees[5].Bounds = bounds;

            min = source.min;
            min.y += halfSize.y;
            min.z += halfSize.z;
            bounds = default;
            bounds.SetMinMax(min, min + halfSize);
            bounds.Expand(epsilon);
            octrees[6].Bounds = bounds;

            min = source.min;
            min.y += halfSize.y;
            min.x += halfSize.x;
            min.z += halfSize.z;
            bounds = default;
            bounds.SetMinMax(min, min + halfSize);
            bounds.Expand(epsilon);
            octrees[7].Bounds = bounds;
        }

        protected static void CollectLeaves(Octree<T> root, List<Octree<T>> buffer)
        {
            Populate(root);
            return;

            void Populate(Octree<T> root)
            {
                if (root.IsLeaf)
                {
                    buffer.Add(root);
                }
                if (root._children is not null)
                {
                    for (int c = 0; c < root._children.Count; c++)
                    {
                        Populate(root._children[c]);
                    }
                }
            }
        }

        class OctreePool : ObjectPool<Octree<T>>
        {
            public OctreePool() : base(isThreadSafe: true)
            {
            }

            protected override void Activate(Octree<T> obj)
            {
                base.Activate(obj);
                obj.Reset();
            }

            protected override void Deactivate(Octree<T> obj)
            {
                base.Deactivate(obj);
                obj.Reset();
            }
        }

        class OctreeRootPool : ObjectPool<OctreeRoot<T>>
        {
            public OctreeRootPool() : base(isThreadSafe: true)
            {
            }

            protected override void Activate(OctreeRoot<T> obj)
            {
                base.Activate(obj);
                obj.Reset();
            }

            protected override void Deactivate(OctreeRoot<T> obj)
            {
                base.Deactivate(obj);
                obj.Reset();
            }
        }

        /// <returns>Return <c>false</c> to abort.</returns>
        public delegate bool OctreeSearchDelegate(Octree<T> leaf0, Octree<T> leaf1);
        /// <returns>Return <c>true</c> if the <paramref name="item"/> is intersects with the <paramref name="octreeBounds"/>.</returns>
        /// <remarks>
        /// Shallow-test is enough for the result such as <see cref="Bounds.Intersects(Bounds)"/>.
        /// </remarks>
        public delegate bool IsItemBoundsIntersectsDelegate(Bounds octreeBounds, T item);
        /// <returns>Return <c>false</c> to abort.</returns>
        public delegate bool ItemSearchDelegate(T item, Octree<T> leaf);
        /// <returns>Return <c>true</c> if something is intersects with the <paramref name="octreeBounds"/>.</returns>
        public delegate bool IsBoundsIntersectsDelegate(Bounds octreeBounds);
        /// <returns>Return <c>false</c> to abort.</returns>
        public delegate bool SearchDelegate(Octree<T> leaf);

        static readonly OctreePool _octreePool = new();
        static readonly OctreeRootPool _octreeRootPool = new();
        static readonly ListPool<Octree<T>> _octreeListPool = new(listCapacity: 8, isThreadSafe: true);
        static readonly ListPool<T> _itemListPool = new(isThreadSafe: true);
    }

    public class OctreeRoot<T> : Octree<T>
    {
        /// <summary>
        /// If this octree is root, it has a list of leaf nodes.
        /// </summary>
        readonly List<Octree<T>> _leaves = new();
        public IReadOnlyList<Octree<T>> Leaves => _leaves;

        public void Update(IReadOnlyList<T> itemSources, OrderablePartitioner<Tuple<int, int>>? sourcePartitioner, Bounds sourcesTotalBounds, IsItemBoundsIntersectsDelegate isItemBoundsIntersects)
        {
            Update(this, itemSources, sourcePartitioner, sourcesTotalBounds, isItemBoundsIntersects);
        }

        public void Search(OctreeRoot<T> root1, bool allowSwap, OctreeSearchDelegate search)
        {
            Search(this, root1, allowSwap, search);
        }

        public void Search(T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects, ItemSearchDelegate search)
        {
            Search(this, item, isItemBoundsIntersects, search);
        }

        public void Search(IsBoundsIntersectsDelegate isBoundsIntersects, SearchDelegate search)
        {
            Search(this, isBoundsIntersects, search);
        }

        protected override void Reset()
        {
            _leaves.Clear();

            base.Reset();
        }

        internal void UpdateLeaves()
        {
            _leaves.Clear();
            CollectLeaves(this, _leaves);
        }
    }
}
