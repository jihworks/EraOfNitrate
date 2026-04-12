// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using UnityEngine;

namespace Jih.Unity.Infrastructure.Collisions.Common3D
{
    public class BoxShape : CollisionShape
    {
        public BoxShape()
        {
            LocalBounds = BoundsEx.Empty;
            WorldTransform = Matrix4x4.identity;
        }

        public BoxShape(Bounds bounds)
        {
            LocalBounds = bounds;
            WorldTransform = Matrix4x4.identity;
        }

        public void SetLocalBounds(Bounds bounds)
        {
            LocalBounds = bounds;
        }

        protected override void UpdateBounds_Impl(ref bool isLocalBoundsDirty, ref bool isWorldTransformDirty)
        {
            WorldBounds = LocalBounds.GetApproxTransformed(WorldTransform);

            isLocalBoundsDirty = false;
            isWorldTransformDirty = false;
        }

        protected override bool IntersectsWith_Impl(BoxShape other)
        {
            if (!WorldBounds.Intersects(other.WorldBounds))
            {
                return false;
            }

            return SeparatingAxisTest(other);
        }

        protected override bool IntersectsWith_Impl(MeshShape other)
        {
            return other.IntersectsWith(this);
        }

        protected override bool Raycast_Impl(Vector3 rayOrigin, Vector3 rayDirection, out float t)
        {
            // Transform the ray from world space to local space
            Matrix4x4 worldToLocal = WorldTransform.inverse;
            Vector3 localRayOrigin = worldToLocal.MultiplyPoint(rayOrigin);
            Vector3 localRayDirection = worldToLocal.MultiplyVector(rayDirection);

            return LocalBounds.Raycast(localRayOrigin, localRayDirection, out t);
        }

        #region Does Triangle Intersect Box
        internal bool DoesTriangleIntersectBox(CollisionTriangle triangle)
        {
            if (!WorldBounds.Intersects(triangle.WorldBounds))
            {
                return false;
            }

            Vector3 worldV0 = triangle.WorldV0;
            Vector3 worldV1 = triangle.WorldV1;
            Vector3 worldV2 = triangle.WorldV2;

            Vector3 boxAxisX = ((Vector3)WorldTransform.GetColumn(0)).normalized;
            Vector3 boxAxisY = ((Vector3)WorldTransform.GetColumn(1)).normalized;
            Vector3 boxAxisZ = ((Vector3)WorldTransform.GetColumn(2)).normalized;

            Vector3 boxCenter = WorldTransform.MultiplyPoint(LocalBounds.center);

            Vector3 boxHalfSize = LocalBounds.extents;

            Vector3 edge0 = worldV1 - worldV0;
            Vector3 edge1 = worldV2 - worldV1;
            Vector3 edge2 = worldV0 - worldV2;

            Vector3 triangleNormal = Vector3.Cross(edge0, edge1).normalized;

            if (!TestAxis(boxAxisX, worldV0, worldV1, worldV2, boxCenter, boxHalfSize.x))
            {
                return false;
            }
            if (!TestAxis(boxAxisY, worldV0, worldV1, worldV2, boxCenter, boxHalfSize.y))
            {
                return false;
            }
            if (!TestAxis(boxAxisZ, worldV0, worldV1, worldV2, boxCenter, boxHalfSize.z))
            {
                return false;
            }

            if (!TestAxis(triangleNormal, worldV0, worldV1, worldV2, boxCenter, boxHalfSize, boxAxisX, boxAxisY, boxAxisZ))
            {
                return false;
            }

            Vector3 axis;

            axis = Vector3.Cross(boxAxisX, edge0);
            if (axis.sqrMagnitude > 1e-6f)
            {
                axis = axis.normalized;
                if (!TestAxis(axis, worldV0, worldV1, worldV2, boxCenter, boxHalfSize, boxAxisX, boxAxisY, boxAxisZ))
                {
                    return false;
                }
            }

            axis = Vector3.Cross(boxAxisX, edge1);
            if (axis.sqrMagnitude > 1e-6f)
            {
                axis = axis.normalized;
                if (!TestAxis(axis, worldV0, worldV1, worldV2, boxCenter, boxHalfSize, boxAxisX, boxAxisY, boxAxisZ))
                {
                    return false;
                }
            }

            axis = Vector3.Cross(boxAxisX, edge2);
            if (axis.sqrMagnitude > 1e-6f)
            {
                axis = axis.normalized;
                if (!TestAxis(axis, worldV0, worldV1, worldV2, boxCenter, boxHalfSize, boxAxisX, boxAxisY, boxAxisZ))
                {
                    return false;
                }
            }

            axis = Vector3.Cross(boxAxisY, edge0);
            if (axis.sqrMagnitude > 1e-6f)
            {
                axis = axis.normalized;
                if (!TestAxis(axis, worldV0, worldV1, worldV2, boxCenter, boxHalfSize, boxAxisX, boxAxisY, boxAxisZ))
                {
                    return false;
                }
            }

            axis = Vector3.Cross(boxAxisY, edge1);
            if (axis.sqrMagnitude > 1e-6f)
            {
                axis = axis.normalized;
                if (!TestAxis(axis, worldV0, worldV1, worldV2, boxCenter, boxHalfSize, boxAxisX, boxAxisY, boxAxisZ))
                {
                    return false;
                }
            }

            axis = Vector3.Cross(boxAxisY, edge2);
            if (axis.sqrMagnitude > 1e-6f)
            {
                axis = axis.normalized;
                if (!TestAxis(axis, worldV0, worldV1, worldV2, boxCenter, boxHalfSize, boxAxisX, boxAxisY, boxAxisZ))
                {
                    return false;
                }
            }

            axis = Vector3.Cross(boxAxisZ, edge0);
            if (axis.sqrMagnitude > 1e-6f)
            {
                axis = axis.normalized;
                if (!TestAxis(axis, worldV0, worldV1, worldV2, boxCenter, boxHalfSize, boxAxisX, boxAxisY, boxAxisZ))
                {
                    return false;
                }
            }

            axis = Vector3.Cross(boxAxisZ, edge1);
            if (axis.sqrMagnitude > 1e-6f)
            {
                axis = axis.normalized;
                if (!TestAxis(axis, worldV0, worldV1, worldV2, boxCenter, boxHalfSize, boxAxisX, boxAxisY, boxAxisZ))
                {
                    return false;
                }
            }

            axis = Vector3.Cross(boxAxisZ, edge2);
            if (axis.sqrMagnitude > 1e-6f)
            {
                axis = axis.normalized;
                if (!TestAxis(axis, worldV0, worldV1, worldV2, boxCenter, boxHalfSize, boxAxisX, boxAxisY, boxAxisZ))
                {
                    return false;
                }
            }

            return true;
        }

        static bool TestAxis(in Vector3 axis, in Vector3 v0, in Vector3 v1, in Vector3 v2, in Vector3 boxCenter,
            in Vector3 boxHalfSize, in Vector3 boxAxisX, in Vector3 boxAxisY, in Vector3 boxAxisZ)
        {
            if (axis.sqrMagnitude < 1e-6f)
            {
                return true;
            }

            float p0 = Vector3.Dot(v0 - boxCenter, axis);
            float p1 = Vector3.Dot(v1 - boxCenter, axis);
            float p2 = Vector3.Dot(v2 - boxCenter, axis);

            float triangleMin = Mathf.Min(Mathf.Min(p0, p1), p2);
            float triangleMax = Mathf.Max(Mathf.Max(p0, p1), p2);

            float boxRadius =
                boxHalfSize.x * Mathf.Abs(Vector3.Dot(boxAxisX, axis)) +
                boxHalfSize.y * Mathf.Abs(Vector3.Dot(boxAxisY, axis)) +
                boxHalfSize.z * Mathf.Abs(Vector3.Dot(boxAxisZ, axis));

            return !(triangleMax < -boxRadius || triangleMin > boxRadius);
        }

        static bool TestAxis(in Vector3 axis, in Vector3 v0, in Vector3 v1, in Vector3 v2, in Vector3 boxCenter, float boxHalfSize)
        {
            if (axis.sqrMagnitude < 1e-6f)
            {
                return true;
            }

            float p0 = Vector3.Dot(v0 - boxCenter, axis);
            float p1 = Vector3.Dot(v1 - boxCenter, axis);
            float p2 = Vector3.Dot(v2 - boxCenter, axis);

            float triangleMin = Mathf.Min(Mathf.Min(p0, p1), p2);
            float triangleMax = Mathf.Max(Mathf.Max(p0, p1), p2);

            float boxRadius = boxHalfSize;

            return !(triangleMax < -boxRadius || triangleMin > boxRadius);
        }
        #endregion

        #region Separating Axis Test
        bool SeparatingAxisTest(BoxShape other)
        {
            Vector3 axisX1 = ((Vector3)WorldTransform.GetColumn(0)).normalized;
            Vector3 axisY1 = ((Vector3)WorldTransform.GetColumn(1)).normalized;
            Vector3 axisZ1 = ((Vector3)WorldTransform.GetColumn(2)).normalized;

            Vector3 axisX2 = ((Vector3)other.WorldTransform.GetColumn(0)).normalized;
            Vector3 axisY2 = ((Vector3)other.WorldTransform.GetColumn(1)).normalized;
            Vector3 axisZ2 = ((Vector3)other.WorldTransform.GetColumn(2)).normalized;

            Vector3 center1 = WorldTransform.MultiplyPoint(LocalBounds.center);
            Vector3 halfSize1 = LocalBounds.extents;

            Vector3 center2 = other.WorldTransform.MultiplyPoint(other.LocalBounds.center);
            Vector3 halfSize2 = other.LocalBounds.extents;

            Vector3 T = center2 - center1;

            if (!TestBoxAxis(axisX1, T, halfSize1.x, halfSize2, axisX2, axisY2, axisZ2))
            {
                return false;
            }
            if (!TestBoxAxis(axisY1, T, halfSize1.y, halfSize2, axisX2, axisY2, axisZ2))
            {
                return false;
            }
            if (!TestBoxAxis(axisZ1, T, halfSize1.z, halfSize2, axisX2, axisY2, axisZ2))
            {
                return false;
            }

            if (!TestBoxAxis(axisX2, T, halfSize1, halfSize2.x, axisX1, axisY1, axisZ1))
            {
                return false;
            }
            if (!TestBoxAxis(axisY2, T, halfSize1, halfSize2.y, axisX1, axisY1, axisZ1))
            {
                return false;
            }
            if (!TestBoxAxis(axisZ2, T, halfSize1, halfSize2.z, axisX1, axisY1, axisZ1))
            {
                return false;
            }

            // X1 × X2, X1 × Y2, X1 × Z2
            if (!TestCrossAxis(axisX1, axisX2, T, halfSize1, halfSize2, axisX1, axisY1, axisZ1, axisX2, axisY2, axisZ2))
            {
                return false;
            }
            if (!TestCrossAxis(axisX1, axisY2, T, halfSize1, halfSize2, axisX1, axisY1, axisZ1, axisX2, axisY2, axisZ2))
            {
                return false;
            }
            if (!TestCrossAxis(axisX1, axisZ2, T, halfSize1, halfSize2, axisX1, axisY1, axisZ1, axisX2, axisY2, axisZ2))
            {
                return false;
            }

            // Y1 × X2, Y1 × Y2, Y1 × Z2
            if (!TestCrossAxis(axisY1, axisX2, T, halfSize1, halfSize2, axisX1, axisY1, axisZ1, axisX2, axisY2, axisZ2))
            {
                return false;
            }
            if (!TestCrossAxis(axisY1, axisY2, T, halfSize1, halfSize2, axisX1, axisY1, axisZ1, axisX2, axisY2, axisZ2))
            {
                return false;
            }
            if (!TestCrossAxis(axisY1, axisZ2, T, halfSize1, halfSize2, axisX1, axisY1, axisZ1, axisX2, axisY2, axisZ2))
            {
                return false;
            }

            // Z1 × X2, Z1 × Y2, Z1 × Z2
            if (!TestCrossAxis(axisZ1, axisX2, T, halfSize1, halfSize2, axisX1, axisY1, axisZ1, axisX2, axisY2, axisZ2))
            {
                return false;
            }
            if (!TestCrossAxis(axisZ1, axisY2, T, halfSize1, halfSize2, axisX1, axisY1, axisZ1, axisX2, axisY2, axisZ2))
            {
                return false;
            }
            if (!TestCrossAxis(axisZ1, axisZ2, T, halfSize1, halfSize2, axisX1, axisY1, axisZ1, axisX2, axisY2, axisZ2))
            {
                return false;
            }

            return true;
        }

        static bool TestBoxAxis(in Vector3 axis, in Vector3 T, float halfSize1, in Vector3 halfSize2,
            in Vector3 axisX2, in Vector3 axisY2, in Vector3 axisZ2)
        {
            float t = Mathf.Abs(Vector3.Dot(T, axis));

            float r2 =
                halfSize2.x * Mathf.Abs(Vector3.Dot(axisX2, axis)) +
                halfSize2.y * Mathf.Abs(Vector3.Dot(axisY2, axis)) +
                halfSize2.z * Mathf.Abs(Vector3.Dot(axisZ2, axis));

            return t <= halfSize1 + r2;
        }

        static bool TestBoxAxis(in Vector3 axis, in Vector3 T, in Vector3 halfSize1, float halfSize2,
                                 in Vector3 axisX1, in Vector3 axisY1, in Vector3 axisZ1)
        {
            float t = Mathf.Abs(Vector3.Dot(T, axis));

            float r1 =
                halfSize1.x * Mathf.Abs(Vector3.Dot(axisX1, axis)) +
                halfSize1.y * Mathf.Abs(Vector3.Dot(axisY1, axis)) +
                halfSize1.z * Mathf.Abs(Vector3.Dot(axisZ1, axis));

            return t <= r1 + halfSize2;
        }

        static bool TestCrossAxis(in Vector3 a, in Vector3 b, in Vector3 T, in Vector3 halfSize1, in Vector3 halfSize2,
                                   in Vector3 axisX1, in Vector3 axisY1, in Vector3 axisZ1,
                                   in Vector3 axisX2, in Vector3 axisY2, in Vector3 axisZ2)
        {
            Vector3 axis = Vector3.Cross(a, b);

            if (axis.sqrMagnitude < 1e-6f)
            {
                return true;
            }

            axis = axis.normalized;

            float t = Mathf.Abs(Vector3.Dot(T, axis));

            float r1 =
                halfSize1.x * Mathf.Abs(Vector3.Dot(axisX1, axis)) +
                halfSize1.y * Mathf.Abs(Vector3.Dot(axisY1, axis)) +
                halfSize1.z * Mathf.Abs(Vector3.Dot(axisZ1, axis));

            float r2 =
                halfSize2.x * Mathf.Abs(Vector3.Dot(axisX2, axis)) +
                halfSize2.y * Mathf.Abs(Vector3.Dot(axisY2, axis)) +
                halfSize2.z * Mathf.Abs(Vector3.Dot(axisZ2, axis));

            return t <= r1 + r2;
        }
        #endregion
    }
}
