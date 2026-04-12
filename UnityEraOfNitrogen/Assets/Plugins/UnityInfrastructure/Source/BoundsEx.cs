// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Runtime.CompilerServices;
using UnityEngine;

namespace Jih.Unity.Infrastructure
{
    public static class BoundsEx
    {
        public static Bounds Empty => new(Vector3.zero, Vector3.zero);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(this Bounds bounds)
        {
            return bounds.center == Vector3.zero && bounds.extents == Vector3.zero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bounds CreateMinMax(Vector3 min, Vector3 max)
        {
            Bounds result = default;
            result.SetMinMax(min, max);
            return result;
        }

        public static Bounds GetApproxTransformed(this Bounds source, Matrix4x4 m)
        {
            Vector3 srcMin = source.min, srcMax = source.max;

            Vector3 ult = new(srcMin.x, srcMin.y, srcMin.z);
            Vector3 urt = new(srcMax.x, srcMin.y, srcMin.z);
            Vector3 ulb = new(srcMin.x, srcMin.y, srcMax.z);
            Vector3 urb = new(srcMax.x, srcMin.y, srcMax.z);

            Vector3 llt = new(srcMin.x, srcMax.y, srcMin.z);
            Vector3 lrt = new(srcMax.x, srcMax.y, srcMin.z);
            Vector3 llb = new(srcMin.x, srcMax.y, srcMax.z);
            Vector3 lrb = new(srcMax.x, srcMax.y, srcMax.z);

            ult = m.MultiplyPoint(ult);
            urt = m.MultiplyPoint(urt);
            ulb = m.MultiplyPoint(ulb);
            urb = m.MultiplyPoint(urb);

            llt = m.MultiplyPoint(llt);
            lrt = m.MultiplyPoint(lrt);
            llb = m.MultiplyPoint(llb);
            lrb = m.MultiplyPoint(lrb);

            Vector3 min = Vector3.Min(ult, urt);
            min = Vector3.Min(min, ulb);
            min = Vector3.Min(min, urb);

            min = Vector3.Min(min, llt);
            min = Vector3.Min(min, lrt);
            min = Vector3.Min(min, llb);
            min = Vector3.Min(min, lrb);

            Vector3 max = Vector3.Max(ult, urt);
            max = Vector3.Max(max, ulb);
            max = Vector3.Max(max, urb);

            max = Vector3.Max(max, llt);
            max = Vector3.Max(max, lrt);
            max = Vector3.Max(max, llb);
            max = Vector3.Max(max, lrb);

            return CreateMinMax(min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bounds Union(Bounds l, Bounds r)
        {
            if (l.IsEmpty())
            {
                return r;
            }
            if (r.IsEmpty())
            {
                return l;
            }
            return CreateMinMax(Vector3.Min(l.min, r.min), Vector3.Max(l.max, r.max));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bounds Intersect(Bounds l, Bounds r)
        {
            if (l.IsEmpty() || l.IsEmpty())
            {
                return Empty;
            }
            return CreateMinMax(Vector3.Max(l.min, r.min), Vector3.Min(l.max, r.max));
        }

        /// <remarks>
        /// Add or subtract given size to min and max.<br/>
        /// In contrast, <see cref="Bounds.Expand(Vector3)"/> applying <b>half</b> size.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bounds GetInflated(this Bounds bounds, Vector3 size)
        {
            return CreateMinMax(bounds.min - size, bounds.max + size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Raycast(this Bounds bounds, Vector3 rayOrigin, Vector3 rayDirection)
        {
            return bounds.IntersectRay(new Ray(rayOrigin, rayDirection));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Raycast(this Bounds bounds, Vector3 rayOrigin, Vector3 rayDirection, out float t)
        {
            return bounds.IntersectRay(new Ray(rayOrigin, rayDirection), out t);
        }
    }
}
