// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using UnityEngine;

namespace Jih.Unity.Infrastructure.Collisions.Common3D
{
    /// <summary>
    /// Prefer CW order but CCW order also allowed.<br/>
    /// However, must preserve the same order across a world.
    /// </summary>
    public struct CollisionTriangle : IEquatable<CollisionTriangle>
    {
        public readonly Bounds Bounds;
        public readonly Vector3 V0, V1, V2;

        public Vector3 WorldV0 { get; internal set; }
        public Vector3 WorldV1 { get; internal set; }
        public Vector3 WorldV2 { get; internal set; }
        public Bounds WorldBounds { get; internal set; }

        public CollisionTriangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;

            WorldV0 = Vector3.zero;
            WorldV1 = Vector3.zero;
            WorldV2 = Vector3.zero;
            WorldBounds = BoundsEx.Empty;

            Vector3 min = Vector3.Min(Vector3.Min(v0, v1), v2);
            Vector3 max = Vector3.Max(Vector3.Max(v0, v1), v2);
            Bounds = BoundsEx.CreateMinMax(min, max);
            Bounds = Bounds.GetInflated(CollisionEx.BoundsEpsilon3);
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is CollisionTriangle triangle && Equals(triangle);
        }
        public readonly bool Equals(CollisionTriangle other)
        {
            return V0.Equals(other.V0) &&
                   V1.Equals(other.V1) &&
                   V2.Equals(other.V2);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(V0, V1, V2);
        }

        public static bool operator ==(CollisionTriangle left, CollisionTriangle right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(CollisionTriangle left, CollisionTriangle right)
        {
            return !(left == right);
        }
    }
}
