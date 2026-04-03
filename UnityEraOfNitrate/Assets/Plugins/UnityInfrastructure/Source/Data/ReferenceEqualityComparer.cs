// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Jih.Unity.Infrastructure.Data
{
    public class ReferenceEqualityComparer : IEqualityComparer<object?>, IEqualityComparer
    {
        public static ReferenceEqualityComparer Instance { get; } = new();

        public new bool Equals(object? x, object? y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object? obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    public class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    {
        public static ReferenceEqualityComparer<T> Instance { get; } = new();

        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
