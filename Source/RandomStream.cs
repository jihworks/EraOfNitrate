// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;

namespace Jih.Unity.Infrastructure
{
    /// <summary>
    /// Supports random access to the random numbers sequence by Position value.
    /// </summary>
    /// <remarks>
    /// <b>NOT</b> thread-safe.
    /// </remarks>
    /// <seealso cref="Philox4x32"/>
    public class RandomStream : IRandomInt32
    {
        public int Seed { get; }

        long _position = 0;
        public long Position
        {
            get => _position;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Position must be non-negative.");
                }
                _position = value;
            }
        }

        readonly Philox4x32 _generator;

        public RandomStream() : this(Environment.TickCount)
        {
        }
        public RandomStream(int seed)
        {
            Seed = seed;
            _generator = new Philox4x32(seed);
        }

        public uint GetUInt32()
        {
            return _generator.GenerateUInt32(_position);
        }
        public ulong GetUInt64()
        {
            return _generator.GenerateUInt64(_position);
        }
        public int GetInt32(int minInclusive, int maxExclusive)
        {
            return _generator.GenerateInt32(_position, minInclusive, maxExclusive);
        }
        public double GetDouble()
        {
            return _generator.GenerateDouble(_position);
        }
        public bool GetBoolean()
        {
            return _generator.GenerateBoolean(_position);
        }
        public void GetBytes(Span<byte> destination)
        {
            _generator.GenerateBytes(_position, destination);
        }

        public uint NextUInt32()
        {
            return _generator.GenerateUInt32(checked(_position++));
        }
        public ulong NextUInt64()
        {
            return _generator.GenerateUInt64(checked(_position++));
        }
        public int NextInt32(int minInclusive, int maxExclusive)
        {
            return _generator.GenerateInt32(checked(_position++), minInclusive, maxExclusive);
        }
        public double NextDouble()
        {
            return _generator.GenerateDouble(checked(_position++));
        }
        public bool NextBoolean()
        {
            return _generator.GenerateBoolean(checked(_position++));
        }
        public void NextBytes(Span<byte> destination)
        {
            _generator.GenerateBytes(checked(_position++), destination);
        }
    }
}
