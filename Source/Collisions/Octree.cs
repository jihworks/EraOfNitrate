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
    public abstract class OctreeNode<T>
    {
        public Bounds Bounds { get; internal set; } = new(Vector3.zero, Vector3.zero);

        /// <summary>
        /// Root is <c>0</c>, increases when go down the tree.
        /// </summary>
        public int Depth { get; internal set; }

        internal virtual void Reset()
        {
            Bounds = new Bounds(Vector3.zero, Vector3.zero);
            Depth = 0;
        }
    }

    public class OctreeContainerNode<T> : OctreeNode<T>
    {
        internal List<OctreeNode<T>> ChildrenInternal { get; } = new();
        public IReadOnlyList<OctreeNode<T>> Children => ChildrenInternal;

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

    public sealed class OctreeLeafNode<T> : OctreeNode<T>
    {
        readonly List<T> _items = new();
        public List<T> Items => _items;

        internal override void Reset()
        {
            _items.Clear();

            base.Reset();
        }
    }

    public sealed class Octree<T> : OctreeContainerNode<T>
    {
        public static Octree<T> Create(int targetDepth)
        {
            if (targetDepth <= 0)
            {
                throw new ArgumentException("Octree depth cannot be 0 or negative.", nameof(targetDepth));
            }

            Octree<T> root = _octreePool.Get();
            root.Depth = 0;
            Populate(root, root, 1, targetDepth);
            return root;


            static void Populate(Octree<T> root, OctreeContainerNode<T> parent, int currentDepth, int targetDepth)
            {
                if (currentDepth == targetDepth)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        OctreeLeafNode<T> child = _octreeLeafPool.Get();
                        child.Depth = currentDepth;
                        parent.ChildrenInternal.Add(child);

                        root.LeavesInternal.Add(child);
                    }
                    return;
                }

                for (int i = 0; i < 8; i++)
                {
                    OctreeContainerNode<T> child = _octreeContainerPool.Get();
                    child.Depth = currentDepth;
                    parent.ChildrenInternal.Add(child);

                    Populate(root, child, currentDepth + 1, targetDepth);
                }
            }
        }

        public static void Release(ref Octree<T>? root)
        {
            if (root is null)
            {
                return;
            }
            _octreePool.Release(root);
            root = null;
        }

        internal List<OctreeLeafNode<T>> LeavesInternal { get; } = new();
        public IReadOnlyList<OctreeLeafNode<T>> Leaves => LeavesInternal;

        internal override void Reset()
        {
            LeavesInternal.Clear();

            base.Reset();
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
        public void Update(Octree<T> root, IReadOnlyList<T> itemSources, OrderablePartitioner<Tuple<int, int>>? sourcePartitioner, Bounds sourcesTotalBounds, IsItemBoundsIntersectsDelegate isItemBoundsIntersects)
        {
            IReadOnlyList<OctreeLeafNode<T>> leaves = root.Leaves;

            UpdateBounds(root, sourcesTotalBounds);

            for (int i = 0; i < leaves.Count; i++)
            {
                OctreeLeafNode<T> leaf = leaves[i];

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

                    if (!TryAddItemInternal(this, item, isItemBoundsIntersects, false))
                    {
                        throw new InvalidOperationException("An item was not added to any octree.");
                    }
                }
            }
        }

        /// <summary>
        /// Try to add an item to the octree without updating bounds.
        /// </summary>
        /// <remarks>
        /// This is useful for the case that the octree bounds already cover whole expecting space and spawning some items to search.<br/>
        /// This method does <c>NOT</c> check duplication.
        /// </remarks>
        /// <returns>Whether the <paramref name="item"/> was successfully added to the octree.</returns>
        public bool TryAddItem(T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects)
        {
            return TryAddItemInternal(this, item, isItemBoundsIntersects, false);
        }

        static bool TryAddItemInternal(Octree<T> root, T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects, bool useLock)
        {
            return Populate(root);


            bool Populate(OctreeNode<T> self)
            {
                if (!isItemBoundsIntersects(self.Bounds, item))
                {
                    return false;
                }

                if (self is OctreeLeafNode<T> selfLeaf)
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

                if (self is OctreeContainerNode<T> selfContainer)
                {
                    IReadOnlyList<OctreeNode<T>> children = selfContainer.Children;

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
        /// Remove an item from the octree without updating bounds.
        /// </summary>
        /// <remarks>
        /// This method will search the item in whole octree and remove it all.
        /// </remarks>
        /// <returns>Whether the <paramref name="item"/> was found and removed.</returns>
        public bool RemoveItem(T item)
        {
            IReadOnlyList<OctreeLeafNode<T>> leaves = Leaves;

            bool anyRemoved = false;
            for (int l = 0; l < leaves.Count; l++)
            {
                OctreeLeafNode<T> leaf = leaves[l];

                anyRemoved |= leaf.Items.Remove(item);
            }
            return anyRemoved;
        }

        /// <summary>
        /// Remove items from the octree without updating bounds.
        /// </summary>
        /// <returns>Total sum of removed items count.</returns>
        public int RemoveAllItems(Predicate<T> match)
        {
            IReadOnlyList<OctreeLeafNode<T>> leaves = Leaves;

            int count = 0;
            for (int l = 0; l < leaves.Count; l++)
            {
                OctreeLeafNode<T> leaf = leaves[l];

                List<T> leafItems = leaf.Items;

                count += leafItems.RemoveAll(match);
            }
            return count;
        }

        /// <returns>Whether the <paramref name="item"/> was found in any leaf octree.</returns>
        public bool ContainsItem(T item)
        {
            IReadOnlyList<OctreeLeafNode<T>> leaves = Leaves;

            for (int l = 0; l < leaves.Count; l++)
            {
                OctreeLeafNode<T> leaf = leaves[l];

                List<T> leafItems = leaf.Items;

                if (leafItems.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Search octree versus octree.</summary>
        /// <param name="allowSwap">
        /// Whether allowing swap <c>this</c> and <paramref name="other"/> to increase performance.<br/>
        /// If <c>true</c>, input arguments order for <paramref name="search"/> can be different with <c>this</c> after <paramref name="other"/> order.
        /// </param>
        public void Search(Octree<T> other, bool allowSwap, OctreeSearchDelegate search)
        {
            Octree<T> searchRoot = this, searchOther = other;
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
                OctreeLeafNode<T> otherLeaf = searchOther.Leaves[i];
                if (!PopulateSearch(searchRoot, otherLeaf, search))
                {
                    return;
                }
            }
        }
        static bool PopulateSearch(OctreeNode<T> self, OctreeLeafNode<T> otherLeaf, OctreeSearchDelegate search)
        {
            if (!self.Bounds.Intersects(otherLeaf.Bounds))
            {
                return true;
            }

            if (self is OctreeLeafNode<T> selfLeaf && !search(selfLeaf, otherLeaf))
            {
                return false;
            }

            if (self is OctreeContainerNode<T> selfContainer)
            {
                for (int c = 0; c < selfContainer.Children.Count; c++)
                {
                    OctreeNode<T> child = selfContainer.Children[c];
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
        public void Search(T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects, ItemSearchDelegate search)
        {
            PopulateSearch(this, item, isItemBoundsIntersects, search);
        }
        static bool PopulateSearch(OctreeNode<T> self, T item, IsItemBoundsIntersectsDelegate isItemBoundsIntersects, ItemSearchDelegate search)
        {
            if (!isItemBoundsIntersects(self.Bounds, item))
            {
                return true;
            }

            if (self is OctreeLeafNode<T> selfLeaf && !search(item, selfLeaf))
            {
                return false;
            }

            if (self is OctreeContainerNode<T> selfContainer)
            {
                for (int c = 0; c < selfContainer.Children.Count; c++)
                {
                    OctreeNode<T> child = selfContainer.Children[c];
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
        public void Search(IsBoundsIntersectsDelegate isBoundsIntersects, SearchDelegate search)
        {
            PopulateSearch(this, isBoundsIntersects, search);
        }
        static bool PopulateSearch(OctreeNode<T> self, IsBoundsIntersectsDelegate isBoundsIntersects, SearchDelegate search)
        {
            if (!isBoundsIntersects(self.Bounds))
            {
                return true;
            }

            if (self is OctreeLeafNode<T> selfLeaf && !search(selfLeaf))
            {
                return false;
            }

            if (self is OctreeContainerNode<T> selfContainer)
            {
                for (int c = 0; c < selfContainer.Children.Count; c++)
                {
                    OctreeNode<T> child = selfContainer.Children[c];
                    if (!PopulateSearch(child, isBoundsIntersects, search))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        static void UpdateBounds(OctreeNode<T> root, Bounds source)
        {
            root.Bounds = source;
            Populate(root);
            return;

            static void Populate(OctreeNode<T> parent)
            {
                if (parent is not OctreeContainerNode<T> container)
                {
                    return;
                }

                IReadOnlyList<OctreeNode<T>> children = container.Children;
                UpdateBounds(children, parent.Bounds);

                for (int i = 0; i < children.Count; i++)
                {
                    OctreeNode<T> child = children[i];
                    Populate(child);
                }
            }
        }

        static void UpdateBounds(IReadOnlyList<OctreeNode<T>> octrees, Bounds source)
        {
            Vector3 halfSize = source.size * 0.5f;

            Vector3 epsilon = CollisionEx.BoundsEpsilon3;

            Vector3 min = source.min;
            Bounds bounds = default;
            bounds.SetMinMax(min - epsilon, min + halfSize + epsilon);
            octrees[0].Bounds = bounds;

            min = source.min;
            min.x += halfSize.x;
            bounds = default;
            bounds.SetMinMax(min - epsilon, min + halfSize + epsilon);
            octrees[1].Bounds = bounds;

            min = source.min;
            min.z += halfSize.z;
            bounds = default;
            bounds.SetMinMax(min - epsilon, min + halfSize + epsilon);
            octrees[2].Bounds = bounds;

            min = source.min;
            min.x += halfSize.x;
            min.z += halfSize.z;
            bounds = default;
            bounds.SetMinMax(min - epsilon, min + halfSize + epsilon);
            octrees[3].Bounds = bounds;


            min = source.min;
            min.y += halfSize.y;
            bounds = default;
            bounds.SetMinMax(min - epsilon, min + halfSize + epsilon);
            octrees[4].Bounds = bounds;

            min = source.min;
            min.y += halfSize.y;
            min.x += halfSize.x;
            bounds = default;
            bounds.SetMinMax(min - epsilon, min + halfSize + epsilon);
            octrees[5].Bounds = bounds;

            min = source.min;
            min.y += halfSize.y;
            min.z += halfSize.z;
            bounds = default;
            bounds.SetMinMax(min - epsilon, min + halfSize + epsilon);
            octrees[6].Bounds = bounds;

            min = source.min;
            min.y += halfSize.y;
            min.x += halfSize.x;
            min.z += halfSize.z;
            bounds = default;
            bounds.SetMinMax(min - epsilon, min + halfSize + epsilon);
            octrees[7].Bounds = bounds;
        }

        #region Object Pools
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
        class OctreeContainerPool : ObjectPool<OctreeContainerNode<T>>
        {
            public OctreeContainerPool() : base(isThreadSafe: true)
            {
            }

            protected override void Activate(OctreeContainerNode<T> obj)
            {
                base.Activate(obj);
                obj.Reset();
            }

            protected override void Deactivate(OctreeContainerNode<T> obj)
            {
                base.Deactivate(obj);
                obj.Reset();
            }
        }
        class OctreeLeafPool : ObjectPool<OctreeLeafNode<T>>
        {
            public OctreeLeafPool() : base(isThreadSafe: true)
            {
            }

            protected override void Activate(OctreeLeafNode<T> obj)
            {
                base.Activate(obj);
                obj.Reset();
            }

            protected override void Deactivate(OctreeLeafNode<T> obj)
            {
                base.Deactivate(obj);
                obj.Reset();
            }
        }
        #endregion

        /// <returns>Return <c>false</c> to abort.</returns>
        public delegate bool OctreeSearchDelegate(OctreeLeafNode<T> leaf0, OctreeLeafNode<T> leaf1);
        /// <returns>Return <c>true</c> if the <paramref name="item"/> is intersects with the <paramref name="octreeBounds"/>.</returns>
        /// <remarks>
        /// Shallow-test is enough for the result such as <see cref="Bounds.Intersects(Bounds)"/>.
        /// </remarks>
        public delegate bool IsItemBoundsIntersectsDelegate(Bounds octreeBounds, T item);
        /// <returns>Return <c>false</c> to abort.</returns>
        public delegate bool ItemSearchDelegate(T item, OctreeLeafNode<T> leaf);
        /// <returns>Return <c>true</c> if something is intersects with the <paramref name="octreeBounds"/>.</returns>
        public delegate bool IsBoundsIntersectsDelegate(Bounds octreeBounds);
        /// <returns>Return <c>false</c> to abort.</returns>
        public delegate bool SearchDelegate(OctreeLeafNode<T> leaf);

        static readonly OctreePool _octreePool = new();
        static readonly OctreeContainerPool _octreeContainerPool = new();
        static readonly OctreeLeafPool _octreeLeafPool = new();
    }
}
