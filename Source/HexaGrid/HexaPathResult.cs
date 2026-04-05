// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.Unity.Infrastructure.Collections;
using System.Collections.Generic;

namespace Jih.Unity.Infrastructure.HexaGrid
{
    /// <summary>
    /// This class contains caches for path finding and the result path.
    /// </summary>
    /// <remarks>
    /// <c>NOT</c> thread-safe.
    /// </remarks>
    public class HexaPathResult
    {
        internal Dictionary<HexaCell, HexaCell> Connections { get; } = new();
        internal Dictionary<HexaCell, int> Costs { get; } = new();
        internal PriorityQueue<HexaCell, int> Frontiers { get; } = new();
        internal HashSet<HexaCell> Visited { get; } = new();

        public List<HexaCell> ResultPath { get; } = new();
        public bool IsSucceed { get; internal set; }

        public void Clear()
        {
            Connections.Clear();
            Costs.Clear();
            Frontiers.Clear();
            Visited.Clear();

            ResultPath.Clear();
            IsSucceed = false;
        }
    }
}
