// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;

namespace Jih.Unity.Infrastructure.HexaGrid
{
    public class HexaAreaResult
    {
        internal Queue<HexaCell> Frontiers { get; } = new();
        internal HashSet<HexaCell> Visited { get; } = new();

        public List<HexaCell> ResultCells { get; } = new();

        public void Clear()
        {
            Frontiers.Clear();
            Visited.Clear();
            ResultCells.Clear();
        }
    }
}
