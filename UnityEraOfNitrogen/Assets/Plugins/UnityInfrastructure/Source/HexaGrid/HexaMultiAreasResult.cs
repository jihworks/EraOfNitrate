// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;

namespace Jih.Unity.Infrastructure.HexaGrid
{
    public class HexaMultiAreasResult
    {
        internal Queue<HexaMap.MultiAreaFrontier> Frontiers { get; } = new();

        /// <remarks>
        /// Key: Cell on the grid.<br/>
        /// Value: The owner starting cell of the key.<br/>
        /// <br/>
        /// Including starting cells with itself as value.
        /// </remarks>
        public Dictionary<HexaCell, HexaCell> CellToStartingCells { get; } = new();

        public int GetCells(HexaCell startingCell, List<HexaCell> buffer, bool includeStartingCell = false)
        {
            int count = 0;
            foreach (var pair in CellToStartingCells)
            {
                if (pair.Value != startingCell)
                {
                    continue;
                }

                if (pair.Key == startingCell)
                {
                    if (includeStartingCell)
                    {
                        buffer.Add(pair.Key);
                    }
                    continue;
                }

                buffer.Add(pair.Key);
                count++;
            }
            return count;
        }

        public void Clear()
        {
            Frontiers.Clear();
            CellToStartingCells.Clear();
        }
    }
}
