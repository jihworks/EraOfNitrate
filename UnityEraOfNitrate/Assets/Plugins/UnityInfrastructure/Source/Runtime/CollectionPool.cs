// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;

namespace Jih.Unity.Infrastructure.Runtime
{
    public class ListPool<T> : ObjectPoolBase<List<T>>
    {
        public int ListCapacity { get; set; }

        /// <param name="listCapacity">Initial capacity of the newly created <see cref="List{T}"/>s.</param>
        public ListPool(int listCapacity = 8, int initialCollectionCapacity = DefaultInitialCollectionCapacity, int initialPoolCapacity = DefaultInitialPoolCapacity, bool isThreadSafe = DefaultIsThreadSafe)
            : base(initialCollectionCapacity, initialPoolCapacity, isThreadSafe)
        {
            ListCapacity = listCapacity;
        }

        protected override List<T> Create()
        {
            return new List<T>(ListCapacity);
        }

        protected override void Activate(List<T> obj)
        {
            obj.Clear();
        }

        protected override void Deactivate(List<T> obj)
        {
            obj.Clear();
        }
    }

    public class DictionaryPool<TKey, TValue> : ObjectPoolBase<Dictionary<TKey, TValue>>
    {
        public int DictionaryCapacity { get; set; }

        /// <param name="dictionaryCapacity">Initial capacity of the newly created <see cref="Dictionary{TKey, TValue}"/>s.</param>
        public DictionaryPool(int dictionaryCapacity = 8, int initialCollectionCapacity = DefaultInitialCollectionCapacity, int initialPoolCapacity = DefaultInitialPoolCapacity, bool isThreadSafe = DefaultIsThreadSafe)
            : base(initialCollectionCapacity, initialPoolCapacity, isThreadSafe)
        {
            DictionaryCapacity = dictionaryCapacity;
        }

        protected override Dictionary<TKey, TValue> Create()
        {
            return new Dictionary<TKey, TValue>(DictionaryCapacity);
        }

        protected override void Activate(Dictionary<TKey, TValue> obj)
        {
            obj.Clear();
        }

        protected override void Deactivate(Dictionary<TKey, TValue> obj)
        {
            obj.Clear();
        }
    }

    public class HashSetPool<T> : ObjectPoolBase<HashSet<T>>
    {
        public int HashSetCapacity { get; set; }

        /// <param name="hashSetCapacity">Initial capacity of the newly created <see cref="HashSet{T}"/>s.</param>
        public HashSetPool(int hashSetCapacity = 8, int initialCollectionCapacity = DefaultInitialCollectionCapacity, int initialPoolCapacity = DefaultInitialPoolCapacity, bool isThreadSafe = DefaultIsThreadSafe)
            : base(initialCollectionCapacity, initialPoolCapacity, isThreadSafe)
        {
            HashSetCapacity = hashSetCapacity;
        }

        protected override HashSet<T> Create()
        {
            return new HashSet<T>(HashSetCapacity);
        }

        protected override void Activate(HashSet<T> obj)
        {
            obj.Clear();
        }

        protected override void Deactivate(HashSet<T> obj)
        {
            obj.Clear();
        }
    }
}
