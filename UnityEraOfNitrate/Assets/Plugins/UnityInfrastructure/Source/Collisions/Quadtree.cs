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
    /// <summary>
    /// Please Check <see cref="Octree{T}"/> for details and usage.
    /// </summary>
    public class Quadtree<T>
    {
        public Rect Bounds { get; private set; } = new(Vector2.zero, Vector2.zero);

        List<Quadtree<T>>? _children;
        public bool IsLeaf => _children is null;

        public int Depth { get; private set; }

        List<T>? _items;
        public IReadOnlyList<T>? Items => _items;

        public IReadOnlyList<T> GetLeafItems()
        {
            if (!IsLeaf)
            {
                throw new InvalidOperationException("This quadtree is not a leaf but getting items.");
            }
            if (_items is null)
            {
                throw new InvalidOperationException("This quadtree is leaf but items list is null.");
            }
            return _items;
        }

        protected virtual void Reset()
        {
            Bounds = new Rect(Vector2.zero, Vector2.zero);
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
                _quadtreeListPool.Release(_children);
                _children = null;
            }
        }

        public static QuadtreeRoot<T> Create(int targetDepth)
        {
            if (targetDepth <= 0)
            {
                throw new ArgumentException("Quadtree depth cannot be 0 or negative.", nameof(targetDepth));
            }

            QuadtreeRoot<T> root = _quadtreeRootPool.Get();
            Populate(root, 0, targetDepth);

            root.UpdateLeaves();
            return root;


            static void Populate(Quadtree<T> parent, int depth, int targetDepth)
            {
                parent.Depth = depth;

                if (depth == targetDepth)
                {
                    return;
                }

                List<Quadtree<T>> children = _quadtreeListPool.Get();
                parent._children = children;

                for (int i = 0; i < 4; i++)
                {
                    Quadtree<T> child = _quadtreePool.Get();
                    children.Add(child);

                    Populate(child, depth + 1, targetDepth);
                }
            }
        }

        public static void Update(QuadtreeRoot<T> root, IReadOnlyList<T> itemSources, OrderablePartitioner<Tuple<int, int>>? sourcePartitioner, Rect sourcesTotalBounds, IsItemBoundsIntersectsDelegate isItemBoundsIntersects)
        {
            IReadOnlyList<Quadtree<T>> leaves = root.Leaves;

            UpdateBounds(root, sourcesTotalBounds);

            for (int i = 0; i < leaves.Count; i++)
            {
                Quadtree<T> leaf = leaves[i];

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
                            Quadtree<T> leaf = leaves[l];

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
                        Quadtree<T> leaf = leaves[l];

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

        public static bool ContainsItem(QuadtreeRoot<T> root, T item)
        {
            IReadOnlyList<Quadtree<T>> leaves = root.Leaves;

            for (int l = 0; l < leaves.Count; l++)
            {
                Quadtree<T> leaf = leaves[l];

                List<T> leafItems = leaf._items ?? throw new InvalidOperationException();

                if (leafItems.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool TryAddItem(QuadtreeRoot<T> root, T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects)
        {
            IReadOnlyList<Quadtree<T>> leaves = root.Leaves;

            for (int l = 0; l < leaves.Count; l++)
            {
                Quadtree<T> leaf = leaves[l];

                if (!isItemBoundsIntersects(leaf.Bounds, item))
                {
                    continue;
                }

                List<T> leafItems = leaf._items ?? throw new InvalidOperationException();

                leafItems.Add(item);
                return true;
            }
            return false;
        }

        public static bool RemoveItem(QuadtreeRoot<T> root, T item)
        {
            IReadOnlyList<Quadtree<T>> leaves = root.Leaves;

            for (int l = 0; l < leaves.Count; l++)
            {
                Quadtree<T> leaf = leaves[l];

                List<T> leafItems = leaf._items ?? throw new InvalidOperationException();

                if (leafItems.Remove(item))
                {
                    return true;
                }
            }
            return false;
        }

        public static int RemoveAllItems(QuadtreeRoot<T> root, Predicate<T> match)
        {
            IReadOnlyList<Quadtree<T>> leaves = root.Leaves;

            int count = 0;
            for (int l = 0; l < leaves.Count; l++)
            {
                Quadtree<T> leaf = leaves[l];

                List<T> leafItems = leaf._items ?? throw new InvalidOperationException();

                count += leafItems.RemoveAll(match);
            }
            return count;
        }

        public static void Search(QuadtreeRoot<T> root0, QuadtreeRoot<T> root1, bool allowSwap, QuadtreeSearchDelegate search)
        {
            QuadtreeRoot<T> searchRoot = root0, searchOther = root1;
            if (allowSwap &&
                root1.Leaves.Count > root0.Leaves.Count)
            {
                searchRoot = root1;
                searchOther = root0;
            }

            for (int i = 0; i < searchOther.Leaves.Count; i++)
            {
                Quadtree<T> otherLeaf = searchOther.Leaves[i];
                if (!PopulateSearch(searchRoot, otherLeaf, search))
                {
                    return;
                }
            }
        }
        static bool PopulateSearch(Quadtree<T> root, Quadtree<T> otherLeaf, QuadtreeSearchDelegate search)
        {
            if (!root.Bounds.Overlaps(otherLeaf.Bounds))
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
                    Quadtree<T> child = root._children[c];
                    if (!PopulateSearch(child, otherLeaf, search))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void Search(QuadtreeRoot<T> root, T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects, ItemSearchDelegate search)
        {
            PopulateSearch(root, item, isItemBoundsIntersects, search);
        }
        static bool PopulateSearch(Quadtree<T> root, T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects, ItemSearchDelegate search)
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
                    Quadtree<T> child = root._children[c];
                    if (!PopulateSearch(child, item, isItemBoundsIntersects, search))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void Search(QuadtreeRoot<T> root, IsBoundsIntersectsDelegate isBoundsIntersects, SearchDelegate search)
        {
            PopulateSearch(root, isBoundsIntersects, search);
        }
        static bool PopulateSearch(Quadtree<T> root, IsBoundsIntersectsDelegate isBoundsIntersects, SearchDelegate search)
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
                    Quadtree<T> child = root._children[c];
                    if (!PopulateSearch(child, isBoundsIntersects, search))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void Release(ref QuadtreeRoot<T>? root)
        {
            if (root is null)
            {
                return;
            }
            _quadtreeRootPool.Release(root);
            root = null;
        }

        static void UpdateBounds(Quadtree<T> root, Rect source)
        {
            root.Bounds = source;
            Populate(root);
            return;

            static void Populate(Quadtree<T> parent)
            {
                List<Quadtree<T>>? children = parent._children;
                if (children is null)
                {
                    return;
                }

                UpdateBounds(children, parent.Bounds);

                for (int i = 0; i < children.Count; i++)
                {
                    Populate(children[i]);
                }
            }
        }

        static void UpdateBounds(IReadOnlyList<Quadtree<T>> quadtrees, Rect source)
        {
            Vector2 halfSize = source.size * 0.5f;

            Vector2 epsilon = CollisionEx.BoundsEpsilon2;

            Vector2 min = source.min;
            Rect bounds = new(min, halfSize);
            bounds.min -= epsilon;
            bounds.max += epsilon;
            quadtrees[0].Bounds = bounds;

            bounds = new(new Vector2(min.x + halfSize.x, min.y), halfSize);
            bounds.min -= epsilon;
            bounds.max += epsilon;
            quadtrees[1].Bounds = bounds;

            bounds = new(new Vector2(min.x, min.y + halfSize.y), halfSize);
            bounds.min -= epsilon;
            bounds.max += epsilon;
            quadtrees[2].Bounds = bounds;

            bounds = new(min + halfSize, halfSize);
            bounds.min -= epsilon;
            bounds.max += epsilon;
            quadtrees[3].Bounds = bounds;
        }

        protected static void CollectLeaves(Quadtree<T> root, List<Quadtree<T>> buffer)
        {
            Populate(root);
            return;

            void Populate(Quadtree<T> root)
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

        class QuadtreePool : ObjectPool<Quadtree<T>>
        {
            public QuadtreePool() : base(isThreadSafe: true)
            {
            }

            protected override void Activate(Quadtree<T> obj)
            {
                obj.Reset();
            }

            protected override void Deactivate(Quadtree<T> obj)
            {
                obj.Reset();
            }
        }
        class QuadtreeRootPool : ObjectPool<QuadtreeRoot<T>>
        {
            public QuadtreeRootPool() : base(isThreadSafe: true)
            {
            }

            protected override void Activate(QuadtreeRoot<T> obj)
            {
                obj.Reset();
            }
            protected override void Deactivate(QuadtreeRoot<T> obj)
            {
                obj.Reset();
            }
        }

        /// <returns>Return <c>false</c> to abort.</returns>
        public delegate bool QuadtreeSearchDelegate(Quadtree<T> leaf0, Quadtree<T> leaf1);
        /// <returns>Return <c>true</c> if the <paramref name="item"/> is intersects with the <paramref name="quadtreeBounds"/>.</returns>
        /// <remarks>
        /// Shallow-test is enough for the result such as <see cref="Rect.Overlaps(Rect)"/>.
        /// </remarks>
        public delegate bool IsItemBoundsIntersectsDelegate(Rect quadtreeBounds, T item);
        /// <returns>Return <c>false</c> to abort.</returns>
        public delegate bool ItemSearchDelegate(T item, Quadtree<T> leaf);
        /// <returns>Return <c>true</c> if something is intersects with the <paramref name="quadtreeBounds"/>.</returns>
        public delegate bool IsBoundsIntersectsDelegate(Rect quadtreeBounds);
        /// <returns>Return <c>false</c> to abort.</returns>
        public delegate bool SearchDelegate(Quadtree<T> leaf);

        static readonly QuadtreePool _quadtreePool = new();
        static readonly QuadtreeRootPool _quadtreeRootPool = new();
        static readonly ListPool<Quadtree<T>> _quadtreeListPool = new(4, isThreadSafe: true);
        static readonly ListPool<T> _itemListPool = new(isThreadSafe: true);
    }

    public class QuadtreeRoot<T> : Quadtree<T>
    {
        readonly List<Quadtree<T>> _leaves = new();
        public IReadOnlyList<Quadtree<T>> Leaves => _leaves;

        public void Update(IReadOnlyList<T> itemSources, OrderablePartitioner<Tuple<int, int>>? sourcePartitioner, Rect sourcesTotalBounds, IsItemBoundsIntersectsDelegate isItemBoundsIntersects)
        {
            Update(this, itemSources, sourcePartitioner, sourcesTotalBounds, isItemBoundsIntersects);
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
