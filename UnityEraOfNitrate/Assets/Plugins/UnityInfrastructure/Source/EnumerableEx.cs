// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Jih.Unity.Infrastructure
{
    public static class EnumerableEx
    {
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
