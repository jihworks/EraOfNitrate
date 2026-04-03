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
        public Rect Bounds { get; internal set; } = new(Vector2.zero, Vector2.zero);

        internal List<Quadtree<T>>? ChildrenInternal { get; set; }
        public bool IsLeaf => ChildrenInternal is null;

        public int Depth { get; private set; }

        internal List<T>? ItemsInternal { get; set; }
        public IReadOnlyList<T>? Items => ItemsInternal;

        public IReadOnlyList<T> GetLeafItems()
        {
            if (!IsLeaf)
            {
                throw new InvalidOperationException("This quadtree is not a leaf but getting items.");
            }
            if (ItemsInternal is null)
            {
                throw new InvalidOperationException("This quadtree is leaf but items list is null.");
            }
            return ItemsInternal;
        }

        protected virtual void Reset()
        {
            Bounds = new Rect(Vector2.zero, Vector2.zero);
            Depth = 0;

            if (ItemsInternal is not null)
            {
                ItemsInternal.Clear();
                _itemListPool.Release(ItemsInternal);
                ItemsInternal = null;
            }

            if (ChildrenInternal is not null)
            {
                for (int i = 0; i < ChildrenInternal.Count; i++)
                {
                    ChildrenInternal[i].Reset();
                }
                ChildrenInternal.Clear();
                _quadtreeListPool.Release(ChildrenInternal);
                ChildrenInternal = null;
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
                parent.ChildrenInternal = children;

                for (int i = 0; i < 4; i++)
                {
                    Quadtree<T> child = _quadtreePool.Get();
                    children.Add(child);

                    Populate(child, depth + 1, targetDepth);
                }
            }
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

        internal class QuadtreePool : ObjectPool<Quadtree<T>>
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
        internal class QuadtreeRootPool : ObjectPool<QuadtreeRoot<T>>
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

        internal static readonly QuadtreePool _quadtreePool = new();
        internal static readonly QuadtreeRootPool _quadtreeRootPool = new();
        internal static readonly ListPool<Quadtree<T>> _quadtreeListPool = new(4, isThreadSafe: true);
        internal static readonly ListPool<T> _itemListPool = new(isThreadSafe: true);
    }

    public sealed class QuadtreeRoot<T> : Quadtree<T>
    {
        readonly List<Quadtree<T>> _leaves = new();
        public IReadOnlyList<Quadtree<T>> Leaves => _leaves;

        protected override void Reset()
        {
            _leaves.Clear();
            
            base.Reset();
        }

        internal void UpdateLeaves()
        {
            _leaves.Clear();
            Populate(this, _leaves);
            return;


            static void Populate(Quadtree<T> root, List<Quadtree<T>> buffer)
            {
                if (root.IsLeaf)
                {
                    buffer.Add(root);
                }
                if (root.ChildrenInternal is not null)
                {
                    for (int c = 0; c < root.ChildrenInternal.Count; c++)
                    {
                        Populate(root.ChildrenInternal[c], buffer);
                    }
                }
            }
        }

        public void Update(IReadOnlyList<T> itemSources, OrderablePartitioner<Tuple<int, int>>? sourcePartitioner, Rect sourcesTotalBounds, IsItemBoundsIntersectsDelegate isItemBoundsIntersects)
        {
            IReadOnlyList<Quadtree<T>> leaves = Leaves;

            UpdateBounds(this, sourcesTotalBounds);

            for (int i = 0; i < leaves.Count; i++)
            {
                Quadtree<T> leaf = leaves[i];

                if (leaf.ItemsInternal is null)
                {
                    leaf.ItemsInternal = _itemListPool.Get();
                }
                else
                {
                    leaf.ItemsInternal.Clear();
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

                            List<T> leafItems = leaf.ItemsInternal ?? throw new InvalidOperationException();

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

                        List<T> leafItems = leaf.ItemsInternal ?? throw new InvalidOperationException();

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

        public bool ContainsItem(T item)
        {
            IReadOnlyList<Quadtree<T>> leaves = Leaves;

            for (int l = 0; l < leaves.Count; l++)
            {
                Quadtree<T> leaf = leaves[l];

                List<T> leafItems = leaf.ItemsInternal ?? throw new InvalidOperationException();

                if (leafItems.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        public bool TryAddItem(T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects)
        {
            IReadOnlyList<Quadtree<T>> leaves = Leaves;

            for (int l = 0; l < leaves.Count; l++)
            {
                Quadtree<T> leaf = leaves[l];

                if (!isItemBoundsIntersects(leaf.Bounds, item))
                {
                    continue;
                }

                List<T> leafItems = leaf.ItemsInternal ?? throw new InvalidOperationException();

                leafItems.Add(item);
                return true;
            }
            return false;
        }

        public bool RemoveItem(T item)
        {
            IReadOnlyList<Quadtree<T>> leaves = Leaves;

            for (int l = 0; l < leaves.Count; l++)
            {
                Quadtree<T> leaf = leaves[l];

                List<T> leafItems = leaf.ItemsInternal ?? throw new InvalidOperationException();

                if (leafItems.Remove(item))
                {
                    return true;
                }
            }
            return false;
        }

        public int RemoveAllItems(Predicate<T> match)
        {
            IReadOnlyList<Quadtree<T>> leaves = Leaves;

            int count = 0;
            for (int l = 0; l < leaves.Count; l++)
            {
                Quadtree<T> leaf = leaves[l];

                List<T> leafItems = leaf.ItemsInternal ?? throw new InvalidOperationException();

                count += leafItems.RemoveAll(match);
            }
            return count;
        }

        public void Search(QuadtreeRoot<T> other, bool allowSwap, QuadtreeSearchDelegate search)
        {
            QuadtreeRoot<T> searchRoot = this, searchOther = other;
            if (allowSwap &&
                other.Leaves.Count > Leaves.Count)
            {
                searchRoot = other;
                searchOther = this;
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

            if (root.ChildrenInternal is not null)
            {
                for (int c = 0; c < root.ChildrenInternal.Count; c++)
                {
                    Quadtree<T> child = root.ChildrenInternal[c];
                    if (!PopulateSearch(child, otherLeaf, search))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void Search(T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects, ItemSearchDelegate search)
        {
            PopulateSearch(this, item, isItemBoundsIntersects, search);
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

            if (root.ChildrenInternal is not null)
            {
                for (int c = 0; c < root.ChildrenInternal.Count; c++)
                {
                    Quadtree<T> child = root.ChildrenInternal[c];
                    if (!PopulateSearch(child, item, isItemBoundsIntersects, search))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void Search(IsBoundsIntersectsDelegate isBoundsIntersects, SearchDelegate search)
        {
            PopulateSearch(this, isBoundsIntersects, search);
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

            if (root.ChildrenInternal is not null)
            {
                for (int c = 0; c < root.ChildrenInternal.Count; c++)
                {
                    Quadtree<T> child = root.ChildrenInternal[c];
                    if (!PopulateSearch(child, isBoundsIntersects, search))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        static void UpdateBounds(Quadtree<T> root, Rect source)
        {
            root.Bounds = source;
            Populate(root);
            return;

            static void Populate(Quadtree<T> parent)
            {
                List<Quadtree<T>>? children = parent.ChildrenInternal;
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
    }
}
