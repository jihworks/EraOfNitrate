// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

namespace Jih.Unity.Infrastructure.Runtime
{
    /// <summary>
    /// This is a <b>struct</b> to avoid cache-miss and GC pressure.<br/>
    /// <b>DO NOT</b> use <c>readonly</c> for this field.<br/>
    /// <b>DO NOT</b> use automatically implemented property.
    /// </summary>
    /// <remarks>
    /// This struct does not check the instance already set. Only check the same instance had been set. Because of PIE stability.<br/>
    /// <b>NOT</b> thread-safe.
    /// </remarks>
    public struct SingletonStorage<T> where T : class
    {
        T? _instance;

        public SingletonStorage(T instance)
        {
            _instance = instance;
        }

        public void Set(T instance)
        {
            if (ReferenceEquals(_instance, instance))
            {
                return;
            }
            if (_instance is not null)
            {
                throw new InvalidOperationException($"Instance of '{typeof(T)}' is already set but trying to set again.");
            }
            _instance = instance;
        }

        public readonly T Get()
        {
            if (!TryGet(out T? instance))
            {
                throw new InvalidOperationException($"Instance of '{typeof(T)}' is not set but trying to get.");
            }
            return instance;
        }

        public readonly bool TryGet([NotNullWhen(true)] out T? instance)
        {
            if (_instance is null)
            {
                instance = null;
                return false;
            }
            instance = _instance;
            return true;
        }

        public void Clear()
        {
            _instance = null;
        }
    }
}
