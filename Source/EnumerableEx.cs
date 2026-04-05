// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Jih.Unity.Infrastructure
{
    public static class EnumerableEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex<T>(this List<T> collection, int index)
        {
            return 0 <= index && index < collection.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValueAt<T>(this List<T> collection, int index, out T? value)
        {
            if (collection.IsValidIndex(index))
            {
                value = collection[index];
                return true;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddMultiple<T>(this List<T> collection, T value, int count, int additionalCapacity = 0)
        {
            collection.SecureCapacity(collection.Count + count + additionalCapacity);

            for (int i = 0; i < count; i++)
            {
                collection.Add(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddAll<T>(this List<T> collection, IReadOnlyCollection<T> other, int additionalCapacity = 0)
        {
            collection.SecureCapacity(collection.Count + other.Count + additionalCapacity);

            collection.AddRange(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SecureCapacity<T>(this List<T> collection, int desiredCapacity)
        {
            if (desiredCapacity < collection.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(desiredCapacity));
            }

            if (collection.Capacity < desiredCapacity)
            {
                collection.Capacity = desiredCapacity;
            }
            return collection.Capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidIndex<T>(this IReadOnlyList<T> collection, int index)
        {
            return 0 <= index && index < collection.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValueAt<T>(this IReadOnlyList<T> collection, int index, out T? value)
        {
            if (collection.IsValidIndex(index))
            {
                value = collection[index];
                return true;
            }

            value = default;
            return false;
        }
    }
}
