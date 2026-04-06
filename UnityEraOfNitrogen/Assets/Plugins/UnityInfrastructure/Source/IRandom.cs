// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;

namespace Jih.Unity.Infrastructure
{
    /// <seealso cref="SystemRandom"/>
    /// <seealso cref="UnityRandom"/>
    /// <seealso cref="RandomStream"/>
    public interface IRandomInt32
    {
        /// <summary>
        /// Implementing class must return a random <c>int</c> that is greater than or equal to <paramref name="minInclusive"/> and <b>less</b> than <paramref name="maxExclusive"/>.
        /// </summary>
        /// <remarks>
        /// The distribution of returned values must be approximately uniform across the range of possible values.
        /// </remarks>
        /// <param name="minInclusive">Inclusive minimum value.</param>
        /// <param name="maxExclusive"><b>Exclusive</b> maximum value.</param>
        /// <returns>[<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>)</returns>
        int NextInt32(int minInclusive, int maxExclusive);
    }

    public class SystemRandom : Random, IRandomInt32
    {
        public SystemRandom()
        {
        }
        public SystemRandom(int seed) : base(seed)
        {
        }

        public int NextInt32(int minInclusive, int maxExclusive)
        {
            return Next(minInclusive, maxExclusive);
        }
    }

    public class UnityRandom : IRandomInt32
    {
        public int NextInt32(int minInclusive, int maxExclusive)
        {
            return UnityEngine.Random.Range(minInclusive, maxExclusive);
        }
    }
}
