// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.Unity.Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Jih.Unity.Infrastructure.Runtime
{
    /// <remarks>
    /// Duplication check is based on a HashSet with <see cref="ReferenceEqualityComparer{T}"/>.<br/>
    /// Therefore, there is no effect even though <typeparamref name="T"/> overrides <see cref="object.GetHashCode"/> or <see cref="object.Equals(object?)"/>.
    /// </remarks>
    public abstract class BaseObjectPool<T> where T : class
    {
        protected const int DefaultInitialCollectionCapacity = 8;
        protected const int DefaultInitialPoolCapacity = 0;
        protected const bool DefaultIsThreadSafe = false;

        private readonly Stack<T> pool;
        private readonly HashSet<T> poolMap;

        private readonly object? lockObject;
        public bool IsThreadSafe => lockObject is not null;

        /// <summary>
        /// Objects count in the pool.
        /// </summary>
        public int Count => pool.Count;

        /// <param name="initialCollectionCapacity">Initial internal management collections capacity.</param>
        /// <param name="initialPoolCapacity">Initial pool object count.</param>
        /// <param name="isThreadSafe">Whether thread-safe or not. If <c>true</c>, will use <c>lock</c> to achieve thread-safe. Pass <c>false</c> to gain performance if thread-safe is not essential.</param>
        protected BaseObjectPool(int initialCollectionCapacity, int initialPoolCapacity, bool isThreadSafe)
        {
            if (isThreadSafe)
            {
                lockObject = new object();
            }

            pool = new Stack<T>(initialCollectionCapacity);
            poolMap = new HashSet<T>(initialCollectionCapacity, ReferenceEqualityComparer<T>.Instance);

            // Create by initial capacity
            for (int i = 0; i < initialPoolCapacity; i++)
            {
                T item = Create();
                pool.Push(item);
                poolMap.Add(item);
            }
        }

        /// <summary>
        /// Takes an object from the pool. If the pool is empty, a new object will be created.
        /// </summary>
        public T Get()
        {
            T item;

            if (lockObject is not null)
            {
                lock (lockObject)
                {
                    item = GetInternal();
                }
            }
            else
            {
                item = GetInternal();
            }

            return item;
        }

        private T GetInternal()
        {
            T item;

            if (pool.Count > 0)
            {
                item = pool.Pop();
                poolMap.Remove(item);
            }
            else
            {
                item = Create();
            }

            // Initialize_Internal the object.
            Activate(item);

            return item;
        }

        /// <summary>
        /// Returns an object to the pool. If the object is already in the pool, it will be ignored.
        /// </summary>
        public void Release(T item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (lockObject is not null)
            {
                lock (lockObject)
                {
                    ReleaseInternal(item);
                }
            }
            else
            {
                ReleaseInternal(item);
            }
        }

        private void ReleaseInternal(T item)
        {
            // Atomic check & add to prevent duplicates in poolMap
            if (!poolMap.Add(item))
            {
                // Already released item.
                return;
            }

            try
            {
                // Call Deactivate before adding to pool.
                Deactivate(item);
                // Add to pool if Deactivate succeeded.
                pool.Push(item);
            }
            catch
            {
                // If Deactivate throws an exception, remove the item from poolMap.
                poolMap.Remove(item);
                throw; // Throw the exception to the caller.
            }
        }

        /// <summary>
        /// Returns multiple objects to the pool. If any object is already in the pool, it will be ignored.
        /// </summary>
        public void ReleaseMany(IEnumerable<T> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (lockObject is not null)
            {
                lock (lockObject)
                {
                    foreach (var item in items)
                    {
                        if (item is not null)
                        {
                            ReleaseInternal(item);
                        }
                    }
                }
            }
            else
            {
                foreach (var item in items)
                {
                    if (item is not null)
                    {
                        ReleaseInternal(item);
                    }
                }
            }
        }

        /// <summary>
        /// Removes all objects from the pool. The objects will be deactivated.
        /// </summary>
        public void Clear()
        {
            if (lockObject is not null)
            {
                lock (lockObject)
                {
                    ClearInternal();
                }
            }
            else
            {
                ClearInternal();
            }
        }

        private void ClearInternal()
        {
            while (pool.Count > 0)
            {
                T item = pool.Pop();
                poolMap.Remove(item);

                Deactivate(item);
            }
        }

        /// <summary>
        /// Resizes the pool. If the pool size is larger than count, the excess layers will be removed.
        /// </summary>
        /// <param name="count">Target pool size in count.</param>
        public void Trim(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (lockObject is not null)
            {
                lock (lockObject)
                {
                    TrimInternal(count);
                }
            }
            else
            {
                TrimInternal(count);
            }
        }

        private void TrimInternal(int count)
        {
            while (pool.Count > count)
            {
                T item = pool.Pop();
                poolMap.Remove(item);

                Deactivate(item);
            }
        }

        /// <summary>
        /// Adds objects to the pool until the pool size reaches the specified count.
        /// </summary>
        /// <param name="count">Target size in count.</param>
        public void EnsureCapacity(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (lockObject is not null)
            {
                lock (lockObject)
                {
                    EnsureCapacityInternal(count);
                }
            }
            else
            {
                EnsureCapacityInternal(count);
            }
        }

        private void EnsureCapacityInternal(int count)
        {
            for (int i = pool.Count; i < count; i++)
            {
                T item = Create();
                pool.Push(item);
                poolMap.Add(item);
            }
        }

        /// <summary>
        /// Called when an object is created by the pool. The object will be added to the pool after this method returns.
        /// </summary>
        protected abstract T Create();
        /// <summary>
        /// Called when an object is taken from the pool. The object will be removed from the pool before this method returns.
        /// </summary>
        protected abstract void Activate(T obj);
        /// <summary>
        /// Called when an object is returned to the pool. The object will be added to the pool after this method returns.
        /// </summary>
        protected abstract void Deactivate(T obj);
    }
}
