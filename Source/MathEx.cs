// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Jih.Unity.Infrastructure
{
    public static class MathEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Min(ref int left, int right)
        {
            left = Math.Min(left, right);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Min(ref long left, long right)
        {
            left = Math.Min(left, right);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Min(ref float left, float right)
        {
            left = Math.Min(left, right);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Min(ref double left, double right)
        {
            left = Math.Min(left, right);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Min(ref decimal left, decimal right)
        {
            left = Math.Min(left, right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Max(ref int left, int right)
        {
            left = Math.Max(left, right);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Max(ref long left, long right)
        {
            left = Math.Max(left, right);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Max(ref float left, float right)
        {
            left = Math.Max(left, right);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Max(ref double left, double right)
        {
            left = Math.Max(left, right);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Max(ref decimal left, decimal right)
        {
            left = Math.Max(left, right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int v0, int v1, int v2)
        {
            return Math.Min(v0, Math.Min(v1, v2));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Min(long v0, long v1, long v2)
        {
            return Math.Min(v0, Math.Min(v1, v2));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(float v0, float v1, float v2)
        {
            return Math.Min(v0, Math.Min(v1, v2));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Min(double v0, double v1, double v2)
        {
            return Math.Min(v0, Math.Min(v1, v2));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Min(decimal v0, decimal v1, decimal v2)
        {
            return Math.Min(v0, Math.Min(v1, v2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(int v0, int v1, int v2)
        {
            return Math.Max(v0, Math.Max(v1, v2));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Max(long v0, long v1, long v2)
        {
            return Math.Max(v0, Math.Max(v1, v2));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(float v0, float v1, float v2)
        {
            return Math.Max(v0, Math.Max(v1, v2));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Max(double v0, double v1, double v2)
        {
            return Math.Max(v0, Math.Max(v1, v2));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Max(decimal v0, decimal v1, decimal v2)
        {
            return Math.Max(v0, Math.Max(v1, v2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref int value, int min, int max)
        {
            value = Math.Clamp(value, min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref long value, long min, long max)
        {
            value = Math.Clamp(value, min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref float value, float min, float max)
        {
            value = Math.Clamp(value, min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref double value, double min, double max)
        {
            value = Math.Clamp(value, min, max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(ref decimal value, decimal min, decimal max)
        {
            value = Math.Clamp(value, min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float p1, float p2, float alpha)
        {
            return p1 + (p2 - p1) * alpha;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Lerp(double p1, double p2, double alpha)
        {
            return p1 + (p2 - p1) * alpha;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal Lerp(decimal p1, decimal p2, decimal alpha)
        {
            return p1 + (p2 - p1) * alpha;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SafeInverse(this float v, float fallback = 0f)
        {
            return v != 0f ? 1f / v : fallback;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double SafeInverse(this double v, double fallback = 0d)
        {
            return v != 0d ? 1d / v : fallback;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal SafeInverse(this decimal v, decimal fallback = 0m)
        {
            return v != 0m ? 1m / v : fallback;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SafeInverse(this Vector2 v, float fallback = 0f)
        {
            return new Vector2(v.x.SafeInverse(fallback), v.y.SafeInverse(fallback));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SafeInverse(this Vector3 v, float fallback = 0f)
        {
            return new Vector3(v.x.SafeInverse(fallback), v.y.SafeInverse(fallback), v.z.SafeInverse(fallback));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 SafeInverse(this Vector4 v, float fallback = 0f)
        {
            return new Vector4(v.x.SafeInverse(fallback), v.y.SafeInverse(fallback), v.z.SafeInverse(fallback), v.w.SafeInverse(fallback));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SafeDivide(this int left, int right, int fallback = 0)
        {
            return 0 != right ? left / right : fallback;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SafeDivide(this long left, long right, long fallback = 0L)
        {
            return 0L != right ? left / right : fallback;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SafeDivide(this float left, float right, float fallback = 0f)
        {
            return 0f != right ? left / right : fallback;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double SafeDivide(this double left, double right, double fallback = 0d)
        {
            return 0d != right ? left / right : fallback;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal SafeDivide(this decimal left, decimal right, decimal fallback = 0m)
        {
            return 0m != right ? left / right : fallback;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearly(this float left, float right, float tolerance = 0.0001f)
        {
            return Math.Abs(left - right) <= Math.Abs(tolerance);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearly(this double left, double right, double tolerance = 0.0001d)
        {
            return Math.Abs(left - right) <= Math.Abs(tolerance);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearly(this decimal left, decimal right, decimal tolerance = 0.0001m)
        {
            return Math.Abs(left - right) <= Math.Abs(tolerance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearly(this Vector2 left, Vector2 right, float tolerance = 0.0001f)
        {
            return left.x.IsNearly(right.x, tolerance) &&
                left.y.IsNearly(right.y, tolerance);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearly(this Vector3 left, Vector3 right, float tolerance = 0.0001f)
        {
            return left.x.IsNearly(right.x, tolerance) &&
                left.y.IsNearly(right.y, tolerance) &&
                left.z.IsNearly(right.z, tolerance);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearly(this Vector4 left, Vector4 right, float tolerance = 0.0001f)
        {
            return left.x.IsNearly(right.x, tolerance) &&
                left.y.IsNearly(right.y, tolerance) &&
                left.z.IsNearly(right.z, tolerance) &&
                left.w.IsNearly(right.w, tolerance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyZero(this float left, float tolerance = 0.0001f)
        {
            return IsNearly(left, 0f, tolerance);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyZero(this double left, double tolerance = 0.0001d)
        {
            return IsNearly(left, 0d, tolerance);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyZero(this decimal left, decimal tolerance = 0.0001m)
        {
            return IsNearly(left, 0m, tolerance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyZero(this Vector2 left, float tolerance = 0.0001f)
        {
            return left.x.IsNearlyZero(tolerance) &&
                left.y.IsNearlyZero(tolerance);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyZero(this Vector3 left, float tolerance = 0.0001f)
        {
            return left.x.IsNearlyZero(tolerance) &&
                left.y.IsNearlyZero(tolerance) &&
                left.z.IsNearlyZero(tolerance);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearlyZero(this Vector4 left, float tolerance = 0.0001f)
        {
            return left.x.IsNearlyZero(tolerance) &&
                left.y.IsNearlyZero(tolerance) &&
                left.z.IsNearlyZero(tolerance) &&
                left.w.IsNearlyZero(tolerance);
        }

        /// <param name="d">Angle in degrees.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToRadians(this float d)
        {
            return d / 180f * Mathf.PI;
        }
        /// <param name="d">Angle in degrees.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToRadians(this double d)
        {
            return d / 180d * Math.PI;
        }

        /// <param name="r">Angle in radians.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToDegrees(this float r)
        {
            return r * 180f / Mathf.PI;
        }
        /// <param name="r">Angle in radians.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToDegrees(this double r)
        {
            return r * 180d / Math.PI;
        }

        /// <summary>
        /// Collapse angle degrees to [0, 360).
        /// </summary>
        /// <param name="d">Angle in degrees.</param>
        public static float CollapseDegrees(this float d)
        {
            // Check already in [0, 360)
            if (0f <= d && d < 360f)
            {
                return d;
            }

            // Modulo to (-360, 360)
            d %= 360f;

            // If negative, add 360 to adjust to [0, 360)
            if (d < 0f)
            {
                d += 360f;
            }

            return d;
        }
        /// <summary>
        /// Collapse angle degrees to [0, 360).
        /// </summary>
        /// <param name="d">Angle in degrees.</param>
        public static int CollapseDegrees(this int d)
        {
            // Check already in [0, 360)
            if (0 <= d && d < 360)
            {
                return d;
            }

            // Modulo to (-360, 360)
            d %= 360;

            // If negative, add 360 to adjust to [0, 360)
            if (d < 0)
            {
                d += 360;
            }

            return d;
        }

        /// <summary>
        /// Normalize angle degrees to [-180, 180).
        /// </summary>
        /// <param name="d">Angle in degrees.</param>
        public static float NormalizeDegrees(this float d)
        {
            // Check already in [-180, 180)
            if (-180f <= d && d < 180f)
            {
                return d;
            }

            // Modulo to (-360, 360)
            d %= 360f;

            // If negative, add 360 to adjust to [0, 360)
            if (d < 0f)
            {
                d += 360f;
            }

            // If 180 or more, subtract 360 to adjust to [-180, 180)
            if (d >= 180f)
            {
                d -= 360f;
            }

            return d;
        }
        /// <summary>
        /// Normalize angle degrees to [-180, 180).
        /// </summary>
        /// <param name="d">Angle in degrees.</param>
        public static int NormalizeDegrees(this int d)
        {
            // Check already in [-180, 180)
            if (-180 <= d && d < 180)
            {
                return d;
            }

            // Modulo to (-360, 360)
            d %= 360;

            // If negative, add 360 to adjust to [0, 360)
            if (d < 0)
            {
                d += 360;
            }

            // If 180 or more, subtract 360 to adjust to [-180, 180)
            if (d >= 180)
            {
                d -= 360;
            }

            return d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sq(this float v)
        {
            return v * v;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sq(this double v)
        {
            return v * v;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sq(this int v)
        {
            return v * v;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Sq(this long v)
        {
            return v * v;
        }

        /// <summary>
        /// Radius vector of the given angle on the unit circle in euclidean 2D space.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>0º: (1, 0)</item>
        /// <item>90º: (0, 1)</item>
        /// <item>180º: (-1, 0)</item>
        /// <item>270º: (0, -1)</item>
        /// </list>
        /// </remarks>
        /// <param name="radians">Angle in <b>radians</b>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 RadiusVector(float radians)
        {
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }
        /// <summary>
        /// Calculates the angle of the given radius vector on the unit circle in euclidean 2D space.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>(1, 0): 0º</item>
        /// <item>(0, 1): 90º</item>
        /// <item>(-1, 0): 180º</item>
        /// <item>(0, -1): 270º</item>
        /// </list>
        /// </remarks>
        /// <param name="v">It must be normalized.</param>
        /// <returns>Angle in <b>radians</b>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RadiusRadians(this Vector2 v)
        {
            return Mathf.Atan2(v.y, v.x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Negate(this Quaternion q)
        {
            return new Quaternion(-q.x, -q.y, -q.z, -q.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion ShortestSlerp(Quaternion q0, Quaternion q1, float alpha)
        {
            if (Quaternion.Dot(q0, q1) < 0f)
            {
                return Quaternion.Slerp(q0, q1.Negate(), alpha);
            }
            return Quaternion.Slerp(q0, q1, alpha);
        }

        /// <summary>
        /// Returns the closest point on a line segment to a given point.
        /// </summary>
        /// <param name="point">The point to find the closest point to.</param>
        /// <returns>The closest point on the line.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
        {
            float t = GetClosestPointPositionOnLine(lineStart, lineEnd, point);
            return lineStart + (lineEnd - lineStart) * t;
        }

        /// <summary>
        /// Returns the position value t in range [0, 1] representing the closest point on a line to a given point.
        /// </summary>
        /// <param name="point">The point to find the closest point to.</param>
        /// <returns>A position value t in range [0, 1] representing the closest point on the line.</returns>
        public static float GetClosestPointPositionOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
        {
            Vector3 lineDirection = lineEnd - lineStart;
            float sqrLineLength = lineDirection.sqrMagnitude;

            // Handle degenerate case where line is a point
            if (sqrLineLength.IsNearlyZero())
            {
                return 0f;
            }

            // Project (point - lineStart) onto lineDirection
            float dotProduct = Vector3.Dot(point - lineStart, lineDirection) / sqrLineLength;

            return Math.Clamp(dotProduct, 0f, 1f);
        }

        /// <param name="point1">Closest point on line 1.</param>
        /// <param name="point2">Closest point on line 2.</param>
        /// <returns><c>false</c> if failed to get closest point because the two lines are almost parallel.</returns>
        public static bool GetClosestPointsBetweenLines(
            Vector3 line1Start, Vector3 line1End,
            Vector3 line2Start, Vector3 line2End,
            out Vector3 point1, out Vector3 point2)
        {
            Vector3 u = line1End - line1Start;
            Vector3 v = line2End - line2Start;
            Vector3 w = line1Start - line2Start;

            float a = Vector3.Dot(u, u);
            float b = Vector3.Dot(u, v);
            float c = Vector3.Dot(v, v);
            float d = Vector3.Dot(u, w);
            float e = Vector3.Dot(v, w);
            float denominator = a * c - b * b;

            // Almost parallel lines
            if (denominator.IsNearlyZero())
            {
                point1 = line1Start;
                point2 = line2Start + Vector3.Project(w, v);
                return false;
            }

            float sc = (b * e - c * d) / denominator;
            float tc = (a * e - b * d) / denominator;

            point1 = line1Start + u * sc;
            point2 = line2Start + v * tc;

            return true;
        }

        /// <summary>
        /// Gets the angle between the two directions:<br/>
        /// from <paramref name="prevLocation"/> to <paramref name="currentLocation"/> and from <paramref name="currentLocation"/> to <paramref name="nextLocation"/>.
        /// </summary>
        /// <returns>In degrees.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetInterAngle(Vector3 prevLocation, Vector3 currentLocation, Vector3 nextLocation)
        {
            return GetInterAngle(prevLocation, currentLocation, nextLocation, out _, out _);
        }
        /// <summary>
        /// Gets the angle between the two directions:<br/>
        /// from <paramref name="prevLocation"/> to <paramref name="currentLocation"/> and from <paramref name="currentLocation"/> to <paramref name="nextLocation"/>.
        /// </summary>
        /// <returns>In degrees.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetInterAngle(Vector3 prevLocation, Vector3 currentLocation, Vector3 nextLocation, out Vector3 direction0, out Vector3 direction1)
        {
            direction0 = (currentLocation - prevLocation).normalized;
            direction1 = (nextLocation - currentLocation).normalized;
            return GetInterAngle(direction0, direction1);
        }

        /// <returns>Whether the <paramref name="worldLocation"/> is in front of the <paramref name="camera"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInFrontOf(Camera camera, Vector3 worldLocation)
        {
            return GetInterAngle(camera, worldLocation) < 90f;
        }
        /// <summary>
        /// Gets the angle between the camera's forward direction and the direction from the camera to the given world location.
        /// </summary>
        /// <returns>In degrees.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetInterAngle(Camera camera, Vector3 worldLocation)
        {
            Vector3 camForward = camera.transform.forward;
            Vector3 destDir = (worldLocation - camera.transform.position).normalized;
            return GetInterAngle(camForward, destDir);
        }
        /// <returns>In degrees.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetInterAngle(Vector3 direction0, Vector3 direction1)
        {
            float cos = Vector3.Dot(direction0, direction1);
            Clamp(ref cos, -1f, 1f);
            return Mathf.Acos(cos).ToDegrees();
        }
    }
}
