// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using System.Collections.Generic;

namespace Jih.Unity.Infrastructure.HexaGrid
{
    /// <summary>
    /// Basis +A = D60 | C2 | NE<br/>
    /// Basis +B = D180 | C6 | S<br/>
    /// Basis +C = D300 | C10 | NW<br/>
    /// </summary>
    /// <remarks>
    /// A + B + C must be 0.<br/>
    /// <br/>
    /// Adding A = moves to D30 or D90<br/>
    /// Adding B = moves to D150 or D210<br/>
    /// Adding C = moves to D270 or D330<br/>
    /// Exact moving direction is determined by which axis will subtract or fix.<br/>
    /// Use <see cref="HexaPositionEx.GetOffset(HexaNeighborPosition)"/> for convenience.
    /// </remarks>
    public readonly struct HexaCoord : IEquatable<HexaCoord>
    {
        public static HexaCoord Add(in HexaCoord left, in HexaCoord right)
        {
            return new(left.A + right.A, left.B + right.B, left.C + right.C);
        }
        public static HexaCoord Subtract(in HexaCoord left, in HexaCoord right)
        {
            return new(left.A - right.A, left.B - right.B, left.C - right.C);
        }
        public static HexaCoord Multiply(in HexaCoord left, int scale)
        {
            return new(left.A * scale, left.B * scale, left.C * scale);
        }

        /// <summary>
        /// Rotate the coordinate 60 degrees in CW with origin(0, 0, 0) as axis.
        /// </summary>
        public static HexaCoord RotateCw(in HexaCoord hexCoord)
        {
            return new(-hexCoord.B, -hexCoord.C, -hexCoord.A);
        }
        /// <summary>
        /// Rotate the coordinate 60 degrees in CCW with origin(0, 0, 0) as axis.
        /// </summary>
        public static HexaCoord RotateCcw(in HexaCoord hexCoord)
        {
            return new(-hexCoord.C, -hexCoord.A, -hexCoord.B);
        }

        public static int Distance(in HexaCoord left, in HexaCoord right)
        {
            return Subtract(left, right).GetLength();
        }

        /// <summary>
        /// Collect cell coordinates on the given line segment.
        /// </summary>
        public static int Sweep(in HexaCoord start, in HexaCoord end, List<HexaCoord>? buffer)
        {
            int dist = Distance(start, end);

            if (buffer is not null)
            {
                float invDist = ((float)dist).SafeInverse();

                HexaCoordF epsilon = new(1e-6f, 2e-6f, -3e-6f);
                HexaCoordF startF = start + epsilon;
                HexaCoordF endF = end + epsilon;

                for (int i = 0; i <= dist; i++)
                {
                    buffer.Add((HexaCoord)HexaCoordF.Lerp(startF, endF, invDist * i));
                }
            }
            return dist + 1;
        }

        public readonly int A, B, C;

        public HexaCoord(int a, int b, int c)
        {
            if (a + b + c != 0)
            {
                throw new InvalidOperationException("Invalid hexa coordinate.");
            }
            A = a;
            B = b;
            C = c;
        }

        /// <param name="radius">1 means direct neighbors.</param>
        public int GetRing(int radius, List<HexaCoord>? buffer)
        {
            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius cannot be 0 or negative.");
            }

            if (buffer is not null)
            {
                HexaCoord coord = this + (HexaNeighborPosition.D270.GetOffset() * radius);

                for (int i = 0; i < 6; i++)
                {
                    HexaCoord offset = HexaPositionEx.GetOffset((HexaNeighborPosition)i);

                    for (int j = 0; j < radius; j++)
                    {
                        buffer.Add(coord);

                        coord += offset;
                    }
                }
            }
            return 6 * radius;
        }

        public readonly HexaCoord GetNeighbor(HexaNeighborPosition position)
        {
            return this + position.GetOffset();
        }

        public readonly HexaCoord GetDiagonal(HexaDiagonalPosition position)
        {
            return this + position.GetOffset();
        }

        public readonly int GetLength()
        {
            return (Math.Abs(A) + Math.Abs(B) + Math.Abs(C)) / 2;
        }

        public readonly override string ToString()
        {
            return $"<{A}, {B}, {C}>";
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is HexaCoord coord && Equals(coord);
        }

        public readonly bool Equals(HexaCoord other)
        {
            return A == other.A &&
                   B == other.B &&
                   C == other.C;
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(A, B, C);
        }

        public static HexaCoord operator +(HexaCoord left, HexaCoord right)
        {
            return Add(left, right);
        }
        public static HexaCoord operator -(HexaCoord left, HexaCoord right)
        {
            return Subtract(left, right);
        }
        public static HexaCoord operator *(HexaCoord left, int scale)
        {
            return Multiply(left, scale);
        }

        public static implicit operator HexaCoordF(HexaCoord hexCoord)
        {
            return new(hexCoord.A, hexCoord.B, hexCoord.C);
        }

        public static bool operator ==(HexaCoord left, HexaCoord right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(HexaCoord left, HexaCoord right)
        {
            return !(left == right);
        }
    }
}
