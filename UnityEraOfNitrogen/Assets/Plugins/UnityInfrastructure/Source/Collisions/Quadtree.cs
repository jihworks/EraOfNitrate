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
    public abstract class QuadtreeNode<T>
    {
        public Rect Rect { get; internal set; } = new(Vector2.zero, Vector2.zero);

        /// <summary>
        /// Root is <c>0</c>, increases when go down the tree.
        /// </summary>
        public int Depth { get; internal set; }

        internal virtual void Reset()
        {
            Rect = new Rect(Vector2.zero, Vector2.zero);
            Depth = 0;
        }
    }

    public class QuadtreeContainerNode<T> : QuadtreeNode<T>
    {
        internal List<QuadtreeNode<T>> ChildrenInternal { get; } = new();
        public IReadOnlyList<QuadtreeNode<T>> Children => ChildrenInternal;

        internal override void Reset()
        {
            for (int i = 0; i < ChildrenInternal.Count; i++)
            {
                ChildrenInternal[i].Reset();
            }
            ChildrenInternal.Clear();

            base.Reset();
        }
    }

    public sealed class QuadtreeLeafNode<T> : QuadtreeNode<T>
    {
        readonly List<T> _items = new();
        public List<T> Items => _items;

        internal override void Reset()
        {
            _items.Clear();

            base.Reset();
        }
    }

    public sealed class Quadtree<T> : QuadtreeContainerNode<T>
    {
        public static Quadtree<T> Create(int targetDepth)
        {
            if (targetDepth <= 0)
            {
                throw new ArgumentException("Quadtree depth cannot be 0 or negative.", nameof(targetDepth));
            }

            Quadtree<T> root = _quadtreePool.Get();
            root.Depth = 0;
            Populate(root, root, 1, targetDepth);
            return root;


            static void Populate(Quadtree<T> root, QuadtreeContainerNode<T> parent, int currentDepth, int targetDepth)
            {
                if (currentDepth == targetDepth)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        QuadtreeLeafNode<T> child = _quadtreeLeafPool.Get();
                        child.Depth = currentDepth;
                        parent.ChildrenInternal.Add(child);

                        root.LeavesInternal.Add(child);
                    }
                    return;
                }

                for (int i = 0; i < 4; i++)
                {
                    QuadtreeContainerNode<T> child = _quadtreeContainerPool.Get();
                    child.Depth = currentDepth;
                    parent.ChildrenInternal.Add(child);

                    Populate(root, child, currentDepth + 1, targetDepth);
                }
            }
        }

        public static void Release(ref Quadtree<T>? root)
        {
            if (root is null)
            {
                return;
            }
            _quadtreePool.Release(root);
            root = null;
        }

        internal List<QuadtreeLeafNode<T>> LeavesInternal { get; } = new();
        public IReadOnlyList<QuadtreeLeafNode<T>> Leaves => LeavesInternal;

        internal override void Reset()
        {
            LeavesInternal.Clear();

            base.Reset();
        }

        /// <param name="itemSources">Items to hold by the <paramref name="root"/>.</param>
        /// <param name="sourcePartitioner">Partitioner for looping all of <paramref name="itemSources"/> with parallelism. If <c>null</c>, will use standard <c>for</c> loop.</param>
        /// <param name="sourcesTotalBounds">A bounds which must contains all of <paramref name="itemSources"/>.</param>
        /// <remarks>
        /// This method is garbage-free.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Throws when an item was not added to any quadtree.<br/>
        /// This may occur when the <paramref name="sourcesTotalBounds"/> does not contains all of <paramref name="itemSources"/>. Then, the item wasn't passed any <paramref name="isItemBoundsIntersects"/>.
        /// </exception>
        public void Update(IReadOnlyList<T> itemSources, OrderablePartitioner<Tuple<int, int>>? sourcePartitioner, Rect sourcesTotalBounds, IsItemBoundsIntersectsDelegate isItemBoundsIntersects)
        {
            UpdateBounds(this, sourcesTotalBounds);

            for (int i = 0; i < LeavesInternal.Count; i++)
            {
                QuadtreeLeafNode<T> leaf = LeavesInternal[i];

                leaf.Items.Clear();
            }

            if (sourcePartitioner is not null)
            {
                Parallel.ForEach(sourcePartitioner, (range, state) =>
                {
                    for (int i = range.Item1; i < range.Item2; i++)
                    {
                        T item = itemSources[i];

                        if (!TryAddItemInternal(this, item, isItemBoundsIntersects, true))
                        {
                            throw new InvalidOperationException("An item was not added to any quadtree.");
                        }
                    }
                });
            }
            else
            {
                for (int i = 0; i < itemSources.Count; i++)
                {
                    T item = itemSources[i];

                    if (!TryAddItemInternal(this, item, isItemBoundsIntersects, false))
                    {
                        throw new InvalidOperationException("An item was not added to any quadtree.");
                    }
                }
            }
        }

        /// <summary>
        /// Try to add an item to the quadtree without updating bounds.
        /// </summary>
        /// <remarks>
        /// This is useful for the case that the quadtree bounds already cover whole expecting space and spawning some items to search.<br/>
        /// This method does <c>NOT</c> check duplication.
        /// </remarks>
        /// <returns>Whether the <paramref name="item"/> was successfully added to the quadtree.</returns>
        public bool TryAddItem(T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects)
        {
            return TryAddItemInternal(this, item, isItemBoundsIntersects, false);
        }

        static bool TryAddItemInternal(Quadtree<T> root, T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects, bool useLock)
        {
            return Populate(root);


            bool Populate(QuadtreeNode<T> self)
            {
                if (!isItemBoundsIntersects(self.Rect, item))
                {
                    return false;
                }

                if (self is QuadtreeLeafNode<T> selfLeaf)
                {
                    List<T> leafItems = selfLeaf.Items;
                    if (useLock)
                    {
                        lock (leafItems)
                        {
                            leafItems.Add(item);
                        }
                    }
                    else
                    {
                        leafItems.Add(item);
                    }
                    return true;
                }

                if (self is QuadtreeContainerNode<T> selfContainer)
                {
                    IReadOnlyList<QuadtreeNode<T>> children = selfContainer.Children;

                    bool addedToAny = false;
                    for (int c = 0; c < children.Count; c++)
                    {
                        addedToAny |= Populate(children[c]);
                    }
                    return addedToAny;
                }

                return false;
            }
        }

        /// <summary>
        /// Remove an item from the quadtree without updating bounds.
        /// </summary>
        /// <remarks>
        /// This method will search the item in whole quadtree and remove it all.
        /// </remarks>
        /// <returns>Whether the <paramref name="item"/> was found and removed.</returns>
        public bool RemoveItem(T item)
        {
            IReadOnlyList<QuadtreeLeafNode<T>> leaves = Leaves;

            bool anyRemoved = false;
            for (int l = 0; l < leaves.Count; l++)
            {
                QuadtreeLeafNode<T> leaf = leaves[l];

                anyRemoved |= leaf.Items.Remove(item);
            }
            return anyRemoved;
        }

        /// <summary>
        /// Remove items from the quadtree without updating bounds.
        /// </summary>
        /// <returns>Total sum of removed items count.</returns>
        public int RemoveAllItems(Predicate<T> match)
        {
            IReadOnlyList<QuadtreeLeafNode<T>> leaves = Leaves;

            int count = 0;
            for (int l = 0; l < leaves.Count; l++)
            {
                QuadtreeLeafNode<T> leaf = leaves[l];

                List<T> leafItems = leaf.Items;

                count += leafItems.RemoveAll(match);
            }
            return count;
        }

        /// <returns>Whether the <paramref name="item"/> was found in any leaf quadtree.</returns>
        public bool ContainsItem(T item)
        {
            IReadOnlyList<QuadtreeLeafNode<T>> leaves = Leaves;

            for (int l = 0; l < leaves.Count; l++)
            {
                QuadtreeLeafNode<T> leaf = leaves[l];

                List<T> leafItems = leaf.Items;

                if (leafItems.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Search quadtree versus quadtree.</summary>
        /// <param name="allowSwap">
        /// Whether allowing swap <c>this</c> and <paramref name="other"/> to increase performance.<br/>
        /// If <c>true</c>, input arguments order for <paramref name="search"/> can be different with <c>this</c> after <paramref name="other"/> order.
        /// </param>
        public void Search(Quadtree<T> other, bool allowSwap, QuadtreeSearchDelegate search)
        {
            Quadtree<T> searchRoot = this, searchOther = other;
            if (allowSwap &&
                other.Leaves.Count > Leaves.Count)
            {
                // Swapping for performance.
                // The search will be performed in the tree with more leaves (deeper tree).
                searchRoot = other;
                searchOther = this;
            }

            for (int i = 0; i < searchOther.Leaves.Count; i++)
            {
                QuadtreeLeafNode<T> otherLeaf = searchOther.Leaves[i];
                if (!PopulateSearch(searchRoot, otherLeaf, search))
                {
                    return;
                }
            }
        }
        static bool PopulateSearch(QuadtreeNode<T> self, QuadtreeLeafNode<T> otherLeaf, QuadtreeSearchDelegate search)
        {
            if (!self.Rect.Overlaps(otherLeaf.Rect))
            {
                return true;
            }

            if (self is QuadtreeLeafNode<T> selfLeaf && !search(selfLeaf, otherLeaf))
            {
                return false;
            }

            if (self is QuadtreeContainerNode<T> selfContainer)
            {
                for (int c = 0; c < selfContainer.Children.Count; c++)
                {
                    QuadtreeNode<T> child = selfContainer.Children[c];
                    if (!PopulateSearch(child, otherLeaf, search))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Search quadtree versus item.
        /// </summary>
        /// <param name="item">Searching item.</param>
        public void Search(T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects, ItemSearchDelegate search)
        {
            PopulateSearch(this, item, isItemBoundsIntersects, search);
        }
        static bool PopulateSearch(QuadtreeNode<T> self, T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects, ItemSearchDelegate search)
        {
            if (!isItemBoundsIntersects(self.Rect, item))
            {
                return true;
            }

            if (self is QuadtreeLeafNode<T> selfLeaf && !search(item, selfLeaf))
            {
                return false;
            }

            if (self is QuadtreeContainerNode<T> selfContainer)
            {
                for (int c = 0; c < selfContainer.Children.Count; c++)
                {
                    QuadtreeNode<T> child = selfContainer.Children[c];
                    if (!PopulateSearch(child, item, isItemBoundsIntersects, search))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Search quadtree but the caller have to handle concrete mechanism.
        /// </summary>
        public void Search(IsBoundsIntersectsDelegate isBoundsIntersects, SearchDelegate search)
        {
            PopulateSearch(this, isBoundsIntersects, search);
        }
        static bool PopulateSearch(QuadtreeNode<T> self, IsBoundsIntersectsDelegate isBoundsIntersects, SearchDelegate search)
        {
            if (!isBoundsIntersects(self.Rect))
            {
                return true;
            }

            if (self is QuadtreeLeafNode<T> selfLeaf && !search(selfLeaf))
            {
                return false;
            }

            if (self is QuadtreeContainerNode<T> selfContainer)
            {
                for (int c = 0; c < selfContainer.Children.Count; c++)
                {
                    QuadtreeNode<T> child = selfContainer.Children[c];
                    if (!PopulateSearch(child, isBoundsIntersects, search))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        static void UpdateBounds(QuadtreeNode<T> root, Rect source)
        {
            root.Rect = source;
            Populate(root);
            return;

            static void Populate(QuadtreeNode<T> parent)
            {
                if (parent is not QuadtreeContainerNode<T> container)
                {
                    return;
                }

                IReadOnlyList<QuadtreeNode<T>> children = container.Children;
                UpdateBounds(children, parent.Rect);

                for (int i = 0; i < children.Count; i++)
                {
                    QuadtreeNode<T> child = children[i];
                    Populate(child);
                }
            }
        }

        static void UpdateBounds(IReadOnlyList<QuadtreeNode<T>> quadtrees, Rect source)
        {
            Vector2 halfSize = source.size * 0.5f;

            Vector2 epsilon = CollisionEx.BoundsEpsilon2;

            Vector2 min = source.min;
            Rect bounds = new(min, halfSize);
            bounds.min -= epsilon;
            bounds.max += epsilon;
            quadtrees[0].Rect = bounds;

            bounds = new(new Vector2(min.x + halfSize.x, min.y), halfSize);
            bounds.min -= epsilon;
            bounds.max += epsilon;
            quadtrees[1].Rect = bounds;

            bounds = new(new Vector2(min.x, min.y + halfSize.y), halfSize);
            bounds.min -= epsilon;
            bounds.max += epsilon;
            quadtrees[2].Rect = bounds;

            bounds = new(min + halfSize, halfSize);
            bounds.min -= epsilon;
            bounds.max += epsilon;
            quadtrees[3].Rect = bounds;
        }

        #region Object Pools
        class QuadtreePool : ObjectPool<Quadtree<T>>
        {
            public QuadtreePool() : base(isThreadSafe: true)
            {
            }

            protected override void Activate(Quadtree<T> obj)
            {
                base.Activate(obj);
                obj.Reset();
            }

            protected override void Deactivate(Quadtree<T> obj)
            {
                base.Deactivate(obj);
                obj.Reset();
            }
        }
        class QuadtreeContainerPool : ObjectPool<QuadtreeContainerNode<T>>
        {
            public QuadtreeContainerPool() : base(isThreadSafe: true)
            {
            }

            protected override void Activate(QuadtreeContainerNode<T> obj)
            {
                base.Activate(obj);
                obj.Reset();
            }

            protected override void Deactivate(QuadtreeContainerNode<T> obj)
            {
                base.Deactivate(obj);
                obj.Reset();
            }
        }
        class QuadtreeLeafPool : ObjectPool<QuadtreeLeafNode<T>>
        {
            public QuadtreeLeafPool() : base(isThreadSafe: true)
            {
            }

            protected override void Activate(QuadtreeLeafNode<T> obj)
            {
                base.Activate(obj);
                obj.Reset();
            }

            protected override void Deactivate(QuadtreeLeafNode<T> obj)
            {
                base.Deactivate(obj);
                obj.Reset();
            }
        }
        #endregion

        /// <returns>Return <c>false</c> to abort.</returns>
        public delegate bool QuadtreeSearchDelegate(QuadtreeLeafNode<T> leaf0, QuadtreeLeafNode<T> leaf1);
        /// <returns>Return <c>true</c> if the <paramref name="item"/> is intersects with the <paramref name="quadtreeBounds"/>.</returns>
        /// <remarks>
        /// Shallow-test is enough for the result such as <see cref="Rect.Overlaps(Rect)"/>.
        /// </remarks>
        public delegate bool IsItemBoundsIntersectsDelegate(Rect quadtreeBounds, T item);
        /// <returns>Return <c>false</c> to abort.</returns>
        public delegate bool ItemSearchDelegate(T item, QuadtreeLeafNode<T> leaf);
        /// <returns>Return <c>true</c> if something is intersects with the <paramref name="quadtreeBounds"/>.</returns>
        public delegate bool IsBoundsIntersectsDelegate(Rect quadtreeBounds);
        /// <returns>Return <c>false</c> to abort.</returns>
        public delegate bool SearchDelegate(QuadtreeLeafNode<T> leaf);

        static readonly QuadtreePool _quadtreePool = new();
        static readonly QuadtreeContainerPool _quadtreeContainerPool = new();
        static readonly QuadtreeLeafPool _quadtreeLeafPool = new();
    }
}
