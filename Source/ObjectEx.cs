// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Runtime.CompilerServices;
using UnityEngine;

namespace Jih.Unity.Infrastructure
{
    public static class ObjectEx
    {
        /// <summary>
        /// Checks if the given object is null.
        /// </summary>
        /// <remarks>
        /// This method is also checks deep-null for Unity objects wheather the Unity object had been destroyed or not by the engine.
        /// </remarks>
        /// <param name="objectName">Debugging purpose <paramref name="obj"/>'s name.</param>
        /// <exception cref="System.NullReferenceException">Throws if the given object is system <c>null</c> or Unity null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ThrowIfNull<T>(this T? obj, string objectName) where T : class
        {
            if (obj is null)
            {
                throw new System.NullReferenceException($"Object '{objectName}' is null.");
            }
            if (obj is Object uobj && uobj == null)
            {
                throw new System.NullReferenceException($"Unity object '{objectName}' is null.");
            }
            return obj;
        }
        /// <summary>
        /// Checks if the given object is null and outputs the non-null value for convenience in nullable analyzer context.
        /// </summary>
        /// <remarks>
        /// This method is also checks deep-null for Unity objects wheather the Unity object had been destroyed or not by the engine.
        /// </remarks>
        /// <param name="objectName">Debugging purpose <paramref name="obj"/>'s name.</param>
        /// <exception cref="System.NullReferenceException">Throws if the given object is system <c>null</c> or Unity null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ThrowIfNull<T>(this T? obj, out T value, string objectName) where T : class
        {
            if (obj is null)
            {
                throw new System.NullReferenceException($"Object '{objectName}' is null.");
            }
            if (obj is Object uobj && uobj == null)
            {
                throw new System.NullReferenceException($"Unity object '{objectName}' is null.");
            }
            value = obj;
        }
    }
}
