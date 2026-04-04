// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;

namespace Jih.Unity.Infrastructure.HexaGrid
{
    public readonly struct HexaCoordF : IEquatable<HexaCoordF>
    {
        public static HexaCoordF Add(in HexaCoordF left, in HexaCoordF right)
        {
            return new(left.A + right.A, left.B + right.B, left.C + right.C);
        }
        public static HexaCoordF Subtract(in HexaCoordF left, in HexaCoordF right)
        {
            return new(left.A - right.A, left.B - right.B, left.C - right.C);
        }
        public static HexaCoordF Multiply(in HexaCoordF left, int scale)
        {
            return new(left.A * scale, left.B * scale, left.C * scale);
        }

        public static HexaCoord Round(in HexaCoordF hexCoord)
        {
            int a = (int)Math.Round(hexCoord.A);
            int b = (int)Math.Round(hexCoord.B);
            int c = (int)Math.Round(hexCoord.C);
            double da = Math.Abs(a - hexCoord.A);
            double db = Math.Abs(b - hexCoord.B);
            double dc = Math.Abs(c - hexCoord.C);
            if (da > db && da > dc)
            {
                a = -b - c;
            }
            else if (db > dc)
            {
                b = -a - c;
            }
            else
            {
                c = -a - b;
            }
            return new HexaCoord(a, b, c);
        }

        public static float Distance(in HexaCoordF left, in HexaCoordF right)
        {
            return Subtract(left, right).GetLength();
        }

        public static HexaCoordF Lerp(in HexaCoordF left, in HexaCoordF right, float alpha)
        {
            return new(MathEx.Lerp(left.A, right.A, alpha), MathEx.Lerp(left.B, right.B, alpha), MathEx.Lerp(left.C, right.C, alpha));
        }

        public readonly float A, B, C;

        public HexaCoordF(float a, float b, float c)
        {
            if (Math.Round(a + b + c) != 0f)
            {
                throw new InvalidOperationException("Invalid hexa coordinate.");
            }
            A = a;
            B = b;
            C = c;
        }

        public readonly float GetLength()
        {
            return (Math.Abs(A) + Math.Abs(B) + Math.Abs(C)) / 2f;
        }

        public readonly override string ToString()
        {
            return $"<{A}, {B}, {C}>";
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is HexaCoordF f && Equals(f);
        }

        public readonly bool Equals(HexaCoordF other)
        {
            return A == other.A &&
                   B == other.B &&
                   C == other.C;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(A, B, C);
        }

        public static HexaCoordF operator +(HexaCoordF left, HexaCoordF right)
        {
            return Add(left, right);
        }
        public static HexaCoordF operator -(HexaCoordF left, HexaCoordF right)
        {
            return Subtract(left, right);
        }
        public static HexaCoordF operator *(HexaCoordF left, int scale)
        {
            return Multiply(left, scale);
        }

        /// <summary>
        /// Using round method.
        /// </summary>
        /// <seealso cref="Round(in HexaCoordF)"/>
        public static explicit operator HexaCoord(HexaCoordF hexCoord)
        {
            return Round(hexCoord);
        }

        public static bool operator ==(HexaCoordF left, HexaCoordF right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(HexaCoordF left, HexaCoordF right)
        {
            return !(left == right);
        }
    }
}
