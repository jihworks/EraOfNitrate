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
        /// Divides two integers and rounds up the result.
        /// </summary>
        /// <param name="value">The dividend (must be non-negative).</param>
        /// <param name="divisor">The divisor (must be greater than 0).</param>
        /// <returns>The quotient rounded up to the next integer.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="divisor"/> is less than or equal to 0 -or- when <paramref name="value"/> is negative.</exception>
        /// <exception cref="OverflowException">Thrown when the operation causes an arithmetic overflow.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CeilDivision(this int value, int divisor)
        {
            if (divisor <= 0)
            {
                throw new ArgumentException("Ceiling divisor must be greater than 0.", nameof(divisor));
            }
            if (value < 0)
            {
                throw new ArgumentException("Ceiling value must be non-negative.", nameof(value));
            }
            checked
            {
                return (value + (divisor - 1)) / divisor;
            }
        }
        /// <summary>
        /// Divides two integers and rounds up the result.
        /// </summary>
        /// <param name="value">The dividend (must be non-negative).</param>
        /// <param name="divisor">The divisor (must be greater than 0).</param>
        /// <returns>The quotient rounded up to the next integer.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="divisor"/> is less than or equal to 0 -or- when <paramref name="value"/> is negative.</exception>
        /// <exception cref="OverflowException">Thrown when the operation causes an arithmetic overflow.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long CeilDivision(this long value, long divisor)
        {
            if (divisor <= 0L)
            {
                throw new ArgumentException("Ceiling divisor must be greater than 0.", nameof(divisor));
            }
            if (value < 0L)
            {
                throw new ArgumentException("Ceiling value must be non-negative.", nameof(value));
            }
            checked
            {
                return (value + (divisor - 1L)) / divisor;
            }
        }
        /// <summary>
        /// Divides two integers and rounds up the result.
        /// </summary>
        /// <param name="value">The dividend (must be non-negative).</param>
        /// <param name="divisor">The divisor (must be greater than 0).</param>
        /// <returns>The quotient rounded up to the next integer.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="divisor"/> is equal to 0</exception>
        /// <exception cref="OverflowException">Thrown when the operation causes an arithmetic overflow.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CeilDivision(this uint value, uint divisor)
        {
            if (divisor == 0u)
            {
                throw new ArgumentException("Ceiling divisor must be greater than 0.", nameof(divisor));
            }
            checked
            {
                return (value + (divisor - 1u)) / divisor;
            }
        }
        /// <summary>
        /// Divides two integers and rounds up the result.
        /// </summary>
        /// <param name="value">The dividend (must be non-negative).</param>
        /// <param name="divisor">The divisor (must be greater than 0).</param>
        /// <returns>The quotient rounded up to the next integer.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="divisor"/> is equal to 0</exception>
        /// <exception cref="OverflowException">Thrown when the operation causes an arithmetic overflow.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CeilDivision(this ulong value, ulong divisor)
        {
            if (divisor == 0ul)
            {
                throw new ArgumentException("Ceiling divisor must be greater than 0.", nameof(divisor));
            }
            checked
            {
                return (value + (divisor - 1ul)) / divisor;
            }
        }

        /// <summary>
        /// Rounds up the given value to the nearest multiple of the specified number.
        /// </summary>
        /// <param name="value">The non-negative value to round up.</param>
        /// <param name="multiple">The positive multiple to which the value will be rounded.</param>
        /// <returns>The smallest multiple of <paramref name="multiple"/> that is greater than or equal to <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="multiple"/> is less than or equal to 0 -or- when <paramref name="value"/> is negative.</exception>
        /// <exception cref="OverflowException">Thrown when the operation causes an arithmetic overflow.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CeilToMultiple(this int value, int multiple)
        {
            return CeilDivision(value, multiple) * multiple;
        }
        /// <summary>
        /// Rounds up the given value to the nearest multiple of the specified number.
        /// </summary>
        /// <param name="value">The non-negative value to round up.</param>
        /// <param name="multiple">The positive multiple to which the value will be rounded.</param>
        /// <returns>The smallest multiple of <paramref name="multiple"/> that is greater than or equal to <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="multiple"/> is less than or equal to 0 -or- when <paramref name="value"/> is negative.</exception>
        /// <exception cref="OverflowException">Thrown when the operation causes an arithmetic overflow.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long CeilToMultiple(this long value, long multiple)
        {
            return CeilDivision(value, multiple) * multiple;
        }
        /// <summary>
        /// Rounds up the given value to the nearest multiple of the specified number.
        /// </summary>
        /// <param name="value">The non-negative value to round up.</param>
        /// <param name="multiple">The positive multiple to which the value will be rounded.</param>
        /// <returns>The smallest multiple of <paramref name="multiple"/> that is greater than or equal to <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="multiple"/> is equal to 0</exception>
        /// <exception cref="OverflowException">Thrown when the operation causes an arithmetic overflow.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint CeilToMultiple(this uint value, uint multiple)
        {
            return CeilDivision(value, multiple) * multiple;
        }
        /// <summary>
        /// Rounds up the given value to the nearest multiple of the specified number.
        /// </summary>
        /// <param name="value">The non-negative value to round up.</param>
        /// <param name="multiple">The positive multiple to which the value will be rounded.</param>
        /// <returns>The smallest multiple of <paramref name="multiple"/> that is greater than or equal to <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="multiple"/> is equal to 0</exception>
        /// <exception cref="OverflowException">Thrown when the operation causes an arithmetic overflow.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong CeilToMultiple(this ulong value, ulong multiple)
        {
            return CeilDivision(value, multiple) * multiple;
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

        /// <summary>
        /// Möller-Trumbore algorithm implementation.
        /// </summary>
        public static bool AreRayTriangleIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 v0, Vector3 v1, Vector3 v2, out float t)
        {
            t = 0f;

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 h = Vector3.Cross(rayDirection, edge2);
            float a = Vector3.Dot(edge1, h);

            // Check if ray is parallel to the triangle
            if (a > -0.0001f && a < 0.0001f)
            {
                return false;
            }

            float f = 1f / a;
            Vector3 s = rayOrigin - v0;
            float u = f * Vector3.Dot(s, h);

            // Check if intersection is outside the triangle
            if (u < 0f || u > 1f)
            {
                return false;
            }

            Vector3 q = Vector3.Cross(s, edge1);
            float v = f * Vector3.Dot(rayDirection, q);

            // Check if intersection is outside the triangle
            if (v < 0f || u + v > 1f)
            {
                return false;
            }

            // Compute t to find out where the intersection point is on the line
            t = f * Vector3.Dot(edge2, q);

            // Check if the intersection is behind the ray origin
            if (t > 0.0001f)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if two 3D triangles intersect.<br/>
        /// Vertices should be in CW or CCW order consistently.
        /// </summary>
        public static bool AreTrianglesIntersect(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 u0, Vector3 u1, Vector3 u2)
        {
            const float Epsilon = 0.0001f;

            if (IsDegenerate(v0, v1, v2) || IsDegenerate(u0, u1, u2))
            {
                return false;
            }

            // Compute plane equation of triangle(v0,v1,v2) 
            Vector3 e1 = v1 - v0;
            Vector3 e2 = v2 - v0;
            Vector3 n1 = Vector3.Cross(e1, e2);
            float d1 = -Vector3.Dot(n1, v0);
            // Plane equation 1: N1.x + d1 = 0

            // Put u0,u1,u2 into plane equation 1 to compute signed distances to the plane
            float du0 = Vector3.Dot(n1, u0) + d1;
            float du1 = Vector3.Dot(n1, u1) + d1;
            float du2 = Vector3.Dot(n1, u2) + d1;

            // Coplanarity robustness check 
            if (Mathf.Abs(du0) < Epsilon) du0 = 0f;
            if (Mathf.Abs(du1) < Epsilon) du1 = 0f;
            if (Mathf.Abs(du2) < Epsilon) du2 = 0f;

            float du0du1 = du0 * du1;
            float du0du2 = du0 * du2;

            // Same sign on all of them + not equal 0 ? 
            if (du0du1 > 0f && du0du2 > 0f)
            {
                return false; // No intersection occurs
            }

            // Compute plane of triangle (u0,u1,u2)
            Vector3 e1_2 = u1 - u0;
            Vector3 e2_2 = u2 - u0;
            Vector3 n2 = Vector3.Cross(e1_2, e2_2);
            float d2 = -Vector3.Dot(n2, u0);

            // Plane equation 2: N2.x + d2 = 0 
            // Put v0,v1,v2 into plane equation 2
            float dv0 = Vector3.Dot(n2, v0) + d2;
            float dv1 = Vector3.Dot(n2, v1) + d2;
            float dv2 = Vector3.Dot(n2, v2) + d2;

            if (Mathf.Abs(dv0) < Epsilon) dv0 = 0f;
            if (Mathf.Abs(dv1) < Epsilon) dv1 = 0f;
            if (Mathf.Abs(dv2) < Epsilon) dv2 = 0f;

            float dv0dv1 = dv0 * dv1;
            float dv0dv2 = dv0 * dv2;

            // Same sign on all of them + not equal 0 ? 
            if (dv0dv1 > 0f && dv0dv2 > 0f)
            {
                return false; // No intersection occurs
            }

            // Compute direction of intersection line 
            Vector3 dd = Vector3.Cross(n1, n2);

            // Compute and index to the largest component of D 
            float max = Mathf.Abs(dd.x);
            short index = 0;
            float bb = Mathf.Abs(dd.y);
            float cc = Mathf.Abs(dd.z);

            if (bb > max) { max = bb; index = 1; }
            if (cc > max) { index = 2; }

            // This is the simplified projection onto L
            float vp0 = v0[index];
            float vp1 = v1[index];
            float vp2 = v2[index];

            float up0 = u0[index];
            float up1 = u1[index];
            float up2 = u2[index];

            // Compute interval for triangle 1 
            float a = 0, b = 0, c = 0, x0 = 0, x1 = 0;
            if (ComputeIntervals(vp0, vp1, vp2, dv0, dv1, dv2, dv0dv1, dv0dv2, ref a, ref b, ref c, ref x0, ref x1))
            {
                return TriTriCoplanar(n1, v0, v1, v2, u0, u1, u2);
            }

            // Compute interval for triangle 2 
            float d = 0, e = 0, f = 0, y0 = 0, y1 = 0;
            if (ComputeIntervals(up0, up1, up2, du0, du1, du2, du0du1, du0du2, ref d, ref e, ref f, ref y0, ref y1))
            {
                return TriTriCoplanar(n1, v0, v1, v2, u0, u1, u2);
            }

            float xx = x0 * x1;
            float yy = y0 * y1;
            float xxyy = xx * yy;

            float tmp1 = a * xxyy;
            Vector2 isect1 = new(tmp1 + b * x1 * yy, tmp1 + c * x0 * yy);

            float tmp2 = d * xxyy;
            Vector2 isect2 = new(tmp2 + e * xx * y1, tmp2 + f * xx * y0);

            Sort(ref isect1);
            Sort(ref isect2);

            return !(isect1.y < isect2.x || isect2.y < isect1.x);


            static bool IsDegenerate(Vector3 a, Vector3 b, Vector3 c)
            {
                return (a - b).sqrMagnitude < Epsilon ||
                       (b - c).sqrMagnitude < Epsilon ||
                       (c - a).sqrMagnitude < Epsilon;
            }
            static void Sort(ref Vector2 v)
            {
                if (v.x > v.y)
                {
                    (v.y, v.x) = (v.x, v.y);
                }
            }
            static bool EdgeEdgeTest(Vector3 v0, Vector3 v1, Vector3 u0, Vector3 u1, int i0, int i1)
            {
                float Ax = v1[i0] - v0[i0];
                float Ay = v1[i1] - v0[i1];

                float Bx = u0[i0] - u1[i0];
                float By = u0[i1] - u1[i1];
                float Cx = v0[i0] - u0[i0];
                float Cy = v0[i1] - u0[i1];

                float f = Ay * Bx - Ax * By;
                float d = By * Cx - Bx * Cy;

                if ((f > 0 && d >= 0 && d <= f) || (f < 0 && d <= 0 && d >= f))
                {
                    float e = Ax * Cy - Ay * Cx;
                    if (f > 0)
                    {
                        if (e >= 0 && e <= f) return true;
                    }
                    else
                    {
                        if (e <= 0 && e >= f) return true;
                    }
                }

                return false;
            }
            static bool EdgeAgainstTriEdges(Vector3 v0, Vector3 v1, Vector3 u0, Vector3 u1, Vector3 u2, short i0, short i1)
            {
                // Test edge u0,u1 against v0,v1
                if (EdgeEdgeTest(v0, v1, u0, u1, i0, i1)) return true;

                // Test edge u1,u2 against v0,v1 
                if (EdgeEdgeTest(v0, v1, u1, u2, i0, i1)) return true;

                // Test edge u2,u1 against v0,v1 
                if (EdgeEdgeTest(v0, v1, u2, u0, i0, i1)) return true;

                return false;
            }
            static bool PointInTri(Vector3 v0, Vector3 u0, Vector3 u1, Vector3 u2, short i0, short i1)
            {
                // Is T1 completely inside T2?
                // Check if v0 is inside tri(u0,u1,u2)
                float a = u1[i1] - u0[i1];
                float b = -(u1[i0] - u0[i0]);
                float c = -a * u0[i0] - b * u0[i1];
                float d0 = a * v0[i0] + b * v0[i1] + c;

                a = u2[i1] - u1[i1];
                b = -(u2[i0] - u1[i0]);
                c = -a * u1[i0] - b * u1[i1];
                float d1 = a * v0[i0] + b * v0[i1] + c;

                a = u0[i1] - u2[i1];
                b = -(u0[i0] - u2[i0]);
                c = -a * u2[i0] - b * u2[i1];
                float d2 = a * v0[i0] + b * v0[i1] + c;

                if (d0 * d1 > 0f)
                {
                    if (d0 * d2 > 0f) return true;
                }

                return false;
            }
            static bool TriTriCoplanar(Vector3 N, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 u0, Vector3 u1, Vector3 u2)
            {
                short i0, i1;

                // First project onto an axis-aligned plane, that maximizes the area
                // of the triangles, compute indices: i0,i1. 
                float absNx = Mathf.Abs(N.x);
                float absNy = Mathf.Abs(N.y);
                float absNz = Mathf.Abs(N.z);

                if (absNx > absNy)
                {
                    if (absNx > absNz)
                    {
                        i0 = 1; // y
                        i1 = 2; // z
                    }
                    else
                    {
                        i0 = 0; // x
                        i1 = 1; // y
                    }
                }
                else
                {
                    if (absNz > absNy)
                    {
                        i0 = 0; // x
                        i1 = 1; // y
                    }
                    else
                    {
                        i0 = 0; // x
                        i1 = 2; // z
                    }
                }

                // Test all edges of triangle 1 against the edges of triangle 2 
                if (EdgeAgainstTriEdges(v0, v1, u0, u1, u2, i0, i1)) return true;
                if (EdgeAgainstTriEdges(v1, v2, u0, u1, u2, i0, i1)) return true;
                if (EdgeAgainstTriEdges(v2, v0, u0, u1, u2, i0, i1)) return true;

                // Finally, test if tri1 is totally contained in tri2 or vice versa 
                if (PointInTri(v0, u0, u1, u2, i0, i1)) return true;
                if (PointInTri(u0, v0, v1, v2, i0, i1)) return true;

                return false;
            }
            static bool ComputeIntervals(float VV0, float VV1, float VV2,
            float D0, float D1, float D2, float D0D1, float D0D2,
            ref float A, ref float B, ref float C, ref float X0, ref float X1)
            {
                if (D0D1 > 0f)
                {
                    // Here we know that D0D2 <= 0.0 
                    // that is D0, D1 are on the same side, D2 on the other or on the plane 
                    A = VV2; B = (VV0 - VV2) * D2; C = (VV1 - VV2) * D2; X0 = D2 - D0; X1 = D2 - D1;
                }
                else if (D0D2 > 0f)
                {
                    // Here we know that d0d1 <= 0.0 
                    A = VV1; B = (VV0 - VV1) * D1; C = (VV2 - VV1) * D1; X0 = D1 - D0; X1 = D1 - D2;
                }
                else if (D1 * D2 > 0f || D0 != 0f)
                {
                    // Here we know that d0d1 <= 0.0 or that D0 != 0.0 
                    A = VV0; B = (VV1 - VV0) * D0; C = (VV2 - VV0) * D0; X0 = D0 - D1; X1 = D0 - D2;
                }
                else if (D1 != 0f)
                {
                    A = VV1; B = (VV0 - VV1) * D1; C = (VV2 - VV1) * D1; X0 = D1 - D0; X1 = D1 - D2;
                }
                else if (D2 != 0f)
                {
                    A = VV2; B = (VV0 - VV2) * D2; C = (VV1 - VV2) * D2; X0 = D2 - D0; X1 = D2 - D1;
                }
                else
                {
                    return true;
                }

                return false;
            }
        }
    }
}
