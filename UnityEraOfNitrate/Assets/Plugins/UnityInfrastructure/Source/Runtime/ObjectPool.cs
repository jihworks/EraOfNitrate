// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.Unity.Infrastructure.Runtime
{
    /// <summary>
    /// Simplest version of <see cref="ObjectPoolBase{T}"/>.<br/>
    /// It creates objects by parameterless constructor and does not handle activation and deactivation.
    /// </summary>
    public class ObjectPool<T> : ObjectPoolBase<T> where T : class, new()
    {
        public ObjectPool(int initialCollectionCapacity = DefaultInitialCollectionCapacity, int initialPoolCapacity = DefaultInitialPoolCapacity, bool isThreadSafe = DefaultIsThreadSafe)
            : base(initialCollectionCapacity, initialPoolCapacity, isThreadSafe)
        {
        }

        protected override T Create()
        {
            return new T();
        }

        protected override void Activate(T obj)
        {
        }

        protected override void Deactivate(T obj)
        {
        }
    }
}
