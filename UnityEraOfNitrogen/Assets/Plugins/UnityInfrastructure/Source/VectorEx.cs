// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Runtime.CompilerServices;
using UnityEngine;

namespace Jih.Unity.Infrastructure
{
    public static class Vector2Ex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 CreateUniform(float value)
        {
            return new Vector2(value, value);
        }
    }

    public static class Vector3Ex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 CreateUniform(float value)
        {
            return new Vector3(value, value, value);
        }
    }

    public static class Vector4Ex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 CreateUniform(float value)
        {
            return new Vector4(value, value, value, value);
        }
    }
}
